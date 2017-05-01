#pragma strict

///////////////////////////////////////////////////////////////////////////////////////////////////
/// Writen by Boris Chuprin smokerr@mail.ru and 
/// adapted by Bastien Chatelain and Aurelien Bloch 
/// For Virtual reality project
///////////////////////////////////////////////////////////////////////////////////////////////////
//Debug variable for this class. Can be set in the unity window
var pro_bike_debug : boolean = false;   


///////////////////////////////////////// wheels //////////////////////////////////////////////////
// define their colliders
var coll_frontWheel : WheelCollider;
var coll_rearWheel : WheelCollider;
// define their mesh
var meshFrontWheel : GameObject;
var meshRearWheel : GameObject;


/////////////////////////////// Stifness, CoM(center of mass) /////////////////////////////////////
//for stiffness counting when rear brake is on. Need that to lose real wheel's stiffness during time
private var stiffPowerGain : float = 0.0;
//for CoM moving along and across bike. Pilot's CoM.
private var tmpMassShift : float = 0.0;
					
// define CoM of bike
var CoM : Transform; //CoM object
//normalCoM is for situation when script need to return CoM in starting position :
var normalCoM : float; 

//maximum the bike can be inclined Horizontaly
var maxHorizontalAngle : float; 
var airResistance : float ; 

////////////////// "beauties" of visuals - some meshes for display visual parts of bike ////////////
//rear pendulumn
var rearPendulumn : Transform;
//wheel bar
var steeringWheel : Transform; 
//lower part of front forge
var suspensionFront_down : Transform;

// we need to declare it to know what is normal front spring state is
private var normalFrontSuspSpring : int; 
// we need to declare it to know what is normal rear spring state is
var normalRearSuspSpring: float;


// we need to clamp wheelbar angle according the speed. it means - the faster bike rides the less angle 
// you can rotate wheel bar first number in Keyframe is speed, second is max wheelbar degree
var wheelbarRestrictCurve : AnimationCurve = new AnimationCurve(new Keyframe(0f, 20f), new Keyframe(100f, 1f));
// temporary variable to restrict wheel angle according speed
private var tempMaxWheelAngle : float;

//for wheels vusials match up the wheelColliders
private var wheelCCenter : Vector3;
private var hit : RaycastHit;

////////////////////////////// technical variables ////////////////////////////////////////////////
//to know bike speed km/h
static var bikeSpeed : float;

//to turn On and Off reverse speed
static var isReverseOn : boolean = false; 
// Engine
//brake power which is an absract value but make sense
var frontBrakePower : float; 	
//engine power which is also an abstract value
var EngineTorque : float; 

/// GearBox
//engine maximum rotation per minute(RPM) when gearbox should switch to higher gear 
var MaxEngineRPM : float; 	
// Ideal moment to change up the CurrentGear
var EngineRedline : float; 	
//lowest RPM when gear need to be switched down
var MinEngineRPM : float; 	
// engine current rotation per minute(RPM)
static var EngineRPM : float; 
// gear ratios which is an abstract
var GearRatio: float[];
// current gear
var CurrentGear : int = 0; 

// gameobject with script control variables 
private var ctrlHub : GameObject;
// making a link to corresponding bike's script
private var outsideControls : controlHub;

/////////////////////////////////////////  ON SCREEN INFO /////////////////////////////////////////
function OnGUI ()
{
	//Prepare Styles for different Label size
	var biggerText = new GUIStyle("label");
  	biggerText.fontSize = 40;
  	var middleText = new GUIStyle("label");
  	middleText.fontSize = 22;
  	var smallerText = new GUIStyle("label");
  	smallerText.fontSize = 14;
  	
  	//to show in on display interface: speed, gear and RPM
	
	if(true || outsideControls.cameraMode == controlHub.CameraMode.THIRD_PERSON){
		GUI.color = Color.black;
		GUI.Label(Rect(Screen.width*0.875,Screen.height*0.9, 120, 80), String.Format(""+ "{0:0.}", bikeSpeed), biggerText);
		GUI.Label (Rect (Screen.width*0.76,Screen.height*0.88, 60, 80), "" + (CurrentGear+1),biggerText);
    
		if (!isReverseOn){
			GUI.color = Color.grey;
			GUI.Label (Rect (Screen.width*0.885, Screen.height*0.96,60,40), "REAR", smallerText);
		} else {
			GUI.color = Color.red;
			GUI.Label (Rect (Screen.width*0.885, Screen.height*0.96,60,40), "REAR", smallerText);
		}
	}else if (outsideControls.cameraMode == controlHub.CameraMode.FIRST_PERSON){


	}

    // user info help box lines
	if(outsideControls.help){
		GUI.color = Color.white;
		GUI.Box (Rect (10,10,180,20), "A,W,S,D or arrows - main control", smallerText);
		// TODO Add more ?
		GUI.color = Color.black; 
	}
}


