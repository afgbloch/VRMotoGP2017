using UnityEngine;
using System.Collections;


// Key board Controls class 
// Use the game control hub and set its variables with key board entries
// some commands are constrainted to keyboard only mode

public class KeyboardControls : MonoBehaviour {

	private GameObject ctrlHub;
	private ControlHub outsideControls;

	// Use this for keyboard initialization
	void Start () {
        
        // Get the gameScenario game object which contains the control hub
		ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();
    }
	
	// Update is called once per frame
	void Update () {

        // Open menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // only if the menu is not already open to avoid menu superposition
            if(!outsideControls.menuOn)
                outsideControls.pause();
        }

   
        // full restart the game
        if (Input.GetKey(KeyCode.R))
        {
            outsideControls.fullRestartBike = true;
        }
        
        
        // Change the mode of controle
        if (Input.GetKeyDown(KeyCode.M))
        {
            outsideControls.nextControlMode(); 
        }


        // Change the view Mode
        if (Input.GetKeyDown(KeyCode.V))
        {
            outsideControls.switchCamera();
        }


        // Display help tips
        if (Input.GetKeyDown(KeyCode.H))
        {
            outsideControls.help = !outsideControls.help;
        }


        // Activate or desactivate Reverse gear
        // Activate : take effect the next time the bike has zero speed
        // desactivate : take effect imediatelly 
        if (Input.GetKeyDown(KeyCode.C))
        {
            outsideControls.reverse = !outsideControls.reverse;
        }


        // The following commands are restricted to keyboard only mode to avoid
        // input conflicts with other controlers (i.e. leap motion or webcam)
        if (outsideControls.controlMode == ControlHub.ControlMode.KEYBOARD_ONLY)
        {

            // Accelerate (vertical > 0 )and braking (vertical < 0),  
            // Clamp to get less than 0.9 as acceleration to amelioate control of the bike
            outsideControls.vertical = Mathf.Clamp(Input.GetAxis("Vertical"), -1.0f, 0.9f );
            
            // Turning
            outsideControls.horizontal = Input.GetAxis("Horizontal");


            //Move the camera point of view and menu control with the mouse
            outsideControls.camVrView = Input.GetMouseButton(1);
            outsideControls.camX = Input.GetAxis("Mouse X");
            outsideControls.camY = Input.GetAxis("Mouse Y");

            // the mouse click is independant to the other control devices. 
            outsideControls.menuClick = false;

            // If the menu is open, compute the boolean for the display
            // Mainly calculate if the menu item must be selected or not
            if (outsideControls.menuOn)
            {
                outsideControls.menuStartStop = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 350 && Input.mousePosition.y > 350 - 40;


                outsideControls.menuFullRestart = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 300 && Input.mousePosition.y > 300 - 40;


                outsideControls.menuMode = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 250 && Input.mousePosition.y > 250 - 40;


                outsideControls.menuView = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 200 && Input.mousePosition.y > 200 - 40;


                outsideControls.menuHelp = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 150 && Input.mousePosition.y > 150 - 40;

                outsideControls.menuExit = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
                    && Input.mousePosition.y < 100 && Input.mousePosition.y > 100 - 40;
            }
        }
    }
}
