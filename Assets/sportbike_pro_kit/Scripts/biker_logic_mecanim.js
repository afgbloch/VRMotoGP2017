#pragma strict
///////////////////////////////////////////////////////////////////////////////////////////////////
/// Writen by Boris Chuprin smokerr@mail.ru
/// And adapted by Bastien Chatelain and Aurelien Bloch
///////////////////////////////////////////////////////////////////////////////////////////////////
private var myAnimator : Animator;

// variables for turn IK link off for a time
private var IK_rightWeight : int = 1;
private var IK_leftWeight : int = 1;

//variables for moving right/left and forward/backward
private var bikerLeanAngle : float = 0.0;
private var bikerMoveAlong : float = 0.0;

//variables for moving reverse animation
private var reverseSpeed : float = 0.0;

// standard point of interest for a head
//var camPoint : Transform;


// variables for hand IK joint points
var IK_rightHandTarget :  Transform;
var IK_leftHandTarget :  Transform;

//fake joint for physical movement biker to imitate inertia
var fakeCharPhysJoint : Transform;

//we need to know bike we ride on
var bikeRideOn : GameObject;

// making a link to corresponding bike's script
private var bikeStatusCrashed : pro_bike5;

// gameobject with script control variables
private var ctrlHub : GameObject; 

// making a link to corresponding bike's script
private var outsideControls : controlHub;

function Start () {

	//link to GameObject with script "controlHub"
	ctrlHub = GameObject.Find("gameScenario");
	//to connect c# leap motion and camera control script to this one
	outsideControls = ctrlHub.GetComponent(controlHub);

	//to turn off layer with reverse animation which override all other
	myAnimator = GetComponent(Animator);
	myAnimator.SetLayerWeight(2, 0); 
}

//fundamental mecanim IK script
//just keeps hands on wheelbar
function OnAnimatorIK(layerIndex: int) {
	if (IK_rightHandTarget != null){
		myAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand,IK_rightWeight);
    	myAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand,IK_rightWeight);  
    	myAnimator.SetIKPosition(AvatarIKGoal.RightHand,IK_rightHandTarget.position);
    	myAnimator.SetIKRotation(AvatarIKGoal.RightHand,IK_rightHandTarget.rotation);
    }
    if (IK_leftHandTarget != null){
		myAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand,IK_leftWeight);
    	myAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand,IK_leftWeight);  
    	myAnimator.SetIKPosition(AvatarIKGoal.LeftHand,IK_leftHandTarget.position);
    	myAnimator.SetIKRotation(AvatarIKGoal.LeftHand,IK_leftHandTarget.rotation);
    }
}

