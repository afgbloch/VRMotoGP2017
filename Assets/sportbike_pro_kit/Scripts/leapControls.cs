using UnityEngine;
using System.Collections;
using Leap;

public class leapControls : MonoBehaviour {


	private GameObject ctrlHub;// making a link to corresponding bike's script
	private controlHub outsideControls;// making a link to corresponding bike's script
	Controller controller;
	Frame first;
	bool init = false;

	// Use this for initialization
	void Start () {
		ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
		outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script

		outsideControls.cameraMode = controlHub.CameraMode.FIRST_PERSON;
		outsideControls.help = false; 
	
		controller = new Controller ();
	}

	// Update is called once per frame
	void Update () {
		
		Frame frame = controller.Frame ();
		HandList hands = frame.Hands;
		Hand left = null, right = null;
		bool valid = false;
		outsideControls.restartBike = false;
		float speed = 0;

		if (hands.Count == 2) {
			if (hands [0].IsLeft) {
				left = hands [0];
				right = hands [1];
			} else {
				left = hands [1];
				right = hands [0];
			}
			valid = true;
		}

		if (valid) {

			speed -= left.GrabStrength;

			if (!init && right.GrabStrength == 1) {
				print ("---- INIT -----");
				first = frame;
				init = true;
			}

			if (init && right.GrabStrength == 0) {
				init = false;
			}
				

			if (init && right.GrabStrength == 1) {
				speed += right.RotationAngle (first);
			}
			
			if (speed > 1) {
				outsideControls.Vertical = 1;
			} else if (speed < -1) {
				outsideControls.Vertical = -1;
			} else {
				outsideControls.Vertical = speed;
			}
				

		} else {
			init = false;
		}

	}
}
