using UnityEngine;
using System.Collections;
using OpenCvSharp;

// Parallel is used to speed up the for loop when converting a 2D texture to CvMat
using Uk.Org.Adcock.Parallel;

public class HeadTracking : MonoBehaviour
{

    // unity cam to match our physical webcam
    public Camera camUnity;
    // distance between camUnity and webcam imgPlane
    float wcImgPlaneDist = 1.0f;

    // scaleFactor defines the spatial resolution for distance from camera
    [Range(1.01f, 2.5f)]
    public double scaleFactor = 1.1;
    CvHaarClassifierCascade cascade;
    CvMemStorage storage = new CvMemStorage();

    // approx. size of a face
    float knownFaceSize = 0.15f;
    // object to be controlled with head position
    public Transform controlledTr;

    Vector3 priorPos = new Vector3();
    public V3DoubleExpSmoothing posSmoothPred = new V3DoubleExpSmoothing();


    // webcam video
    public Webcam wc = new Webcam();
    // opencv video
    public OCVCam ocv = new OCVCam();

    // Initialization
    void Start()
    {
        // init webcam capture and render to plane
        if (wc.InitWebcam())
        {
            // init openCV image converter
            ocv.InitOCVCam(wc);

        }
        else
        {
            Debug.LogError("[WebcamOpenCV.cs] no camera device has been found! " +
                            "Certify that a camera is connected and its drivers " +
                            "are working.");
            return;
        }


        // make sure that a camera has been provided,
        // or that this GameObject has a camera component
        if (camUnity == null)
        {
            Camera thisCam = this.GetComponent<Camera>();
            if (thisCam != null)
                camUnity = thisCam;
            else
                Debug.LogError("[WebcamOpenCV.cs] there is no camera component " +
                                "in the GameObject where this script is attached");
        }

        // webcam and camUnity must have matching FOV
        camUnity.fieldOfView = wc.camFOV;
        FitPlaneIntoFOV(wc.imgPlane);

        // load the calibration for the Haar classifier cascade
        // docs.opencv.org/3.1.0/d7/d8b/tutorial_py_face_detection.html
        //cascade = CvHaarClassifierCascade.FromFile("./haarcascade_frontalface_alt.xml");
        // Maybe use Application.dataPath to make the path valid for all platform
        cascade = CvHaarClassifierCascade.FromFile("Assets/TP5HeadTracking/haarcascade_frontalface_alt.xml");

        // scale controlled object to match face size
        controlledTr.localScale = knownFaceSize * Vector3.one;

    }

    // Update is called once per frame
    void Update()
    {
        ocv.UpdateOCVMat();
        //TrackHeadOverImgPlane(); // point 4
        //TrackHead3D (); // point 6
        TrackHead3DSmooth (); // point 7
        
    }



    void FitPlaneIntoFOV(Transform wcImgPlane)
    {

        wcImgPlane.parent = camUnity.transform;

        // set plane position and orientation facing camUnity
        wcImgPlane.rotation = Quaternion.LookRotation(camUnity.transform.forward,
                                                      camUnity.transform.up);
        wcImgPlane.position = camUnity.transform.position +
                              wcImgPlaneDist * camUnity.transform.forward;

        // Fit the imgPlane into the unity camera FOV 
        // compute vertical imgPlane size from FOV angle
        float vScale = AngularSize.GetSize(wcImgPlaneDist, camUnity.fieldOfView);
        // set the scale 
        Vector3 wcPlaneScale = wcImgPlane.localScale;
        float ratioWH = ((float)wc.camWidth / (float)wc.camHeight);
        wcPlaneScale.x = ratioWH * vScale * wcPlaneScale.x;
        wcPlaneScale.y = vScale * wcPlaneScale.y;
        wcImgPlane.localScale = wcPlaneScale;
    }

