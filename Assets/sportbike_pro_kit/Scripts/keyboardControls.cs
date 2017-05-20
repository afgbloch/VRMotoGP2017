using UnityEngine;
using System.Collections;

public class keyboardControls : MonoBehaviour {


	private GameObject ctrlHub;// making a link to corresponding bike's script
	private controlHub outsideControls;// making a link to corresponding bike's script
	

	// Use this for initialization
	void Start () {
		ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
		outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script

        outsideControls.cameraMode = controlHub.CameraMode.FIRST_PERSON;
        outsideControls.help = false; 
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            outsideControls.pauseResume();
        }
        


        // Change the mode of controle
        if (Input.GetKeyDown(KeyCode.M))
        {
            outsideControls.nextControlMode(); 
        }


        /////////////////////////////// ACCELERATE, braking, turning //////////////////////////////
        //to get less than 0.9 as acceleration to prevent wheelie(wheelie begins at >0.9)
        if (outsideControls.contolMode == controlHub.ControlMode.KEYBOARD_ONLY)
        {
            outsideControls.Vertical = Input.GetAxis("Vertical") / 1.112f;
            
            if (Input.GetAxis("Vertical") < 0)
            {
                //need to get 1(full power) for front brake
                outsideControls.Vertical = outsideControls.Vertical * 1.112f;
            }

            outsideControls.Horizontal = Input.GetAxis("Horizontal");
        }
        


        /////////////////////////////////// Restart ////////////////////////////////////////////////
        // Restart & full restart
        if (Input.GetKey (KeyCode.R)) {
			outsideControls.restartBike = true;
		} else
			outsideControls.restartBike = false;

		// RightShift for full restart
		if (Input.GetKey (KeyCode.RightShift)) {
			outsideControls.fullRestartBike = true;
		} else
			outsideControls.fullRestartBike = false;

		////////////////////////////////// Reverse //////////////////////////////////////////////////
		if(Input.GetKeyDown(KeyCode.C)){
				outsideControls.reverse = !outsideControls.reverse;
		} 

        /////////////////////////////////// Help ////////////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.H))
        {
            outsideControls.help = !outsideControls.help;
        }

        /////////////////////////////////// Change View //////////////////////////////////////////////
        if (Input.GetKeyDown(KeyCode.V))
        {
            outsideControls.switchCamera(); 
        }

        if (outsideControls.contolMode == controlHub.ControlMode.KEYBOARD_ONLY)
        {
            outsideControls.camVrView = Input.GetMouseButton(1);
            outsideControls.CamX = Input.GetAxis("Mouse X");
            outsideControls.CamY = Input.GetAxis("Mouse Y");
        }
        
    }
}
