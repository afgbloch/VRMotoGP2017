using UnityEngine;
using System.Collections;

//this is script contains all variables translated to bike's script.
//so, it's just a holder for all control variables
//mobile/keyboard scripts sends nums(float, int, bools) to this one

public class controlHub : MonoBehaviour  {//need that for leap motion controls

    public enum CameraMode { FIRST_PERSON = 0, THIRD_PERSON };
    public enum ControlMode {KEYBOARD_ONLY=0,  BODY_TILT, HAND_TILT}

    public ControlMode contolMode = ControlMode.KEYBOARD_ONLY; // tell if we use keyboard instead of VR controls (webcam and leapmotion)

    public float Vertical;//variable translated to bike script for bike accelerate/stop and leaning
	public float Horizontal;//variable translated to bike script for pilot's mass shift

    public float CamX;
    public float CamY;
    public float camSpeed = 10.0f;//Acceleration factor of the moving direction
    public bool camVrView;

	public bool restartBike;//this variable says to bike's script restart
	public bool fullRestartBike; //this variable says to bike's script to full restart
    
	public bool reverse;//for reverse speed

    public bool help; 

    public CameraMode cameraMode;

    public bool menuOn = false;

    void Start()
    {
        pauseResume(); 
    }


    public void pauseResume()
    {
        Time.timeScale = 0;
        menuOn = true;
        Application.LoadLevelAdditive(1); // 1 is menu in the build settings
    }

    public void switchCamera()
    {
        if (cameraMode == CameraMode.FIRST_PERSON)
        {
            cameraMode = CameraMode.THIRD_PERSON;
        }
        else if (cameraMode == CameraMode.THIRD_PERSON)
        {
            cameraMode = CameraMode.FIRST_PERSON;
        }
    }

    public void nextControlMode()
    {
        if (contolMode == ControlMode.KEYBOARD_ONLY)
            contolMode = ControlMode.BODY_TILT;
        else if (contolMode == ControlMode.BODY_TILT)
            contolMode = ControlMode.HAND_TILT;
        else if (contolMode == ControlMode.HAND_TILT)
            contolMode = ControlMode.KEYBOARD_ONLY; 
    }

}
