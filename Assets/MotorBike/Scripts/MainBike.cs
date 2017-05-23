using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Main Bike Script
//  A lot of parameter are tunable from the editor
//  some of them must be linked to provided bike meshes
//  see the update methode comment for more informations
//
// Any bike model should be compatible if it can provide the folowing
// collider and meshes
// The model we use in this project has been modeled and animated by Boris Churpin
// and adapted by Aurélien Bloch and Bastien Chatelain for a Virtual Reality Class Project
// The scripts we propose probably not take advantage of the whole Bike model which is very complete and complex

public class MainBike : MonoBehaviour
{
    // Debug variable for this class. Can be set in the unity window
    public bool MAIN_BIKE_DEBUG = false;


    ///////////////////////////////////////// wheels //////////////////////////////////////////////////
    // Define their colliders from editor (to link)
    public WheelCollider frontWheelCollider;
    public WheelCollider rearWheelCollider;
    // Define their meshes from editor (to link)
    public GameObject frontWheelMesh;
    public GameObject rearWheelMesh;


    /////////////////////////////// Stifness, CoM(center of mass) /////////////////////////////////////
    // For stiffness counting when rear brake is on. 
    // Need that to lose real wheel's stiffness during time
    private float stiffPowerGain = 0.0f;

    // Define CoM of bike from editor
    public Transform CoM;
    // Default CoM position :
    public float normalCoM = 0.0f;


    ////////////////// "Some meshes for display visual parts of bike ////////////
    // Rear pendulumn
    public Transform rearPendulumn;
    // Wheel bar
    public Transform steeringWheel;
    // Lower part of front forge
    public Transform suspensionFrontDown;

    // Normal front spring state
    private float normalFrontSuspSpring;
    // Normal rear spring state
    private float normalRearSuspSpring;


    // We need to clamp wheelbar angle according the speed : 
    // high speed -> low angle 
    // We create a curve which is 
    // -> 15degree at 0 speed
    // -> 1 degree at 100 speed
    public AnimationCurve wheelbarRestrictCurve = new AnimationCurve(new Keyframe(0f, 15f), new Keyframe(100f, 1f));
    // Temporary variable which contains the evaluation of previous curve for current speed
    private float tempMaxWheelAngle;

    // For wheels vusials match up the wheelColliders
    private Vector3 wheelCCenter;
    private RaycastHit hit;

    ////////////////////////////// technical variables ////////////////////////////////////////////////
    // The bike speed km/h
    public float bikeSpeed;

    // The airResuistance (to trick in the editor)
    public float airResistance = 2;

    // The rear gear can be engaged only if the speed is 0
    bool isRearGearEngaged = false;

    // Brake power which is an absract value but make sense
    public float frontBrakePower;
    // Engine power which is also an abstract value
    public float engineTorque;

    /// GearBox
    // Gear ratios which are abstract values
    public float[] gearRatio;
    // Engine maximum rotation per minute(RPM) 
    // When gearbox should switch to higher gear 
    public float maxEngineRPM;
    // Ideal moment to change up the currentGear
    public float engineRedline;
    // Lowest RPM when gear need to be switched down
    public float minEngineRPM;
    // Engine current rotation per minute(RPM)
    public float engineRPM;
    // current gear
    public int currentGear = 0;


    // Initial position and rotation of the bike for full Restart.
    public Vector3 initialPosition;
    public Quaternion initialRotation;

    private GameObject ctrlHub;
    private ControlHub outsideControls;

    // Use this for bike initialization
    void Start()
    {
        // Get the gameScenario game object which contains the control hub
        ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();


        // From Unity 5.3f we need to use intertiaTensor
        Vector3 setInitialTensor = GetComponent<Rigidbody>().inertiaTensor;
        GetComponent<Rigidbody>().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
        GetComponent<Rigidbody>().inertiaTensor = setInitialTensor;

        // Defaut wheel colors is black
        // Can be colored in debug mode to illustrate breacking and acceleration
        frontWheelMesh.GetComponent<Renderer>().sharedMaterial.color = Color.black;
        rearWheelMesh.GetComponent<Renderer>().sharedMaterial.color = Color.black;

        // Scale editor parameter
        engineTorque = engineTorque * 20;
        frontBrakePower = frontBrakePower * 20;

        //tehcnical variables
        normalRearSuspSpring = rearWheelCollider.suspensionSpring.spring;
        normalFrontSuspSpring = frontWheelCollider.suspensionSpring.spring;

        initialPosition = transform.position + new Vector3(0, 0.2f, 0);
        initialRotation = transform.rotation;

        outsideControls.fullRestartBike = true;
    }