function Start () {
	
	//link to GameObject with script "controlHub"
	ctrlHub = GameObject.Find("gameScenario");
	//to connect c# mobile control script to this one
	outsideControls = ctrlHub.GetComponent(controlHub);

	//Not used for now ! Will be use to set the limit horizontal angle of the bike
	// TODO set the value and use it. 
	maxHorizontalAngle = 40.0f;  
	airResistance = 2.0f; 

	//this string is necessary for Unity 5.3 with new PhysX feature when Tensor decoupled from center of mass
	var setInitialTensor : Vector3 = GetComponent.<Rigidbody>().inertiaTensor; 


	// now Center of Mass(CoM) is alligned to GameObject "CoM"
	GetComponent.<Rigidbody>().centerOfMass = Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z); 
	//this string is necessary for Unity 5.3 with new PhysX feature when Tensor decoupled from center of mass
	GetComponent.<Rigidbody>().inertiaTensor = setInitialTensor;
	
	// wheel colors for understanding of accelerate, idle, brake(black is idle status)
	meshFrontWheel.GetComponent.<Renderer>().material.color = Color.black;
	meshRearWheel.GetComponent.<Renderer>().material.color = Color.black;
	
	//for better physics of fast moving bodies
	GetComponent.<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
	
	// too keep EngineTorque variable like "real" horse powers
	EngineTorque = EngineTorque * 20;
	
	//*30 is for good braking to keep frontBrakePower = 100 for good brakes.
	//30 is abstract but necessary for Unity5
	frontBrakePower = frontBrakePower * 30;

	//tehcnical variables
	normalRearSuspSpring = coll_rearWheel.suspensionSpring.spring;
	normalFrontSuspSpring = coll_frontWheel.suspensionSpring.spring; 

	outsideControls.restartBike = true; 
}


