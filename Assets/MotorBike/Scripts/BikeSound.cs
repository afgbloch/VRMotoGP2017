using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Small script to joint Sounds to the bike
// With the rigid_bike model we get following sounds : 
// - sport_engine_start (use as oneshot)
// - sport_engine_idle (use as loop)
// - sport_engine_high (use as loop)
// - skid (use as loop)
// - gear_change (use as oneshot)
//
// The idea is to use 3 audiosource
// - one main audio source (mainly for idle loop sound, but also for oneshot sound)
// - one source for mixing idle and high sound loop (accelerate feeling)
// - one source for skiding only
//

public class BikeSound : MonoBehaviour {

    // Link to real sound files listed above
    public AudioClip engineStart;
    public AudioClip gearChange;
    public AudioClip engineIdle;
    public AudioClip engineHigh;
    public AudioClip skid;


    // Link to MainBike script
    public MainBike mainBike;


    // AudioSource for main sound
    private AudioSource mainAudio;

    // AudioSource for mixing idle and high RPMs
    private AudioSource mixAudio;

    // AudioSource for skidding sound
    private AudioSource skidAudio;

   
    // Is the bike skidding 
    public bool isSkiding = false;
    public float skidingThreshold = 0.5f;

    // We need to know what gear is now to play change gear sound
    public int lastGear;


    private GameObject ctrlHub;
    private ControlHub outsideControls;

    
	// Use this for initialization
	void Start () {
        
        // Get the gameScenario game object which contains the control hub
        ctrlHub = GameObject.Find("gameScenario");
        // Extract the control hub from it
        outsideControls = ctrlHub.GetComponent<ControlHub>();

        // Initialize main audioSource
        mainAudio = gameObject.AddComponent<AudioSource>();
        mainAudio.loop = true;
        mainAudio.playOnAwake = true;
        mainAudio.pitch = 1.0f;
        mainAudio.volume = 1.0f;

        // Initialize second audioSource
        mixAudio = gameObject.AddComponent<AudioSource>();
        mixAudio.loop = true;
        mixAudio.playOnAwake = false;
        mixAudio.clip = engineHigh;
        mixAudio.pitch = 0;
        mixAudio.volume = 0.0f;

        // Initialize skid audioSource
        skidAudio = gameObject.AddComponent<AudioSource>();
        skidAudio.loop = false;
        skidAudio.playOnAwake = false;
        skidAudio.clip = skid;
        skidAudio.pitch = 1.0f;
        skidAudio.volume = 1.0f;

        // Link to bike and init others variables
        mainBike = GetComponent<MainBike>();
        lastGear = mainBike.currentGear;
        
        
        // Play once the enginstartsound on initialization
        mainAudio.PlayOneShot(engineStart);
        // And lounch the idle sound 
        StartCoroutine(playSounds());
    }
	
	// Update is called once per frame
	void Update () {

        // At slow speed : lot of idle and almost silent for high
        // At high RPM : lot for high and almost none of idle.
        float engineRatio = Mathf.Abs(mainBike.engineRPM / mainBike.maxEngineRPM);
        mainAudio.pitch = engineRatio + 1.0f;
        mainAudio.volume = 1.0f - engineRatio;
        mixAudio.pitch = engineRatio;
        mixAudio.volume = engineRatio;

        // When restart 
        if (outsideControls.fullRestartBike)
        {
            // Stop sounds
            mainAudio.Stop();
            mainAudio.pitch = 1.0f;
            // Play the starting sound
            mainAudio.PlayOneShot(engineStart);

            // Restart other sounds (with delay)
            StartCoroutine(playSounds());
        }

        // When gear change
        if (mainBike.currentGear != lastGear)
        {
            // Play the gear change sound
            mainAudio.PlayOneShot(gearChange);
            // Update the gear
            lastGear = mainBike.currentGear;
        }

        // When the bike skids
        // either the front or the rear wheel that are below the skidingThreshold
        if (!isSkiding &&
            (mainBike.rearWheelCollider.sidewaysFriction.stiffness < skidingThreshold
            || mainBike.frontWheelCollider.sidewaysFriction.stiffness < skidingThreshold) 
            && mainBike.bikeSpeed > 1)
        {
            // Start the skiding sound
            skidAudio.Play();
            // set this boolean to start the sound only once
            isSkiding = true;
        }
        // When the skiding condition are not met anymore
        else if (isSkiding &&
            ((mainBike.rearWheelCollider.sidewaysFriction.stiffness > skidingThreshold
            && mainBike.frontWheelCollider.sidewaysFriction.stiffness > skidingThreshold)
            || mainBike.bikeSpeed < 1))
        {
            // Stop the skiding sound
            skidAudio.Stop();
            // Set this boolean to allow a new skiding sound starting
            isSkiding = false;
        }
        
    }

    // Delayed method of 1 second
    // This allow to play complete starting sound before staring idle and mix
    // You must call this methode with : StartCoroutine(playSounds());
    // Help found here : 
    // http://answers.unity3d.com/questions/527741/c-waitforseconds-doesnt-seem-to-work-.html
    private IEnumerator playSounds()
    {
        // Wait one second
        yield return new WaitForSeconds(1.0f);
        // Start main and mix audios
        mainAudio.clip = engineIdle;
        mainAudio.Play();
        mixAudio.Play();
    }
  
}
