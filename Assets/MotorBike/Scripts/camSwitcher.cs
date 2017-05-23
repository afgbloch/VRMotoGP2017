using UnityEngine;
using System.Collections;


// Camera Switcher class
// This class deal with the two bike camera (first and third person)
// 
//

public class CamSwitcher : MonoBehaviour
{
    // The current camera is one of the following
    private Camera currentCamera;
    
    //////////////////// First Person Camera///////////////////////////////////////////////
    public Camera firstPersonCamera;
    public Transform firstPersonCameraTarget;
    private float maxLeft = -60;//Limit Left angle
    private float maxRight = 60; //Limit Right angle
    private float maxUp = -60; // Limit Up angle
    private float maxDown = 10; // Limit Down angle
    private float x = 0.0f; //Current horizontal angle
    private float y = 0.0f; // current Vertical angle


    //////////////////// Third Person Camera ///////////////////////////////////////////////
    public Camera thirdPersonCamera;
    public Transform thirdPersonCameraTarget;
    float distThirdPersonCamera = 3.0f;
	float heightThirdPersonCamera = 1.0f;


    //Bonus - Camera behaviour by Boris Chuprin
    private float currentTargetAngle;


    private GameObject ctrlHub;
    private ControlHub outsideControls;
	

	/////////////////////// Initialization ////////////////////////////////////////////////////
    void Start ()
	{
        // Get the gameScenario game object which contains the control hub
        ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();


        //By default first person camera is Active, Enabled and so current
        firstPersonCamera.enabled = true;
        thirdPersonCamera.enabled = false;
        firstPersonCamera.gameObject.SetActive(true);
        thirdPersonCamera.gameObject.SetActive(false);
        currentCamera = firstPersonCamera;
        currentTargetAngle = firstPersonCameraTarget.transform.eulerAngles.z;
	}