function FixedUpdate (){
	
	// if RPM is more than engine can hold we should shift gear up or down
	EngineRPM = coll_rearWheel.rpm * GearRatio[CurrentGear];
	if (EngineRPM > EngineRedline){
		EngineRPM = MaxEngineRPM;
	}
	ShiftGears();
	
	ApplyLocalPositionToVisuals(coll_frontWheel);
	ApplyLocalPositionToVisuals(coll_rearWheel);
 	
 	
 	//////////////////////// Do meshes matched to Coliders ////////////////////////////////////////////
 	//beauty - rear pendulumn is looking at rear wheel
 	rearPendulumn.transform.localRotation.eulerAngles.x = 0-8+(meshRearWheel.transform.localPosition.y*100);
 	//beauty - wheel bar rotating by front wheel
	suspensionFront_down.transform.localPosition.y =(meshFrontWheel.transform.localPosition.y - 0.15);
	meshFrontWheel.transform.localPosition.z = meshFrontWheel.transform.localPosition.z - (suspensionFront_down.transform.localPosition.y + 0.4)/5;

	//Color Debug
	if(pro_bike_debug){
		meshFrontWheel.GetComponent.<Renderer>().material.color = Color.black;
		meshRearWheel.GetComponent.<Renderer>().material.color = Color.black;
	}


	// drag and angular drag for emulate air resistance
	//Debug.Log(GetComponent.<Rigidbody>().velocity.magnitude/210 * airResistance );
	GetComponent.<Rigidbody>().drag = GetComponent.<Rigidbody>().velocity.magnitude / 210 * airResistance; // when 250 bike can easy beat 200km/h // ~55 m/s
	GetComponent.<Rigidbody>().angularDrag = 7 + GetComponent.<Rigidbody>().velocity.magnitude/20;
	

	//determinate the bike speed in km/h
	bikeSpeed = Mathf.Round((GetComponent.<Rigidbody>().velocity.magnitude * 3.6)*10) * 0.1; //from m/s to km/h

	/////////////////////////// ACCELERATE ///////////////////////////////////////////////////////////
	//forward case
	if (outsideControls.Vertical >0 && !isReverseOn){
		//we need that to fix strange unity bug when bike stucks if you press "accelerate" just after "brake".
		coll_frontWheel.brakeTorque = 0;
		coll_rearWheel.motorTorque = EngineTorque * outsideControls.Vertical;

	
		// debug - rear wheel is green when accelerate
		if(pro_bike_debug){
			meshRearWheel.GetComponent.<Renderer>().material.color = Color.green;
		}
		// when normal accelerating CoM z is averaged
		CoM.localPosition.z = 0.0 + tmpMassShift;
		CoM.localPosition.y = normalCoM;
		GetComponent.<Rigidbody>().centerOfMass = Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
	
	}
		
	//reverse case
	if (outsideControls.Vertical > 0 && isReverseOn){
		coll_frontWheel.brakeTorque = 0;
		//need to make reverse really slow
		coll_rearWheel.motorTorque = Mathf.Min(EngineTorque * -outsideControls.Vertical/10 + (bikeSpeed*50), 0.0f);
		// debug - rear wheel is green when accelerate
		if(pro_bike_debug){
			meshRearWheel.GetComponent.<Renderer>().material.color = Color.white;
		}


		// when normal accelerating CoM z is averaged
		CoM.localPosition.z = 0.0 + tmpMassShift;
		CoM.localPosition.y = normalCoM;
		GetComponent.<Rigidbody>().centerOfMass = Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
	}



	RearSuspensionRestoration();
		
		
	//////////////////////////////////// BRAKING /////////////////////////////////////////////////////
	if (outsideControls.Vertical <0){

		//Front part
		{
			coll_frontWheel.brakeTorque = frontBrakePower * -outsideControls.Vertical;
			coll_rearWheel.motorTorque = 0; // you can't do accelerate and braking same time.
			
			//more user firendly gomeotric progession braking. But less stoppie and fun :( Boring...
			//coll_frontWheel.brakeTorque = frontBrakePower * -outsideControls.Vertical-(1 - -outsideControls.Vertical)*-outsideControls.Vertical;
			GetComponent.<Rigidbody>().centerOfMass = Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
			// debug - wheel is red when braking
			if(pro_bike_debug){
				meshFrontWheel.GetComponent.<Renderer>().material.color = Color.red;
			}
		}
		//Rear part which is not so good as front brake
		{
			coll_rearWheel.brakeTorque = frontBrakePower / 2;
			
			if (this.transform.localEulerAngles.x > 180 && this.transform.localEulerAngles.x < 350){
				CoM.localPosition.z = 0.0 + tmpMassShift;
			}
			
			coll_frontWheel.sidewaysFriction.stiffness = 1.0 - ((stiffPowerGain/2)-tmpMassShift*3);
			
			stiffPowerGain = stiffPowerGain += 0.025 - (bikeSpeed/10000);
				if (stiffPowerGain > 0.9 - bikeSpeed/300){ //orig 0.90
					stiffPowerGain = 0.9 - bikeSpeed/300;
				}
			coll_rearWheel.sidewaysFriction.stiffness = 1.0 - stiffPowerGain;
		}

	} else {

		// Front part reset
		{
			FrontSuspensionRestoration();
		}
		
		//Rear part reset 
		{
			coll_rearWheel.brakeTorque = 0;

			stiffPowerGain = stiffPowerGain -= 0.05;
			if (stiffPowerGain < 0){
				stiffPowerGain = 0;
			}
			coll_rearWheel.sidewaysFriction.stiffness = 1.0 - stiffPowerGain;// side stiffness is back to 2
			coll_frontWheel.sidewaysFriction.stiffness = 1.0 - stiffPowerGain;// side stiffness is back to 1
		}
	}	

		
	//////////////////////////////////// REVERSE /////////////////////////////////////////////////////////	
	if (outsideControls.reverse && bikeSpeed <=0 && !isReverseOn){
		isReverseOn = true;
	}
	else if (!outsideControls.reverse && bikeSpeed >=0 && isReverseOn){
	Debug.Log(bikeSpeed); 
		isReverseOn = false;
	}

	//////////////////////////////////// TURNING ///////////////////////////////////////////////////////			
	// the Unity physics isn't like real life. Wheel collider isn't round as real bike tyre.
	// so, face it - you can't reach accurate and physics correct countersteering effect on wheelCollider
	// For that and many other reasons we restrict front wheel turn angle when when speed is growing

	//associate speed with curve which you've tuned in Editor
	tempMaxWheelAngle = wheelbarRestrictCurve.Evaluate(bikeSpeed);
	if (outsideControls.Horizontal !=0){		
		coll_frontWheel.steerAngle = tempMaxWheelAngle * outsideControls.Horizontal;
		steeringWheel.rotation = coll_frontWheel.transform.rotation * Quaternion.Euler (0, coll_frontWheel.steerAngle, coll_frontWheel.transform.rotation.z);
	} else {
		coll_frontWheel.steerAngle = 0;
	}	
	
	//TODO Make inclinasion correct

	
	////////////////////////////////// RESTART KEY ////////////////////////////////////////////////////////
	// Restart key - recreate bike few meters above current place
	if (outsideControls.restartBike){
		
		transform.position+=Vector3(0,0.1,0);
		transform.rotation=Quaternion.Euler( 0.0, transform.localEulerAngles.y, 0.0 );
		GetComponent.<Rigidbody>().velocity=Vector3.zero;
		GetComponent.<Rigidbody>().angularVelocity=Vector3.zero;
		CoM.localPosition.x = 0.0;
		CoM.localPosition.y = normalCoM;
		CoM.localPosition.z = 0.0;

		coll_frontWheel.motorTorque = 0;
		coll_frontWheel.brakeTorque = 0;
		coll_rearWheel.motorTorque = 0;
		coll_rearWheel.brakeTorque = 0;
		GetComponent.<Rigidbody>().centerOfMass = Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
	}		
}


