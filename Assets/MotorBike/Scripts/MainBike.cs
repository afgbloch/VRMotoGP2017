using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainBike : MonoBehaviour
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    /// Writen by Boris Chuprin smokerr@mail.ru and 
    /// adapted by Bastien Chatelain and Aurelien Bloch 
    /// For Virtual reality project
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    //Debug variable for this class. Can be set in the unity window
    bool pro_bike_debug = false;


    ///////////////////////////////////////// wheels //////////////////////////////////////////////////
    // define their colliders
    public WheelCollider coll_frontWheel;
    public WheelCollider coll_rearWheel;
    // define their mesh
    public GameObject meshFrontWheel;
    public GameObject meshRearWheel;


    /////////////////////////////// Stifness, CoM(center of mass) /////////////////////////////////////
    //for stiffness counting when rear brake is on. Need that to lose real wheel's stiffness during time
    private float stiffPowerGain = 0.0f;
    //for CoM moving along and across bike. Pilot's CoM.
    private float tmpMassShift = 0.0f;

    // define CoM of bike
    public Transform CoM; //CoM object
                   //normalCoM is for situation when script need to return CoM in starting position :
    public float normalCoM = 0.0f;

    //maximum the bike can be inclined Horizontaly
    public float maxHorizontalAngle;
    public float airResistance;

    ////////////////// "beauties" of visuals - some meshes for display visual parts of bike ////////////
    //rear pendulumn
    public Transform rearPendulumn;
    //wheel bar
    public Transform steeringWheel;
    //lower part of front forge
    public Transform suspensionFront_down;

    // we need to declare it to know what is normal front spring state is
    private float normalFrontSuspSpring;
    // we need to declare it to know what is normal rear spring state is
    public float normalRearSuspSpring;


    // we need to clamp wheelbar angle according the speed. it means - the faster bike rides the less angle 
    // you can rotate wheel bar first number in Keyframe is speed, second is max wheelbar degree
    public AnimationCurve wheelbarRestrictCurve = new AnimationCurve(new Keyframe(0f, 20f), new Keyframe(100f, 1f));
    // temporary variable to restrict wheel angle according speed
    private float tempMaxWheelAngle;

    //for wheels vusials match up the wheelColliders
    private Vector3 wheelCCenter;
    private RaycastHit hit;

    ////////////////////////////// technical variables ////////////////////////////////////////////////
    //to know bike speed km/h
    public float bikeSpeed;

    //to turn On and Off reverse speed
    static bool isReverseOn = false;
    // Engine
    //brake power which is an absract value but make sense
    public float frontBrakePower;
    //engine power which is also an abstract value
    public float EngineTorque;

    /// GearBox
    //engine maximum rotation per minute(RPM) when gearbox should switch to higher gear 
    public float MaxEngineRPM;
    // Ideal moment to change up the CurrentGear
    public float EngineRedline;
    //lowest RPM when gear need to be switched down
    public float MinEngineRPM;
    // engine current rotation per minute(RPM)
    public float EngineRPM;
    // gear ratios which is an abstract
    public float[] GearRatio;
    // current gear
    public int CurrentGear = 0;

    // gameobject with script control variables 
    private GameObject ctrlHub;
    // making a link to corresponding bike's script
    private ControlHub outsideControls;


    // initial position and rotation of the bike for full Restart.
    public Vector3 initialPosition;
    public Quaternion initialRotation;


    /////////////////////////////////////////  ON SCREEN INFO /////////////////////////////////////////
    void OnGUI()
    {
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
                GUI.Label(new Rect(Screen.width * 0.76f, Screen.height * 0.88f, 60, 80), "" + (CurrentGear + 1), biggerText);

                GUI.color = Color.grey;
                GUI.Label(new Rect(Screen.width - 200, 10, 250, 40), "" + outsideControls.CONTROL_MODE[(int)outsideControls.controlMode], middleText);

                if (!isReverseOn)
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


    void Start()
    {

        //link to GameObject with script "ControlHub"
        ctrlHub = GameObject.Find("gameScenario");
        //to connect c# mobile control script to this one
        outsideControls = ctrlHub.GetComponent<ControlHub>();


        //Not used for now ! Will be use to set the limit horizontal angle of the bike
        // TODO set the value and use it. 
        maxHorizontalAngle = 40.0f;
        airResistance = 2.0f;

        //this string is necessary for Unity 5.3f with new PhysX feature when Tensor decoupled from center of mass
        Vector3 setInitialTensor = GetComponent< Rigidbody > ().inertiaTensor;


        // now Center of Mass(CoM) is alligned to GameObject "CoM"
        GetComponent< Rigidbody > ().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
        //this string is necessary for Unity 5.3f with new PhysX feature when Tensor decoupled from center of mass
        GetComponent< Rigidbody > ().inertiaTensor = setInitialTensor;

        // wheel colors for understanding of accelerate, idle, brake(black is idle status)
        meshFrontWheel.GetComponent< Renderer > ().sharedMaterial.color = Color.black;
        meshRearWheel.GetComponent< Renderer > ().sharedMaterial.color = Color.black;

        //for better physics of fast moving bodies
        GetComponent< Rigidbody > ().interpolation = RigidbodyInterpolation.Interpolate;

        // too keep EngineTorque variable like "real" horse powers
        EngineTorque = EngineTorque * 20;

        //*30 is for good braking to keep frontBrakePower = 100 for good brakes.
        //30 is abstract but necessary for Unity5
        frontBrakePower = frontBrakePower * 30;

        //tehcnical variables
        normalRearSuspSpring = coll_rearWheel.suspensionSpring.spring;
        normalFrontSuspSpring = coll_frontWheel.suspensionSpring.spring;

        initialPosition = this.transform.position + new Vector3(0, 0.1f, 0);
        initialRotation = this.transform.rotation;

        outsideControls.fullRestartBike = true;
    }


    void FixedUpdate()
    {



        // if RPM is more than engine can hold we should shift gear up or down
        EngineRPM = Mathf.Max(0.0f, coll_rearWheel.rpm * GearRatio[CurrentGear]);
        if (EngineRPM > EngineRedline)
        {
            EngineRPM = Mathf.Max(0.0f, MaxEngineRPM);
        }
        ShiftGears();

        ApplyLocalPositionToVisuals(coll_frontWheel);
        ApplyLocalPositionToVisuals(coll_rearWheel);


        //////////////////////// Do meshes matched to Coliders ////////////////////////////////////////////
        //beauty - rear pendulumn is looking at rear wheel
        rearPendulumn.transform.localRotation.SetEulerAngles(
            0 - 8 + (meshRearWheel.transform.localPosition.y * 100),
            rearPendulumn.transform.localRotation.eulerAngles.y,
            rearPendulumn.transform.localRotation.eulerAngles.z
            ); 
            

        //beauty - wheel bar rotating by front wheel
        suspensionFront_down.transform.localPosition = new Vector3(
            suspensionFront_down.transform.localPosition.x, 
            meshFrontWheel.transform.localPosition.y - 0.15f,
            suspensionFront_down.transform.localPosition.z
            );

        meshFrontWheel.transform.localPosition = new Vector3(
            meshFrontWheel.transform.localPosition.x,
            meshFrontWheel.transform.localPosition.y,
            meshFrontWheel.transform.localPosition.z - (suspensionFront_down.transform.localPosition.y + 0.4f) / 5
            );

        //Color Debug
        if (pro_bike_debug)
        {
            meshFrontWheel.GetComponent< Renderer > ().material.color = Color.black;
            meshRearWheel.GetComponent< Renderer > ().material.color = Color.black;
        }


        // drag and angular drag for emulate air resistance
        GetComponent< Rigidbody > ().drag = GetComponent< Rigidbody > ().velocity.magnitude / 210 * airResistance; // when 250 bike can easy beat 200km/h // ~55 m/s
        GetComponent< Rigidbody > ().angularDrag = 7 + GetComponent< Rigidbody > ().velocity.magnitude / 20;


        //determinate the bike speed in km/h
        bikeSpeed = Mathf.Round((GetComponent< Rigidbody > ().velocity.magnitude * 3.6f) * 10) * 0.1f; //from m/s to km/h

        /////////////////////////// ACCELERATE ///////////////////////////////////////////////////////////
        //forward case



        if (outsideControls.vertical > 0 && !isReverseOn)
        {
            //we need that to fix strange unity bug when bike stucks if you press "accelerate" just after "brake".
            coll_frontWheel.brakeTorque = 0;
            coll_rearWheel.brakeTorque = 0;
            coll_rearWheel.motorTorque = EngineTorque * outsideControls.vertical;


            // debug - rear wheel is green when accelerate
            if (pro_bike_debug)
            {
                meshRearWheel.GetComponent< Renderer > ().material.color = Color.green;
            }
            // when normal accelerating CoM z is averaged
            CoM.localPosition = new Vector3(CoM.localPosition.x, normalCoM, 0.0f + tmpMassShift);
            
            GetComponent< Rigidbody > ().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);

        }
        //reverse case
        else if (outsideControls.vertical > 0 && isReverseOn)
        {
            coll_frontWheel.brakeTorque = 0;
            coll_rearWheel.brakeTorque = 0;
            //need to make reverse really slow
            coll_rearWheel.motorTorque = Mathf.Min(EngineTorque * -outsideControls.vertical / 10 + (bikeSpeed * 50), 0.0f);
            // debug - rear wheel is green when accelerate
            if (pro_bike_debug)
            {
                meshRearWheel.GetComponent< Renderer > ().material.color = Color.white;
            }


            // when normal accelerating CoM z is averaged
            CoM.localPosition = new Vector3(CoM.localPosition.x, normalCoM, 0.0f + tmpMassShift);
            GetComponent< Rigidbody > ().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
        }
        else
        {
            // slow down when non accelerate 
            //progressively reduce torque
            coll_rearWheel.motorTorque = Mathf.Max(0, coll_rearWheel.motorTorque - 0.01f * airResistance);
        }

        RearSuspensionRestoration();

        //////////////////////////////////// BRAKING /////////////////////////////////////////////////////
        if (outsideControls.vertical < 0)
        {

            //Front part
            {
                coll_frontWheel.brakeTorque = frontBrakePower * -outsideControls.vertical;
                coll_rearWheel.motorTorque = 0; // you can't do accelerate and braking same time.

                //more user firendly gomeotric progession braking. But less stoppie and fun :( Boring...
                //coll_frontWheel.brakeTorque = frontBrakePower * -outsideControls.vertical-(1 - -outsideControls.vertical)*-outsideControls.vertical;
                GetComponent< Rigidbody > ().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
                // debug - wheel is red when braking
                if (pro_bike_debug)
                {
                    meshFrontWheel.GetComponent< Renderer > ().material.color = Color.red;
                }
            }
            //Rear part which is not so good as front brake
            {
                coll_rearWheel.brakeTorque = frontBrakePower / 2;

                if (this.transform.localEulerAngles.x > 180 && this.transform.localEulerAngles.x < 350)
                {
                    CoM.localPosition = new Vector3(CoM.localPosition.x, CoM.localPosition.y, 0.0f + tmpMassShift);
                }

                WheelFrictionCurve sFriction = coll_frontWheel.sidewaysFriction;
                sFriction.stiffness = ((stiffPowerGain / 2) - tmpMassShift * 3);
                coll_frontWheel.sidewaysFriction = sFriction;

                stiffPowerGain = stiffPowerGain += 0.025f - (bikeSpeed / 10000);
                if (stiffPowerGain > 0.9f - bikeSpeed / 300)
                { //orig 0.90
                    stiffPowerGain = 0.9f - bikeSpeed / 300;
                }

                sFriction = coll_rearWheel.sidewaysFriction;
                sFriction.stiffness = 1.0f - stiffPowerGain;
                coll_rearWheel.sidewaysFriction = sFriction;
            }

        }
        else
        {

            if (outsideControls.vertical == 0)
            {
                coll_rearWheel.brakeTorque = Mathf.Max(0, coll_rearWheel.brakeTorque + airResistance);
                //print(coll_rearWheel.brakeTorque);
            }


            // Front part reset
            {
                FrontSuspensionRestoration();
            }

            //Rear part reset 
            {
                
                stiffPowerGain = stiffPowerGain -= 0.05f;
                if (stiffPowerGain < 0)
                {
                    stiffPowerGain = 0;
                }

                WheelFrictionCurve sFriction = coll_rearWheel.sidewaysFriction;
                sFriction.stiffness = 1.0f - stiffPowerGain;// side stiffness is back to 2
                coll_rearWheel.sidewaysFriction = sFriction;

                sFriction = coll_frontWheel.sidewaysFriction;
                sFriction.stiffness = 1.0f - stiffPowerGain;// side stiffness is back to 1
                coll_frontWheel.sidewaysFriction = sFriction;
                
            }
        }


        //////////////////////////////////// REVERSE /////////////////////////////////////////////////////////	
        if (outsideControls.reverse && bikeSpeed <= 0 && !isReverseOn)
        {
            isReverseOn = true;
        }
        else if (!outsideControls.reverse && bikeSpeed >= 0 && isReverseOn)
        {
            isReverseOn = false;
        }

        //////////////////////////////////// TURNING ///////////////////////////////////////////////////////			
        // the Unity physics isn't like real life. Wheel collider isn't round as real bike tyre.
        // so, face it - you can't reach accurate and physics correct countersteering effect on wheelCollider
        // For that and many other reasons we restrict front wheel turn angle when when speed is growing

        //associate speed with curve which you've tuned in Editor
        tempMaxWheelAngle = wheelbarRestrictCurve.Evaluate(bikeSpeed);
        if (outsideControls.horizontal != 0)
        {
            coll_frontWheel.steerAngle = tempMaxWheelAngle * outsideControls.horizontal;
            steeringWheel.rotation = coll_frontWheel.transform.rotation * Quaternion.Euler(0, coll_frontWheel.steerAngle, coll_frontWheel.transform.rotation.z);
        }
        else
        {
            coll_frontWheel.steerAngle = 0;
        }

        //TODO Make inclinasion correct


        ////////////////////////////////// RESTART KEY ////////////////////////////////////////////////////////
        // Restart key - recreate bike few meters above current place

        if (outsideControls.fullRestartBike)
        {

            this.transform.position = initialPosition;
            this.transform.rotation = initialRotation;

            //transform.position+=Vector3(0,0.1f,0);
            transform.rotation = Quaternion.Euler(0.0f, transform.localEulerAngles.y, 0.0f);
            GetComponent< Rigidbody > ().velocity = Vector3.zero;
            GetComponent< Rigidbody > ().angularVelocity = Vector3.zero;
            CoM.localPosition = new Vector3(0.0f, normalCoM, 0.0f); 
                
            coll_frontWheel.motorTorque = 0;
            coll_frontWheel.brakeTorque = 0;
            coll_rearWheel.motorTorque = 0;
            coll_rearWheel.brakeTorque = 0;
            GetComponent< Rigidbody > ().centerOfMass = new Vector3(CoM.localPosition.x, CoM.localPosition.y, CoM.localPosition.z);
            outsideControls.fullRestartBike = false;
        }
    }


    ///////////////////////////////////////////// FUNCTIONS /////////////////////////////////////////////////////////
    void ShiftGears()
    {


        if (EngineRPM >= MaxEngineRPM)
        {

            int AppropriateGear = CurrentGear;

            for (int i = 0; i < GearRatio.Length; i++)
            {
                if (coll_rearWheel.rpm * GearRatio[i] < MaxEngineRPM)
                {
                    AppropriateGear = i;
                    break;
                }
            }
            CurrentGear = AppropriateGear;
        }

        if (EngineRPM <= MinEngineRPM)
        {

            int AppropriateGear = CurrentGear;

            for (int j = GearRatio.Length - 1; j >= 0; j--)
            {
                if (coll_rearWheel.rpm * GearRatio[j] > MinEngineRPM)
                {
                    AppropriateGear = j;
                    break;
                }
            }

            CurrentGear = AppropriateGear;
        }
    }

    void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);
        wheelCCenter = collider.transform.TransformPoint(collider.center);

        if (Physics.Raycast(wheelCCenter, -collider.transform.up, hit.distance, (int) (collider.suspensionDistance + collider.radius)))
        {
            visualWheel.transform.position = hit.point + (collider.transform.up * collider.radius);

        }
        else
        {
            visualWheel.transform.position = wheelCCenter - (collider.transform.up * collider.suspensionDistance);
        }
        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);


        visualWheel.localEulerAngles = new Vector3(visualWheel.localEulerAngles.x, collider.steerAngle - visualWheel.localEulerAngles.z, visualWheel.localEulerAngles.z);
        visualWheel.Rotate(collider.rpm / 60 * 360 * Time.deltaTime, 0, 0);

    }

    //need to restore spring power for rear suspension after make it harder for wheelie
    void RearSuspensionRestoration()
    {
        if (coll_rearWheel.suspensionSpring.spring > normalRearSuspSpring)
        {

            JointSpring spring = coll_rearWheel.suspensionSpring;
            spring.spring = coll_rearWheel.suspensionSpring.spring - 500;
            coll_rearWheel.suspensionSpring = spring;
        }
    }
    //need to restore spring power for front suspension after make it weaker for stoppie
    void FrontSuspensionRestoration()
    {
        if (coll_frontWheel.suspensionSpring.spring < normalFrontSuspSpring)
        {
            JointSpring spring = coll_frontWheel.suspensionSpring;
            spring.spring = coll_frontWheel.suspensionSpring.spring + 500;
            coll_frontWheel.suspensionSpring = spring;
        }
    }
}