function Update () {

	//moves character with fake inertia
	if (fakeCharPhysJoint){
		this.transform.localEulerAngles.x = fakeCharPhysJoint.localEulerAngles.x;
		this.transform.localEulerAngles.y = fakeCharPhysJoint.localEulerAngles.y;
		this.transform.localEulerAngles.z = fakeCharPhysJoint.localEulerAngles.z;
	} else {
		return;
	}

	//the character should play animations when player press control keys
	//horizontal movement
	if (outsideControls.Horizontal <0 && bikerLeanAngle > -1.0){
		bikerLeanAngle = bikerLeanAngle -= 8 * Time.deltaTime;//8 - "magic number" of speed of pilot's body movement across. Just 8 - face it :)
		if (bikerLeanAngle < outsideControls.Horizontal) bikerLeanAngle = outsideControls.Horizontal;//this string seems strange but it's necessary for mobile version
		myAnimator.SetFloat("lean", bikerLeanAngle);//the character play animation "lean" for bikerLeanAngle more and more
	}
	if (outsideControls.Horizontal >0 && bikerLeanAngle < 1.0){
		bikerLeanAngle = bikerLeanAngle += 8 * Time.deltaTime;
		if (bikerLeanAngle > outsideControls.Horizontal) bikerLeanAngle = outsideControls.Horizontal;
		myAnimator.SetFloat("lean", bikerLeanAngle);
	}
	//vertical movement
	if (outsideControls.Vertical > 0 && bikerMoveAlong < 1.0){
		bikerMoveAlong = bikerMoveAlong += 3 * Time.deltaTime;
		if (bikerMoveAlong > outsideControls.Vertical) bikerMoveAlong = outsideControls.Vertical;
		myAnimator.SetFloat("moveAlong", bikerMoveAlong);
	}
	if (outsideControls.Vertical < 0 && bikerMoveAlong > -1.0){
		bikerMoveAlong = bikerMoveAlong -= 3 * Time.deltaTime;
		if (bikerMoveAlong < outsideControls.Vertical) bikerMoveAlong = outsideControls.Vertical;
		myAnimator.SetFloat("moveAlong", bikerMoveAlong);
	}
	
	
	//in a case of restart
	if (outsideControls.restartBike){
		var riderBodyVis = transform.Find("root/Hips");
		riderBodyVis.gameObject.SetActive(true);
	}
	
	//function for avarage rider pose
	bikerComeback();

	
	// pull leg(s) down when bike stopped
	if (Mathf.Round((bikeRideOn.GetComponent.<Rigidbody>().velocity.magnitude * 3.6)*10) * 0.1 <= 15){//no reverse speed
	reverseSpeed = 0.0;
	myAnimator.SetFloat("reverseSpeed", reverseSpeed);
	
		if (bikeRideOn.transform.localEulerAngles.z <=10 || bikeRideOn.transform.localEulerAngles.z >=350){
			if (bikeRideOn.transform.localEulerAngles.x <=10 || bikeRideOn.transform.localEulerAngles.x >=350){
				var legOffValue = (15-(Mathf.Round((bikeRideOn.GetComponent.<Rigidbody>().velocity.magnitude * 3.6)*10) * 0.1))/15;//need to define right speed to begin put down leg(s)
				myAnimator.SetLayerWeight(3, legOffValue);//leg is no layer 3 in animator
		 	}
		}
	}

	//when using reverse speed
	if (Mathf.Round((bikeRideOn.GetComponent.<Rigidbody>().velocity.magnitude * 3.6)*10) * 0.1 <= 15 ){//reverse speed

		myAnimator.SetLayerWeight(3, legOffValue);
		myAnimator.SetLayerWeight(2, 1); //to turn on layer with reverse animation which override all other

		reverseSpeed = bikeStatusCrashed.bikeSpeed/3;
		myAnimator.SetFloat("reverseSpeed", reverseSpeed);
		if (reverseSpeed >= 1.0){
			reverseSpeed = 1.0;
		}
		
		myAnimator.speed = reverseSpeed;

	} else 	if (Mathf.Round((bikeRideOn.GetComponent.<Rigidbody>().velocity.magnitude * 3.6)*10) * 0.1 > 15){
		reverseSpeed = 0.0;
		myAnimator.SetFloat("reverseSpeed", reverseSpeed);
		myAnimator.SetLayerWeight(3, legOffValue);
		myAnimator.SetLayerWeight(2, 0); //to turn off layer with reverse animation which override all other
		myAnimator.speed = 1;
	}
}

function bikerComeback(){
	if (outsideControls.Horizontal == 0 ){
		if (bikerLeanAngle > 0){
			bikerLeanAngle = bikerLeanAngle -= 6 * Time.deltaTime;//6 - "magic number" of speed of pilot's body movement back across. Just 6 - face it :)
			myAnimator.SetFloat("lean", bikerLeanAngle);
		}
		if (bikerLeanAngle < 0){
			bikerLeanAngle = bikerLeanAngle += 6 * Time.deltaTime;
			myAnimator.SetFloat("lean", bikerLeanAngle);
		}
	}
	if (outsideControls.Vertical == 0){
		if (bikerMoveAlong > 0){
			bikerMoveAlong = bikerMoveAlong -= 2 * Time.deltaTime;//3 - "magic number" of speed of pilot's body movement back along. Just 3 - face it :)
			myAnimator.SetFloat("moveAlong", bikerMoveAlong);
		}
		if (bikerMoveAlong < 0){
			bikerMoveAlong = bikerMoveAlong += 2 * Time.deltaTime;
			myAnimator.SetFloat("moveAlong", bikerMoveAlong);
		}
	}
}