///////////////////////////////////////////// FUNCTIONS /////////////////////////////////////////////////////////
function ShiftGears() {

		
	if ( EngineRPM >= MaxEngineRPM ) {
		
		var AppropriateGear : int = CurrentGear;

		for ( var i = 0; i < GearRatio.length; i++ ) {
			if (coll_rearWheel.rpm * GearRatio[i] < MaxEngineRPM ) {
				AppropriateGear = i;
				break;
			}		
		}
		CurrentGear = AppropriateGear;
	}
	
	if ( EngineRPM <= MinEngineRPM ) {
		
		AppropriateGear = CurrentGear;

		for ( var j = GearRatio.length-1; j >= 0; j-- ) {
			if (coll_rearWheel.rpm * GearRatio[j] > MinEngineRPM ) {
				AppropriateGear = j;
				break;
			}
		}
	
		CurrentGear = AppropriateGear;
	}
}
	
function ApplyLocalPositionToVisuals (collider : WheelCollider) {
		if (collider.transform.childCount == 0) {
			return;
		}
		
		var visualWheel : Transform = collider.transform.GetChild (0);
		wheelCCenter = collider.transform.TransformPoint (collider.center);	
		if (Physics.Raycast (wheelCCenter, -collider.transform.up, hit, collider.suspensionDistance + collider.radius)) {
			visualWheel.transform.position = hit.point + (collider.transform.up * collider.radius);
			
		} else {
			visualWheel.transform.position = wheelCCenter - (collider.transform.up * collider.suspensionDistance);
		}
		var position : Vector3;
		var rotation : Quaternion;
		collider.GetWorldPose (position, rotation);

		
		visualWheel.localEulerAngles = Vector3(visualWheel.localEulerAngles.x, collider.steerAngle - visualWheel.localEulerAngles.z, visualWheel.localEulerAngles.z);
		visualWheel.Rotate (collider.rpm / 60 * 360 * Time.deltaTime, 0, 0);

}

//need to restore spring power for rear suspension after make it harder for wheelie
function RearSuspensionRestoration (){
	if (coll_rearWheel.suspensionSpring.spring > normalRearSuspSpring){
		coll_rearWheel.suspensionSpring.spring = coll_rearWheel.suspensionSpring.spring -= 500;
	}
}
//need to restore spring power for front suspension after make it weaker for stoppie
function FrontSuspensionRestoration (){
	if (coll_frontWheel.suspensionSpring.spring < normalFrontSuspSpring){
		coll_frontWheel.suspensionSpring.spring = coll_frontWheel.suspensionSpring.spring += 500;
	}
}