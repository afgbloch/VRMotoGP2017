using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// BikeLogic class
// Mainly contains the bike/biker animations
//
//  The used annimation and the corresponding model
//  come from the store
//

public class BikeLogic : MonoBehaviour {

    // Main Animator
    private Animator mainAnimator;

    // Turn Inverse Kinematics 
    private int rightWeightIK = 1;
    private int leftWeightIK = 1;

    // Moving right/left and forward/backward
    private float bikerLeanAngle = 0.0f;
    private float bikerMoveAlong = 0.0f;

    // Moving reverse animation
    private float reverseSpeed = 0.0f;

    // Hand IK joint points
    public Transform rightHandTargetIK;
    public Transform leftHandTargetIK;

    // Fake joint for physical movement biker to imitate inertia
    public Transform fakeCharPhysJoint;

    // The Bike GameObject
    public GameObject bikeRideOn;


    // Link to MainBike script
    private MainBike mainBike;


    private GameObject ctrlHub;
    private ControlHub outsideControls;


    // Use this for initialization
    void Start() {
        // Get the gameScenario game object which contains the control hub
        ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();

        // Link to bike and init others variables
        mainBike = bikeRideOn.GetComponent<MainBike>();
        mainAnimator = GetComponent<Animator>();
        mainAnimator.SetLayerWeight(2, 0);
    }

    // This methode keeps hands on wheelbar
    void OnAnimatorIK(int layerIndex)
    {
        if (rightHandTargetIK != null)
        {
            mainAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, rightWeightIK);
            mainAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, rightWeightIK);
            mainAnimator.SetIKPosition(AvatarIKGoal.RightHand, rightHandTargetIK.position);
            mainAnimator.SetIKRotation(AvatarIKGoal.RightHand, rightHandTargetIK.rotation);
        }
        if (leftHandTargetIK != null)
        {
            mainAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, leftWeightIK);
            mainAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, leftWeightIK);
            mainAnimator.SetIKPosition(AvatarIKGoal.LeftHand, leftHandTargetIK.position);
            mainAnimator.SetIKRotation(AvatarIKGoal.LeftHand, leftHandTargetIK.rotation);
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

        // Check the outside control to know when launch animation
        // So for the horizontal movement
        if (outsideControls.horizontal < 0 && bikerLeanAngle > -1.0)
        {
            // On the left
            bikerLeanAngle = bikerLeanAngle -= 8 * Time.deltaTime;//8 seems to be a good value
            if (bikerLeanAngle < outsideControls.horizontal) bikerLeanAngle = outsideControls.horizontal;
            // The character play animation "lean" 
            mainAnimator.SetFloat("lean", bikerLeanAngle);
        }
        else if (outsideControls.horizontal > 0 && bikerLeanAngle < 1.0)
        {
            // On the right
            bikerLeanAngle = bikerLeanAngle += 8 * Time.deltaTime;
            if (bikerLeanAngle > outsideControls.horizontal) bikerLeanAngle = outsideControls.horizontal;
            mainAnimator.SetFloat("lean", bikerLeanAngle);
        }
        else if (outsideControls.horizontal == 0)
        {
            // Make the biker gradually going back to its origianl horizontal position
            // if no horizontal input is given
            if (bikerLeanAngle > 0)
            {
                bikerLeanAngle = bikerLeanAngle -= 6 * Time.deltaTime;//3 seems to be a good value
                mainAnimator.SetFloat("lean", bikerLeanAngle);
            }
            if (bikerLeanAngle < 0)
            {
                bikerLeanAngle = bikerLeanAngle += 6 * Time.deltaTime;
                mainAnimator.SetFloat("lean", bikerLeanAngle);
            }
        }


        // And for the vertical movement
        if (outsideControls.vertical > 0 && bikerMoveAlong < 1.0)
        {
            bikerMoveAlong = bikerMoveAlong += 3 * Time.deltaTime;
            if (bikerMoveAlong > outsideControls.vertical) bikerMoveAlong = outsideControls.vertical;
            mainAnimator.SetFloat("moveAlong", bikerMoveAlong);
        }
        else if (outsideControls.vertical < 0 && bikerMoveAlong > -1.0)
        {
            bikerMoveAlong = bikerMoveAlong -= 3 * Time.deltaTime;
            if (bikerMoveAlong < outsideControls.vertical) bikerMoveAlong = outsideControls.vertical;
            mainAnimator.SetFloat("moveAlong", bikerMoveAlong);
        }
        else if (outsideControls.vertical == 0)
        {
            // Make the biker gradually going back to its origianl vertical position
            // if no vertical input is given
            if (bikerMoveAlong > 0)
            {
                bikerMoveAlong = bikerMoveAlong -= 2 * Time.deltaTime;//3 seems to be a good value
                mainAnimator.SetFloat("moveAlong", bikerMoveAlong);
            }
            if (bikerMoveAlong < 0)
            {
                bikerMoveAlong = bikerMoveAlong += 2 * Time.deltaTime;
                mainAnimator.SetFloat("moveAlong", bikerMoveAlong);
            }
        }


        // If restart
        if (outsideControls.fullRestartBike)
        {
            Transform riderBodyVis = transform.Find("root/Hips");
            riderBodyVis.gameObject.SetActive(true);
        }

        
        // Pull one or two legs down when bike is stopped
        float legOffValue = 0;
        if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1 <= 15)
        {
            reverseSpeed = 0.0f;
            mainAnimator.SetFloat("reverseSpeed", reverseSpeed);

            if (bikeRideOn.transform.localEulerAngles.z <= 10 || bikeRideOn.transform.localEulerAngles.z >= 350)
            {
                if (bikeRideOn.transform.localEulerAngles.x <= 10 || bikeRideOn.transform.localEulerAngles.x >= 350)
                {
                    legOffValue = (15 - (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1f)) / 15;
                    mainAnimator.SetLayerWeight(3, legOffValue);//leg is no layer 3 in animator
                }
            }
        }

        // When moving backward
        if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10.0f) * 0.1 <= 15)
        {

            mainAnimator.SetLayerWeight(3, legOffValue);
            //Turn on layer with reverse animation
            mainAnimator.SetLayerWeight(2, 1); 

            reverseSpeed = mainBike.bikeSpeed / 3;
            mainAnimator.SetFloat("reverseSpeed", reverseSpeed);
            if (reverseSpeed >= 1.0f)
            {
                reverseSpeed = 1.0f;
            }

            mainAnimator.speed = reverseSpeed;

        }
        else if (Mathf.Round((bikeRideOn.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * 10) * 0.1 > 15)
        {
            reverseSpeed = 0.0f;
            mainAnimator.SetFloat("reverseSpeed", reverseSpeed);
            mainAnimator.SetLayerWeight(3, legOffValue);
            // Turn off layer with reverse animation 
            mainAnimator.SetLayerWeight(2, 0); 
            mainAnimator.speed = 1;
        }
    }
    
}