    void OnGUI()
    {
        // draw the speedometer interface only if the menu is not open (which is equivalent of not existing)
        if (GameObject.Find("menuCamera") == null || outsideControls.help)
        {
            //Prepare Styles for different Label size
            GUIStyle biggerText = new GUIStyle("label");
            biggerText.fontSize = 40;
            GUIStyle middleText = new GUIStyle("label");
            middleText.fontSize = 22;
            GUIStyle smallerText = new GUIStyle("label");
            smallerText.fontSize = 14;

            //to show in on display interface: speed, gear and RPM
            if (GameObject.Find("menuCamera") == null)
            {

                GUI.color = Color.black;
                GUI.Label(new Rect(Screen.width * 0.875f, Screen.height * 0.9f, 120, 80), string.Format("" + "{0:0.}", bikeSpeed), biggerText);
                GUI.Label(new Rect(Screen.width * 0.76f, Screen.height * 0.88f, 60, 80), "" + (currentGear + 1), biggerText);

                GUI.color = Color.grey;
                GUI.Label(new Rect(Screen.width - 200, 10, 250, 40), "" + outsideControls.CONTROL_MODE[(int)outsideControls.controlMode], middleText);

                if (!isRearGearEngaged)
                {
                    GUI.Label(new Rect(Screen.width * 0.885f, Screen.height * 0.96f, 60, 40), "REAR", smallerText);
                }
                else
                {
                    GUI.color = Color.red;
                    GUI.Label(new Rect(Screen.width * 0.885f, Screen.height * 0.96f, 60, 40), "REAR", smallerText);
                    GUI.color = Color.grey;
                }
            }
            else
            {
                GUI.color = Color.white;
            }

            // user info help box lines
            if (outsideControls.help)
            {
                // Help control for each mode
                // I decided to dupplicate code line with similar command
                // to simplify modification

                if (outsideControls.controlMode == ControlHub.ControlMode.KEYBOARD_ONLY)
                {

                    GUI.Box(new Rect(10, 10, 180, 20), "UP - Accelerate", smallerText);
                    GUI.Box(new Rect(10, 30, 180, 20), "DOWN - Break", smallerText);
                    GUI.Box(new Rect(10, 50, 180, 20), "LEFT / RIGHT - Turn", smallerText);
                    GUI.Box(new Rect(10, 70, 180, 20), "R - Full Restart", smallerText);
                    GUI.Box(new Rect(10, 90, 180, 20), "M - Change Control Mode", smallerText);
                    GUI.Box(new Rect(10, 110, 180, 20), "V - Change V Mode", smallerText);
                    GUI.Box(new Rect(10, 130, 180, 20), "C - Rear Gear ON/OFF", smallerText);
                    GUI.Box(new Rect(10, 150, 180, 20), "RMB - Change point of view", smallerText);
                }
                else if (outsideControls.controlMode == ControlHub.ControlMode.BODY_TILT)
                {
                    GUI.Box(new Rect(10, 10, 180, 20), "RH - Accelerate", smallerText);
                    GUI.Box(new Rect(10, 30, 180, 20), "LH - Break", smallerText);
                    GUI.Box(new Rect(10, 50, 180, 20), "Body Tilt - Turn", smallerText);
                    GUI.Box(new Rect(10, 70, 180, 20), "R - Full Restart", smallerText);
                    GUI.Box(new Rect(10, 90, 180, 20), "M - Change Control Mode", smallerText);
                    GUI.Box(new Rect(10, 110, 180, 20), "V - Change V Mode", smallerText);
                    GUI.Box(new Rect(10, 130, 180, 20), "C - Rear Gear ON/OFF", smallerText);
                }
                else if (outsideControls.controlMode == ControlHub.ControlMode.HAND_TILT)
                {
                    GUI.Box(new Rect(10, 10, 180, 20), "RH - Accelerate", smallerText);
                    GUI.Box(new Rect(10, 30, 180, 20), "LH - Break", smallerText);
                    GUI.Box(new Rect(10, 50, 180, 20), "Hand Tilt - Turn", smallerText);
                    GUI.Box(new Rect(10, 70, 180, 20), "R - Full Restart", smallerText);
                    GUI.Box(new Rect(10, 90, 180, 20), "M - Change Control Mode", smallerText);
                    GUI.Box(new Rect(10, 110, 180, 20), "V - Change V Mode", smallerText);
                    GUI.Box(new Rect(10, 130, 180, 20), "C - Rear Gear ON/OFF", smallerText);
                }

            }
        }
    }


