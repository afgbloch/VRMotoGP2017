using UnityEngine;
using System.Collections;

public class keyboardControls : MonoBehaviour {


	private GameObject ctrlHub;// making a link to corresponding bike's script
	private controlHub outsideControls;// making a link to corresponding bike's script
	

	// Use this for initialization
	void Start () {
		ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
		outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script

        outsideControls.cameraMode = controlHub.CameraMode.THIRD_PERSON;
        outsideControls.help = false; 
    }
	
	// Update is called once per frame
	void Update () {
        /////////////////////////////// ACCELERATE, braking, turning //////////////////////////////
        //to get less than 0.9 as acceleration to prevent wheelie(wheelie begins at >0.9)
        outsideControls.Vertical = Input.GetAxis ("Vertical") / 1.112f;

        //need to get 1(full power) for front brake
        if (Input.GetAxis("Vertical") < 0)
        {
            outsideControls.Vertical = outsideControls.Vertical * 1.112f;
        }

        outsideControls.Horizontal = Input.GetAxis("Horizontal");
		
        
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
            if(outsideControls.cameraMode == controlHub.CameraMode.FIRST_PERSON)
            {
                outsideControls.cameraMode = controlHub.CameraMode.THIRD_PERSON;
            }
            else if (outsideControls.cameraMode == controlHub.CameraMode.THIRD_PERSON)
            {
                outsideControls.cameraMode = controlHub.CameraMode.FIRST_PERSON;
            }
        }
        

    }
}
