using UnityEngine;
using System.Collections;
using Leap;
using OpenCvSharp;

public class leapControls : MonoBehaviour {

	private GameObject ctrlHub;// making a link to corresponding bike's script
	private controlHub outsideControls;// making a link to corresponding bike's script
	Controller controller;
	Frame first;
	bool init = false;
    // webcam video
    public Webcam wc = new Webcam();
    // opencv video
    public OCVCam ocv = new OCVCam();
    CvHaarClassifierCascade cascade;
    // scaleFactor defines the spatial resolution for distance from camera
    [Range(1.01f, 2.5f)]
    public double scaleFactor = 1.1;
    // unity cam to match our physical webcam
    public Camera camUnity;
    // distance between camUnity and webcam imgPlane
    float wcImgPlaneDist = 1.0f;
    // approx. size of a face
    float knownFaceSize = 0.15f;
    Vector3 priorPos = new Vector3();
    public V3DoubleExpSmoothing posSmoothPred = new V3DoubleExpSmoothing();
    // object to be controlled with head position
    public Transform controlledTr;
    Vector2 oldV;
    Vector oldMenuVector = new Vector();

    // Use this for initialization
    void Start () {
		ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
		outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script

		outsideControls.cameraMode = controlHub.CameraMode.FIRST_PERSON;
		outsideControls.help = false; 
	
		controller = new Controller ();

        // init webcam capture and render to plane
        if (wc.InitWebcam()) {
            // init openCV image converter
            ocv.InitOCVCam(wc);
        } else {
            Debug.LogError("[WebcamOpenCV.cs] no camera device has been found! " +
                            "Certify that a camera is connected and its drivers " +
                            "are working.");
            return;
        }

        // webcam and camUnity must have matching FOV
        camUnity.fieldOfView = wc.camFOV;
        FitPlaneIntoFOV(wc.imgPlane);


        cascade = CvHaarClassifierCascade.FromFile("Assets/HeadTracking/haarcascade_frontalface_alt.xml");

        // scale controlled object to match face size
        controlledTr.localScale = knownFaceSize * Vector3.one;

        if (outsideControls.controlMode != controlHub.ControlMode.KEYBOARD_ONLY) {
            outsideControls.camSpeed = 500.0f;
            outsideControls.camVrView = true;
        }

    }
    
    // Update is called once per frame
    void Update () {

        if (outsideControls.controlMode != controlHub.ControlMode.KEYBOARD_ONLY) {
            Frame frame = controller.Frame();
            HandList hands = frame.Hands;
            Hand left = null, right = null;
            bool valid = false;
            outsideControls.restartBike = false;
            float speed = 0;

            if (hands.Count > 0)
            {
                if (hands[0].IsLeft)
                {
                    left = hands[0];
                }
                else
                {
                    right = hands[0];
                }
            }

            if (hands.Count > 1)
            {
                if (hands[1].IsLeft)
                {
                    left = hands[1];
                }
                else
                {
                    right = hands[1];
                }
            }

            if(left != null && right != null)
            {
                valid = true;
            }

            if (right != null && outsideControls.menuOn)
            {

                Vector menuV = hands[0].PalmPosition;

                outsideControls.menuStartStop = menuV.y >= 150;
                outsideControls.menuMode = 75 < menuV.y && menuV.y < 150;
                outsideControls.menuExit = menuV.y <= 75;


                float delta = oldMenuVector.z - menuV.z;
                if (delta > 45)
                {
                    outsideControls.menuClick = true;
                }
                
                oldMenuVector = menuV;
            }

            if (valid) {

                speed -= left.GrabStrength;

                if (!init && right.GrabStrength == 1) {
                    print("---- INIT -----");
                    first = frame;
                    init = true;
                }

                if (init && right.GrabStrength == 0) {
                    init = false;
                }


                if (init && right.GrabStrength == 1) {
                    speed += right.RotationAngle(first);
                }

                speed = speed / 2.0f;

                if (speed > 0.9f) {
                    outsideControls.Vertical = 0.9f;
                } else if (speed < -0.9f) {
                    outsideControls.Vertical = -0.9f;
                } else {
                    outsideControls.Vertical = speed;
                }

                if (outsideControls.controlMode == controlHub.ControlMode.HAND_TILT) {
                    Vector leftV = left.PalmPosition;
                    Vector rightV = right.PalmPosition;
                    float tilt = (rightV.z - leftV.z) / 300.0f;

                    //print("Tilt:" + tilt + " lz:" + leftV.z + " rz:" + rightV.z);

                    if (tilt > 0.9f)
                    {
                        outsideControls.Horizontal = 0.9f;
                    }
                    else if (tilt < -0.9f)
                    {
                        outsideControls.Horizontal = -0.9f;
                    }
                    else
                    {
                        outsideControls.Horizontal = tilt;
                    }
                }

                if(left.PalmNormal.y > 0.9 && right.PalmNormal.y > 0.9 && !outsideControls.menuOn)
                {
                    outsideControls.pauseResume();
                }
            } else {
                init = false;
            }

            ocv.UpdateOCVMat();
            TrackHead3DSmooth();
        }
    }

    void TrackHead3DSmooth() {

        Vector3 cvHeadPos = new Vector3();

        if (HaarClassCascade(ref cvHeadPos)) {

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
        Vector3 v = posSmoothPred.StepPredict();

        if (outsideControls.controlMode == controlHub.ControlMode.BODY_TILT) {
            float tilt = v.x * 3;

            if (tilt > 0.9f)
            {
                outsideControls.Horizontal = 0.9f;
            }
            else if (tilt < -0.9f)
            {
                outsideControls.Horizontal = -0.9f;
            }
            else
            {
                outsideControls.Horizontal = tilt;
            }
        }

        if (outsideControls.controlMode == controlHub.ControlMode.HAND_TILT) {
            outsideControls.CamX = v.x - oldV.x;
            outsideControls.CamY = v.y - oldV.y;
            oldV = v;
        }
    }

    bool HaarClassCascade(ref Vector3 cvTrackedPos) {
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
        if (faces.Total > 0) {
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

    public Vector3 CvMat2ScreenCoord(Vector3 cvPos) {
        // rescale x,y position from cvMat coordinates to screen coordinates 
        cvPos.x = ((float)camUnity.pixelWidth / (float)ocv.cvMat.Width) * cvPos.x;
        // swap the y coordinate origin and +y direction 
        cvPos.y = ((float)camUnity.pixelHeight / (float)ocv.cvMat.Height) * (ocv.cvMat.Height - cvPos.y);

        return cvPos;
    }

    void FitPlaneIntoFOV(Transform wcImgPlane) {

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
}