    // Update is called once per frame
    // Do in this order : 
    // - React if we need to shift the gear
    // - Correct some mesh position
    // - Compute the speed
    // - Accelerate (forward / backward)
    // - Break
    // - Engage the rear gear if needed
    // - Turn
    // - Reset the bike if needed
    //
    void FixedUpdate()
    {
        // if RPM is more than the limit we should shift gear up or down
        engineRPM = Mathf.Max(0.0f, rearWheelCollider.rpm * gearRatio[currentGear]);
        if (engineRPM > engineRedline)
        {
            engineRPM = Mathf.Max(0.0f, maxEngineRPM);
        }

        // The RPM are too high shift gear up
        if (engineRPM >= maxEngineRPM)
        {
            currentGear = currentGear + 1;
            if (currentGear >= gearRatio.Length) currentGear = gearRatio.Length - 1;
        }
        // The RPM are too slow shift gear down
        else if (engineRPM <= minEngineRPM)
        {
            currentGear = currentGear - 1;
            if (currentGear < 0) currentGear = 0;
        }


        //////////////////////// Visual mesh position ////////////////////////////////////////////
        //Rear pendulumn is looking at rear wheel
        rearPendulumn.transform.localRotation.SetEulerAngles(
            0 - 8 + (rearWheelMesh.transform.localPosition.y * 100),
            rearPendulumn.transform.localRotation.eulerAngles.y,
            rearPendulumn.transform.localRotation.eulerAngles.z
            );


        // Wheel bar rotating by front wheel
        suspensionFrontDown.transform.localPosition = new Vector3(
            suspensionFrontDown.transform.localPosition.x,
            frontWheelMesh.transform.localPosition.y - 0.15f,
            suspensionFrontDown.transform.localPosition.z
            );

        frontWheelMesh.transform.localPosition = new Vector3(
            frontWheelMesh.transform.localPosition.x,
            frontWheelMesh.transform.localPosition.y,
            frontWheelMesh.transform.localPosition.z - (suspensionFrontDown.transform.localPosition.y + 0.4f) / 5
            );


        // Color Debug
        if (MAIN_BIKE_DEBUG)
        {
            // reset color to black when neither accelerate or break
            frontWheelMesh.GetComponent<Renderer>().material.color = Color.black;
            rearWheelMesh.GetComponent<Renderer>().material.color = Color.black;
        }


        // Air resistance via a drag and angular drag 
        GetComponent<Rigidbody>().drag = GetComponent<Rigidbody>().velocity.magnitude / 210 * airResistance;
        GetComponent<Rigidbody>().angularDrag = 7 + GetComponent<Rigidbody>().velocity.magnitude / 20;


        //determinate the bike speed in km/h
        bikeSpeed = Mathf.Round((GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1f; //from m/s to km/h


        /////////////////////////// ACCELERATE ///////////////////////////////////////////////////////////
        //Forward case

        if (outsideControls.vertical > 0 && !isRearGearEngaged)
        {
            // Deal with accelerate and brake (we cannot do both).
            // Set the motor torque using the vertical outside control
            frontWheelCollider.brakeTorque = 0;
            rearWheelCollider.brakeTorque = 0;
            rearWheelCollider.motorTorque = engineTorque * outsideControls.vertical;


            // debug - rear wheel is green when accelerate
            if (MAIN_BIKE_DEBUG)
            {
                rearWheelMesh.GetComponent<Renderer>().material.color = Color.green;
            }

            CoM.localPosition = new Vector3(CoM.localPosition.x, normalCoM, 0.0f);
            GetComponent<Rigidbody>().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);

        }
        //Backward case
        else if (outsideControls.vertical > 0 && isRearGearEngaged)
        {
            // Deal with accelerate and brake (we cannot do both).
            // Set the motor torque using the vertical outside control
            frontWheelCollider.brakeTorque = 0;
            rearWheelCollider.brakeTorque = 0;
            // Need to go backward really slow
            rearWheelCollider.motorTorque = Mathf.Min(engineTorque * -outsideControls.vertical / 10 + (bikeSpeed * 50), 0.0f);
            // Debug - rear wheel is green when accelerate
            if (MAIN_BIKE_DEBUG)
            {
                rearWheelMesh.GetComponent<Renderer>().material.color = Color.white;
            }


            CoM.localPosition = new Vector3(CoM.localPosition.x, normalCoM, 0.0f);
            GetComponent<Rigidbody>().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
        }
        else
        {
            // Slow down when non accelerate 
            // Progressively reduce torque
            rearWheelCollider.motorTorque = Mathf.Max(0, rearWheelCollider.motorTorque - 0.01f * airResistance);
        }



        //////////////////////////////////// BRAKING /////////////////////////////////////////////////////

        // If breaking
        if (outsideControls.vertical < 0)
        {
            //Front part
            {
                // Deal with accelerate and brake (we cannot do both).
                // Set the break torque using the vertical outside control
                frontWheelCollider.brakeTorque = frontBrakePower * -outsideControls.vertical;
                rearWheelCollider.motorTorque = 0;

                GetComponent<Rigidbody>().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
                // Debug - wheel is red when braking
                if (MAIN_BIKE_DEBUG)
                {
                    frontWheelMesh.GetComponent<Renderer>().material.color = Color.red;
                }
            }
            //Rear part which is not so good as front brake
            {
                rearWheelCollider.brakeTorque = frontBrakePower / 2;

                if (this.transform.localEulerAngles.x > 180 && this.transform.localEulerAngles.x < 350)
                {
                    CoM.localPosition = new Vector3(CoM.localPosition.x, CoM.localPosition.y, 0.0f);
                }

            }

            // Update the friction values (useful i.e for skiding) 
            WheelFrictionCurve sFriction = frontWheelCollider.sidewaysFriction;
            sFriction.stiffness = ((stiffPowerGain / 2));
            frontWheelCollider.sidewaysFriction = sFriction;

            sFriction = rearWheelCollider.sidewaysFriction;
            sFriction.stiffness = 1.0f - stiffPowerGain;
            rearWheelCollider.sidewaysFriction = sFriction;

            // Update the power stiff value
            stiffPowerGain += 0.025f - (bikeSpeed / 10000);

            // Top the value to 0.9*speed/300
            if (stiffPowerGain > 0.9f - bikeSpeed / 300)
            {
                stiffPowerGain = 0.9f - bikeSpeed / 300;
            }
        }
        // If not breaking
        else
        {
            // if not accelerating eiter
            if (outsideControls.vertical == 0)
            {
                // augment the front break a litle bit to help the bike stopping when noting is down
                // simulate the inertia 
                frontWheelCollider.brakeTorque = Mathf.Max(0, frontWheelCollider.brakeTorque + airResistance);
            }


            // Reset the breaking parameters
            stiffPowerGain = stiffPowerGain -= 0.05f;
            if (stiffPowerGain < 0)
            {
                stiffPowerGain = 0;
            }

            WheelFrictionCurve sFriction = rearWheelCollider.sidewaysFriction;
            sFriction.stiffness = 1.0f - stiffPowerGain;
            rearWheelCollider.sidewaysFriction = sFriction;

            sFriction = frontWheelCollider.sidewaysFriction;
            sFriction.stiffness = 1.0f - stiffPowerGain;
            frontWheelCollider.sidewaysFriction = sFriction;
            
        }


        //////////////////////////////////// BACKWARD /////////////////////////////////////////////////////////	
        // Engage the rear gear only if the bike is stopped
        if (outsideControls.reverse && bikeSpeed <= 0 && !isRearGearEngaged)
        {
            isRearGearEngaged = true;
        }
        else if (!outsideControls.reverse && bikeSpeed >= 0 && isRearGearEngaged)
        {
            isRearGearEngaged = false;
        }

        //////////////////////////////////// TURNING ///////////////////////////////////////////////////////			
        // Evaluate curve for current speed which will restric turning angle according to speed
        // This allow to simulate a better phisic
        tempMaxWheelAngle = wheelbarRestrictCurve.Evaluate(bikeSpeed);
        if (outsideControls.horizontal != 0)
        {
            frontWheelCollider.steerAngle = tempMaxWheelAngle * outsideControls.horizontal;
            steeringWheel.rotation = frontWheelCollider.transform.rotation * Quaternion.Euler(0, frontWheelCollider.steerAngle, frontWheelCollider.transform.rotation.z);
        }
        else
        {
            frontWheelCollider.steerAngle = 0;
        }

        
        ////////////////////////////////// RESTART KEY ////////////////////////////////////////////////////////
        // Restart the game from initial location
        if (outsideControls.fullRestartBike)
        {

            transform.position = initialPosition;
            transform.rotation = initialRotation;

            transform.rotation = Quaternion.Euler(0.0f, transform.localEulerAngles.y, 0.0f);
            GetComponent<Rigidbody>().velocity = Vector3.zero;
            GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            CoM.localPosition = new Vector3(0.0f, normalCoM, 0.0f);

            frontWheelCollider.motorTorque = 0;
            frontWheelCollider.brakeTorque = 0;
            rearWheelCollider.motorTorque = 0;
            rearWheelCollider.brakeTorque = 0;
            GetComponent<Rigidbody>().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
            outsideControls.fullRestartBike = false;
        }
    }
}

