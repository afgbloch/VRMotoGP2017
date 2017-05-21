using UnityEngine;
using System.Collections;

public class keyboardControls : MonoBehaviour {


	private GameObject ctrlHub;// making a link to corresponding bike's script
	private controlHub outsideControls;// making a link to corresponding bike's script
	

	// Use this for initialization
	void Start () {
		ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
		outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script
    }
	
	// Update is called once per frame
	void Update () {

        // Open menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if(!outsideControls.menuOn)
                outsideControls.pauseResume();
        }

   
        // full restart
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


        // Display help
        if (Input.GetKeyDown(KeyCode.H))
        {
            outsideControls.help = !outsideControls.help;
        }


        //////////////////////// Activate or desactivate Reverse gear//////////////////////////////
        if (Input.GetKeyDown(KeyCode.C))
        {
            outsideControls.reverse = !outsideControls.reverse;
        }



        if (outsideControls.controlMode == controlHub.ControlMode.KEYBOARD_ONLY)
        {

            /////////////////////////////// ACCELERATE, braking, turning //////////////////////////////
            //to get less than 0.9 as acceleration to amelioate control of the bike
            outsideControls.Vertical = Input.GetAxis("Vertical") / 1.112f;

            if (Input.GetAxis("Vertical") < 0)
            {
                //need to get 1(full power) for front brake
                outsideControls.Vertical = outsideControls.Vertical * 1.112f;
            }

            outsideControls.Horizontal = Input.GetAxis("Horizontal");


            //Camera point of view and menu control with the mouse
            outsideControls.camVrView = Input.GetMouseButton(1);
            outsideControls.CamX = Input.GetAxis("Mouse X");
            outsideControls.CamY = Input.GetAxis("Mouse Y");

            outsideControls.menuClick = false;

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
