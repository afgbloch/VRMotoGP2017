using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeLogic : MonoBehaviour {

    private Animator myAnimator;

    // variables for turn IK link off for a time
    private int IK_rightWeight = 1;
    private int IK_leftWeight = 1;

    //variables for moving right/left and forward/backward
    private float bikerLeanAngle = 0.0f;
    private float bikerMoveAlong = 0.0f;

    //variables for moving reverse animation
    private float reverseSpeed = 0.0f;

    // variables for hand IK joint points
    public Transform IK_rightHandTarget;
    public Transform IK_leftHandTarget;

    //fake joint for physical movement biker to imitate inertia
    public Transform fakeCharPhysJoint;

    //we need to know bike we ride on
    public GameObject bikeRideOn;



    private MainBike mainBike;// making a link to corresponding bike's script


    private GameObject ctrlHub;
    private ControlHub outsideControls;





    // Use this for initialization
    void Start() {
        // Get the gameScenario game object which contains the control hub
        ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();
        mainBike = bikeRideOn.GetComponent<MainBike>();

        //to turn off layer with reverse animation which override all other
        myAnimator = GetComponent<Animator>();
        myAnimator.SetLayerWeight(2, 0);
    }

    //fundamental mecanim IK script
    //just keeps hands on wheelbar
    void OnAnimatorIK(int layerIndex)
    {
        if (IK_rightHandTarget != null)
        {
            myAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, IK_rightWeight);
            myAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, IK_rightWeight);
            myAnimator.SetIKPosition(AvatarIKGoal.RightHand, IK_rightHandTarget.position);
            myAnimator.SetIKRotation(AvatarIKGoal.RightHand, IK_rightHandTarget.rotation);
        }
        if (IK_leftHandTarget != null)
        {
            myAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, IK_leftWeight);
            myAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, IK_leftWeight);
            myAnimator.SetIKPosition(AvatarIKGoal.LeftHand, IK_leftHandTarget.position);
            myAnimator.SetIKRotation(AvatarIKGoal.LeftHand, IK_leftHandTarget.rotation);
        }
    }


    // Update is called once per frame
    void Update() {

        //moves character with fake inertia
        if (fakeCharPhysJoint)
        {

            this.transform.localEulerAngles = new Vector3(
                fakeCharPhysJoint.localEulerAngles.x,
                fakeCharPhysJoint.localEulerAngles.y,
                fakeCharPhysJoint.localEulerAngles.z
            );

        }
        else
        {
            return;
        }

        //the character should play animations when player press control keys
        //horizontal movement
        if (outsideControls.horizontal < 0 && bikerLeanAngle > -1.0)
        {
            bikerLeanAngle = bikerLeanAngle -= 8 * Time.deltaTime;//8 - "magic number" of speed of pilot's body movement across. Just 8 - face it :)
            if (bikerLeanAngle < outsideControls.horizontal) bikerLeanAngle = outsideControls.horizontal;//this string seems strange but it's necessary for mobile version
            myAnimator.SetFloat("lean", bikerLeanAngle);//the character play animation "lean" for bikerLeanAngle more and more
        }
        if (outsideControls.horizontal > 0 && bikerLeanAngle < 1.0)
        {
            bikerLeanAngle = bikerLeanAngle += 8 * Time.deltaTime;
            if (bikerLeanAngle > outsideControls.horizontal) bikerLeanAngle = outsideControls.horizontal;
            myAnimator.SetFloat("lean", bikerLeanAngle);
        }
        //vertical movement
        if (outsideControls.vertical > 0 && bikerMoveAlong < 1.0)
        {
            bikerMoveAlong = bikerMoveAlong += 3 * Time.deltaTime;
            if (bikerMoveAlong > outsideControls.vertical) bikerMoveAlong = outsideControls.vertical;
            myAnimator.SetFloat("moveAlong", bikerMoveAlong);
        }
        if (outsideControls.vertical < 0 && bikerMoveAlong > -1.0)
        {
            bikerMoveAlong = bikerMoveAlong -= 3 * Time.deltaTime;
            if (bikerMoveAlong < outsideControls.vertical) bikerMoveAlong = outsideControls.vertical;
            myAnimator.SetFloat("moveAlong", bikerMoveAlong);
        }


        //in a case of restart
        if (outsideControls.fullRestartBike)
        {
            Transform riderBodyVis = transform.Find("root/Hips");
            riderBodyVis.gameObject.SetActive(true);
        }

        //function for avarage rider pose
        bikerComeback();


        // pull leg(s) down when bike stopped
        float legOffValue = 0;
        if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1 <= 15)
        {//no reverse speed
            reverseSpeed = 0.0f;
            myAnimator.SetFloat("reverseSpeed", reverseSpeed);

            if (bikeRideOn.transform.localEulerAngles.z <= 10 || bikeRideOn.transform.localEulerAngles.z >= 350)
            {
                if (bikeRideOn.transform.localEulerAngles.x <= 10 || bikeRideOn.transform.localEulerAngles.x >= 350)
                {
                    legOffValue = (15 - (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1f)) / 15;//need to define right speed to begin put down leg(s)
                    myAnimator.SetLayerWeight(3, legOffValue);//leg is no layer 3 in animator
                }
            }
        }

        //when using reverse speed
        //TODO TEST
        if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10.0f) * 0.1 <= 15)
        {//reverse speed

            myAnimator.SetLayerWeight(3, legOffValue);
            myAnimator.SetLayerWeight(2, 1); //to turn on layer with reverse animation which override all other

            reverseSpeed = mainBike.bikeSpeed / 3;
            myAnimator.SetFloat("reverseSpeed", reverseSpeed);
            if (reverseSpeed >= 1.0f)
            {
                reverseSpeed = 1.0f;
            }

            myAnimator.speed = reverseSpeed;

        }
        else if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1 > 15)
        {
            reverseSpeed = 0.0f;
            myAnimator.SetFloat("reverseSpeed", reverseSpeed);
            myAnimator.SetLayerWeight(3, legOffValue);
            myAnimator.SetLayerWeight(2, 0); //to turn off layer with reverse animation which override all other
            myAnimator.speed = 1;
        }
    }

    void bikerComeback()
    {
        if (outsideControls.horizontal == 0)
        {
            if (bikerLeanAngle > 0)
            {
                bikerLeanAngle = bikerLeanAngle -= 6 * Time.deltaTime;//6 - "magic number" of speed of pilot's body movement back across. Just 6 - face it :)
                myAnimator.SetFloat("lean", bikerLeanAngle);
            }
            if (bikerLeanAngle < 0)
            {
                bikerLeanAngle = bikerLeanAngle += 6 * Time.deltaTime;
                myAnimator.SetFloat("lean", bikerLeanAngle);
            }
        }
        if (outsideControls.vertical == 0)
        {
            if (bikerMoveAlong > 0)
            {
                bikerMoveAlong = bikerMoveAlong -= 2 * Time.deltaTime;//3 - "magic number" of speed of pilot's body movement back along. Just 3 - face it :)
                myAnimator.SetFloat("moveAlong", bikerMoveAlong);
            }
            if (bikerMoveAlong < 0)
            {
                bikerMoveAlong = bikerMoveAlong += 2 * Time.deltaTime;
                myAnimator.SetFloat("moveAlong", bikerMoveAlong);
            }
        }
    }
}