    /////////////////////// Update once per frame///////////////////////////////////////////////
    void LateUpdate ()
	{

        // Deal either with the first person camera or the third person camera 

        if (outsideControls.cameraMode == ControlHub.CameraMode.FIRST_PERSON)
        {
            // Change of camera mode if not already done
            if (thirdPersonCamera.enabled)
            {
                thirdPersonCamera.enabled = false;
                firstPersonCamera.enabled = true;
                thirdPersonCamera.gameObject.SetActive(false);
                firstPersonCamera.gameObject.SetActive(true);
                currentCamera = firstPersonCamera;
            }

            // Control of field of view for inertiel effect and restrict it 
            firstPersonCamera.fieldOfView = firstPersonCamera.fieldOfView + outsideControls.vertical * 20f * Time.deltaTime;
            if (firstPersonCamera.fieldOfView > 95)
            {
                firstPersonCamera.fieldOfView = 95;
            }
            if (firstPersonCamera.fieldOfView < 75)
            {
                firstPersonCamera.fieldOfView = 75;
            }
            if (firstPersonCamera.fieldOfView < 85)
            {
                firstPersonCamera.fieldOfView = firstPersonCamera.fieldOfView += 10f * Time.deltaTime;
            }
            if (firstPersonCamera.fieldOfView > 85)
            {
                firstPersonCamera.fieldOfView = firstPersonCamera.fieldOfView -= 10f * Time.deltaTime;
            }

            // Orrientate the camera correctly 
            float currentRotationAngle = firstPersonCameraTarget.eulerAngles.y;
            float currentHeight = firstPersonCameraTarget.position.y;

            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
            currentCamera.transform.position = firstPersonCameraTarget.position;
            currentCamera.transform.position = new Vector3(currentCamera.transform.position.x, currentHeight, currentCamera.transform.position.z);
            currentCamera.transform.LookAt(firstPersonCameraTarget);

            //Bonus - Camera behaviour by Boris Chuprin : rotate camera according with bike leaning
            if (firstPersonCameraTarget.transform.eulerAngles.z > 0 && firstPersonCameraTarget.transform.eulerAngles.z < 180)
            {
                currentTargetAngle = firstPersonCameraTarget.transform.eulerAngles.z / 10;
            }
            if (firstPersonCameraTarget.transform.eulerAngles.z > 180)
            {
                currentTargetAngle = -(360 - firstPersonCameraTarget.transform.eulerAngles.z) / 10;
            }
            currentCamera.transform.rotation = Quaternion.Euler(0.0f, currentRotationAngle, currentTargetAngle);

            ///////////////////////////////////////////////////////////////////////////////////////////
            // Orientate the point of view in first person. 
            ///////////////////////////////////////////////////////////////////////////////////////////
            if (outsideControls.camVrView)
            {
                float speedFactor = outsideControls.camSpeed;

                x += outsideControls.camX * speedFactor;
                y -= outsideControls.camY * speedFactor;

                // Current X is the current horizonatal angle of the bike. 
                float currentX = currentCamera.transform.localRotation.eulerAngles.y;
                
                // Clamp the fov direction considering their Limit
                x = Mathf.Clamp(x, maxLeft, maxRight);
                y = Mathf.Clamp(y, maxUp, maxDown);
  
                currentCamera.transform.localRotation = Quaternion.Euler(y, currentX + x, 0);
                currentCamera.transform.position = firstPersonCameraTarget.position;
            }else{
                x = 0.0f;
                y = 0.0f; 
            }
            ///////////////////////////////////////////////////////////////////////////////////////////

        } else if (outsideControls.cameraMode == ControlHub.CameraMode.THIRD_PERSON){

            // Change of camera mode if not already done
            if (firstPersonCamera.enabled)
            {
                thirdPersonCamera.enabled = true;
                firstPersonCamera.enabled = false;
                thirdPersonCamera.gameObject.SetActive(true);
                firstPersonCamera.gameObject.SetActive(false);
                currentCamera = thirdPersonCamera;
            }

            // Control of field of view for inertiel effect and restrict it 
            thirdPersonCamera.fieldOfView = thirdPersonCamera.fieldOfView + outsideControls.vertical * 20f * Time.deltaTime;
            if (thirdPersonCamera.fieldOfView > 85)
            {
                thirdPersonCamera.fieldOfView = 85;
            }
            if (thirdPersonCamera.fieldOfView < 50)
            {
                thirdPersonCamera.fieldOfView = 50;
            }
            if (thirdPersonCamera.fieldOfView < 60)
            {
                thirdPersonCamera.fieldOfView = thirdPersonCamera.fieldOfView += 10f * Time.deltaTime;
            }
            if (thirdPersonCamera.fieldOfView > 60)
            {
                thirdPersonCamera.fieldOfView = thirdPersonCamera.fieldOfView -= 10f * Time.deltaTime;
            }

            // Orrientate the camera correctly 
            float wantedRotationAngle = thirdPersonCameraTarget.eulerAngles.y;
            float wantedHeight = thirdPersonCameraTarget.position.y + heightThirdPersonCamera;
            float currentRotationAngle = currentCamera.transform.eulerAngles.y;
            float currentHeight = currentCamera.transform.position.y;

            currentRotationAngle = Mathf.LerpAngle(currentRotationAngle, wantedRotationAngle, 3 * Time.deltaTime);
            currentHeight = Mathf.Lerp(currentHeight, wantedHeight, 2 * Time.deltaTime);

            Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);
            currentCamera.transform.position = thirdPersonCameraTarget.position;
            currentCamera.transform.position -= currentRotation * Vector3.forward * distThirdPersonCamera;
            currentCamera.transform.position = new Vector3(currentCamera.transform.position.x, currentHeight, currentCamera.transform.position.z);
            currentCamera.transform.LookAt(thirdPersonCameraTarget);

            //Bonus - Camera behaviour by Boris Chuprin : rotate camera according with bike leaning
            if (thirdPersonCameraTarget.transform.eulerAngles.z > 0 && thirdPersonCameraTarget.transform.eulerAngles.z < 180)
            {
                currentTargetAngle = thirdPersonCameraTarget.transform.eulerAngles.z / 10;
            }
            if (thirdPersonCameraTarget.transform.eulerAngles.z > 180)
            {
                currentTargetAngle = -(360 - thirdPersonCameraTarget.transform.eulerAngles.z) / 10;
            }
            currentCamera.transform.rotation = Quaternion.Euler(heightThirdPersonCamera * 10, currentRotationAngle, currentTargetAngle);   
        }
    }
}