    bool HaarClassCascade(ref Vector3 cvTrackedPos)
    {
        CvMemStorage storage = new CvMemStorage();
        storage.Clear();

        // define minimum head size to 1/10 of the img width
        int minSize = ocv.cvMat.Width / 10;

        // run the Haar detector algorithm
        // docs.opencv.org/3.1.0/d7/d8b/tutorial_py_face_detection.html
        CvSeq<CvAvgComp> faces =
            Cv.HaarDetectObjects(ocv.cvMat, cascade, storage, scaleFactor, 2,
                                  0 | HaarDetectionType.ScaleImage,
                                  new CvSize(minSize, minSize));

        // if faces have been found ....
        if (faces.Total > 0)
        {
            // rectangle defining face 1
            CvRect r = faces[0].Value.Rect;
            // approx. eye center for x,y coordinates
            cvTrackedPos.x = r.X + r.Width * 0.5f;
            cvTrackedPos.y = r.Y + r.Height * 0.3f;
            // approx. the face diameter based on the rectangle size
            cvTrackedPos.z = (r.Width + r.Height) * 0.5f;

            return true;    // YES, we found a face!
        }
        else
            return false;   // no faces in this frame
    }

    public Vector3 CvMat2ScreenCoord(Vector3 cvPos)
    {
        // rescale x,y position from cvMat coordinates to screen coordinates 
        cvPos.x = ((float)camUnity.pixelWidth / (float)ocv.cvMat.Width) * cvPos.x;
        // swap the y coordinate origin and +y direction 
        cvPos.y = ((float)camUnity.pixelHeight / (float)ocv.cvMat.Height) * (ocv.cvMat.Height - cvPos.y);

        return cvPos;
    }

    void TrackHeadOverImgPlane()
    {
        Vector3 cvHeadPos = new Vector3();
        // find head position in the cvMat coordinates 
        if (HaarClassCascade(ref cvHeadPos))
        {

            // Set position of the controlledTr object
            // x,y cvMat coordinates to screen coordinates
            Vector3 cTrPos = CvMat2ScreenCoord(cvHeadPos);
            // z is fixed at the wcImgPlane depth
            cTrPos.z = wcImgPlaneDist;
            // x,y scree coordinates to world coordinates 
            cTrPos = camUnity.ScreenToWorldPoint(cTrPos);
            controlledTr.position = cTrPos;


            // Scale controlledTr to fit the tracked head size
            // tracked head size proportional to cvMat image
            float relHeadSize = cvHeadPos.z / ocv.cvMat.Height;
            // scale x,y proportional to the plane used for render
            Vector3 cTrScale = controlledTr.localScale;
            cTrScale.x = relHeadSize * wc.imgPlane.localScale.y;
            cTrScale.y = cTrScale.x;
            controlledTr.localScale = cTrScale;

        }
    }

    void TrackHead3D()
    {
        Vector3 cvHeadPos = new Vector3();

        // find head position in the cvMat coordinates 
        if (HaarClassCascade(ref cvHeadPos))
        {

            // Set position of the controlledTr object
            // compute the face angular size
            float faceAng = AngularSize.GetAngSize(wcImgPlaneDist, knownFaceSize);
            // compute the face angle to camera angle ratio 
            float faceHeightRatio = faceAng / camUnity.fieldOfView;
            // approximate face distance in world coordinates
            cvHeadPos.z = ((faceHeightRatio * (float)ocv.cvMat.Height) /
                          (float)cvHeadPos.z) * wcImgPlaneDist;
            // x,y cvMat coordinates to screen coordinates
            cvHeadPos = CvMat2ScreenCoord(cvHeadPos);
            // x,y screen coordinates to world coordinates 
            cvHeadPos = camUnity.ScreenToWorldPoint(cvHeadPos);

            controlledTr.position = cvHeadPos;
        }
    }

    void TrackHead3DSmooth()
    {
        Vector3 cvHeadPos = new Vector3();
        if (HaarClassCascade(ref cvHeadPos))
        {

            float faceAng = AngularSize.GetAngSize(wcImgPlaneDist, knownFaceSize);
            float faceHeightRatio = faceAng / camUnity.fieldOfView;
            cvHeadPos.z = ((faceHeightRatio * (float)ocv.cvMat.Height) /
                          (float)cvHeadPos.z) * wcImgPlaneDist;
            cvHeadPos = CvMat2ScreenCoord(cvHeadPos);
            cvHeadPos = camUnity.ScreenToWorldPoint(cvHeadPos);

            // the tracking is noisy, thus we only consider the new reading if it  
            // lands less than .4 meters away from the last smoothed position
            if ((cvHeadPos - priorPos).magnitude < 0.4f)
                priorPos = cvHeadPos;
        }

        // update the smoothing / prediction model
        posSmoothPred.UpdateModel(priorPos);
        // update the position of unity object
        controlledTr.position = posSmoothPred.StepPredict();
    }

}