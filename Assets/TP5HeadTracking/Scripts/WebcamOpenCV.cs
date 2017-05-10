/// <summary>
/// WebcamOpenCV.cs
/// capture webcam input and transform to CvMat object.
/// part of the code has been extracted from the following sample:
/// https://sourceforge.net/projects/unityopencvsharpcamshift/?source=typ_redirect
/// Henrique Debarba - 2016
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OpenCvSharp;

// Parallel is used to speed up the for loop when converting CvMat to 2D texture
using Uk.Org.Adcock.Parallel;  // Stewart Adcock's implementation of parallel processing 

// 
[System.Serializable]
public class Webcam {

	public WebCamTexture camTexture { get; private set;}     //Texture retrieved from the webcam

	// you can set some of the capture parameters of the webcam 
	// this can be used to speed up app
	// -1 is used to bypass these settings
	public float camFrameRate = -1;	// desired webcam frame rate
	public int camWidth = -1;	// desired webcam img width
	public int camHeight = -1;	// desired webcam img height
	public float camFOV = 45.0f;	// vertical field of view of the webcam

	[HideInInspector]
	public Transform imgPlane;		// used to render the webcam image

	// Flip the video source axes (webcams are usually mirrored)
	// Unity and OpenCV images are flipped
	public bool flipUpDownAxis = false, flipLeftRightAxis = true;

	
	public bool playing { get; private set;} 

	public bool InitWebcam (){
		playing = false;
		WebCamDevice[] devices = WebCamTexture.devices;
		
		if (devices.Length > 0) {   // If there is at least one camera
			
			camTexture = new WebCamTexture (devices [0].name);  // Grab first camera
			
			// set requested webcam parameters (if the provided parameters are valid)
			camTexture.requestedFPS = (camFrameRate > 0) ? camFrameRate : 0;
			camTexture.requestedHeight = (camHeight > 0) ? camHeight : 0;
			camTexture.requestedWidth = (camWidth > 0) ? camWidth : 0;
			
			camTexture.Play ();  // Play the video source
			
			camHeight = camTexture.height;
			camWidth = camTexture.width;

			// GameObject where the webcam image stream will be rendered
			imgPlane = GameObject.CreatePrimitive(PrimitiveType.Quad).transform;
			imgPlane.name = "imgPlane";
			imgPlane.position = Vector3.forward;

			// Attach camera to texture of the gameObject
			imgPlane.GetComponent<Renderer>().material.mainTexture = camTexture; 
			// Un-mirror the webcam image
			Vector3 camLScale = imgPlane.transform.localScale;
			camLScale.x = flipLeftRightAxis ? -camLScale.x : camLScale.x;
			camLScale.y = flipUpDownAxis ? -camLScale.y : camLScale.y;
			imgPlane.transform.localScale = camLScale;


			playing = true;
			return true;

		} else {
			return false;
		}

	}
}

[System.Serializable]
public class OCVCam {
	// OpenCV video parameters
	public CvMat cvMat  { get; private set; }

	// reductionFactor reduces img resolution when converting from webcam to CvMat 
	// this should be used in case the program is running slow
	//private int _reductionFactor = 2;
	public int reductionFactor = 2;//{ get { return _reductionFactor; } }

	// use the parallel implementation to convert cam to cv image
	public bool parallelConvertion = true;
	private Color32[] pixels;	// buffer used in the cam to cv conversion

	private Webcam wc;

	private bool ready = false;
	


	public bool InitOCVCam (Webcam webcam){
		wc = webcam;
		if (!wc.playing)
			return false;

		// Get the video source image width and height
		int cvWidth = Cv.Round (wc.camTexture.width / reductionFactor);
		int cvHeight = Cv.Round (wc.camTexture.height / reductionFactor);
		
		// Create standard CvMat image based on web camera video input
		// 1 channel for grayscale image with unsigned 8-bit depth of gray values
		//cvMat = new CvMat (cvHeight, cvWidth, MatrixType.U8C3);
		cvMat = new CvMat (cvHeight, cvWidth, MatrixType.U8C1);
		ready = true;
		return true;
	}

	public bool UpdateOCVMat (){
		if (ready && !wc.playing)
			return false;
		Texture2DToCvMat ();
		return true;
	}

	// Converts the Texture2D type of Unity to OpenCV's CvMat
	// This may use Adcock's parallel C# code to parallelize the conversion and make it faster
	void Texture2DToCvMat ()
	{
		if (wc.camTexture.didUpdateThisFrame){
			pixels = wc.camTexture.GetPixels32 ();
			
			if (parallelConvertion) {
				Parallel.For (0, cvMat.Height, i =>
				              {
					for (var j = 0; j < cvMat.Width; j ++) {	
						var pixel = pixels [j * reductionFactor + (i * reductionFactor) * (cvMat.Width * reductionFactor)];
						
						// RGB to gray scale
						cvMat.Set2D (i, j, (double)pixel.r * .299 + (double)pixel.g * .587 + (double)pixel.b * .114);
					}
				});
				
			} else {		
				Color pixel;
				
				// Non-parallelized code
				for (int i = 0; i < cvMat.Height; i++) {
					for (int j = 0; j < cvMat.Width; j++) {
						pixel = pixels [j * reductionFactor + (i * reductionFactor) * (cvMat.Width * reductionFactor)];
						
						// RGB to gray scale
						cvMat.Set2D (i, j, (double)pixel.r * .299 + (double)pixel.g * .587 + (double)pixel.b * .114);
					}
				}
			}
			
			// Flip up/down dimension and right/left dimension
			if (!wc.flipUpDownAxis && wc.flipLeftRightAxis)
				Cv.Flip (cvMat, cvMat, FlipMode.XY);
			else if (!wc.flipUpDownAxis)
				Cv.Flip (cvMat, cvMat, FlipMode.X);
			else if (wc.flipLeftRightAxis)
				Cv.Flip (cvMat, cvMat, FlipMode.Y);
		}
	}

}


