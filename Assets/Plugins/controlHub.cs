using UnityEngine;
using System.Collections;

// this script contains all control variables needed to control the game.
// so, it's just a holder for all control variables and some useful command method
// keyboard and leap motion scripts set value in this one

public class ControlHub : MonoBehaviour  {

    // Contstant String value in a humain readable format for GUI display
    public readonly string[] CONTROL_MODE = { "Mode: keyboard", "Mode: body tilt", "Mode: hand tilt" };
    public readonly string[] VIEW_MODE = { "First Person", "Third Person" };
    public readonly string[] HELP_MODE = { "Help: OFF", "Help: ON" };

    // Enum containing the view modes
    public enum CameraMode { FIRST_PERSON = 0, THIRD_PERSON };
    public CameraMode cameraMode = CameraMode.FIRST_PERSON; // set by default to first person camera

    // Enum containing the control modes
    public enum ControlMode { KEYBOARD_ONLY=0,  BODY_TILT, HAND_TILT }
    public ControlMode controlMode = ControlMode.KEYBOARD_ONLY; // set by default to keyboard only
    
    // Axis value controling the acceleration/breaking and turning value
    public float vertical = -1; // between -1 (full breaks) and  +1 (full acceleration), 0 is neutral
	public float horizontal = 0; // between -1 (full left) and +1 (full right), 0 is neutral

    // Camera Variable
    public float camX;
    public float camY;
    public float camSpeed = 10.0f; //Acceleration factor of the moving direction
    public bool camVrView;

    // Menu boolean for click selection
    public bool menuStartStop;
    public bool menuFullRestart;
    public bool menuMode;
    public bool menuView;
    public bool menuHelp;
    public bool menuExit;
    public bool menuClick;

    // this variable say to bike script to be reinitialized
    // reset the bike at default position and with default value
    public bool fullRestartBike; 
    
    // indicate if the rear gear is in or not
	public bool reverse = false;

    // Indicate if we want to diplay the help
    public bool help = false; 

    // Indicate if the menu is open or not
    public bool menuOn = false;

    void Start()
    {
       pause(); 
    }


    // Put the game in pause mode. Any controler can call this methode
    // with its corresponding mapping command. 
    public void pause()
    {
        // Set the time scale to 0 to simulate pause
        Time.timeScale = 0;
        // indicate the menu as open
        menuOn = true;
        // Load a menu scene (1 is menu in the build settings)
        Application.LoadLevelAdditive(1);
    }

    // Switch from one camera to the next one. Any controler can call this methode
    // with its corresponding mapping command. 
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

    // Switch from one control mode to the next one. Any controler can call this methode
    // with its corresponding mapping command. 
    public void nextControlMode()
    {
        if (controlMode == ControlMode.KEYBOARD_ONLY)
        {
            controlMode = ControlMode.BODY_TILT;
        }
        else if (controlMode == ControlMode.BODY_TILT)
        {
            controlMode = ControlMode.HAND_TILT;
        }
        else if (controlMode == ControlMode.HAND_TILT)
        {
            controlMode = ControlMode.KEYBOARD_ONLY;
        }
    }

}
