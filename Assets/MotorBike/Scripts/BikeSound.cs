using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BikeSound : MonoBehaviour {

    // making a link to corresponding bike's script
    public MainBike mainBike;

    //we need to know what gear is now
    public int lastGear;

    // makeing second audioSource for mixing idle and high RPMs
    private AudioSource highRPMAudio;

    // makeing another audioSource for skidding sound
    private AudioSource skidSound;

    // creating sounds(Link it to real sound files at editor)
    public AudioClip engineStartSound;
    public AudioClip gearingSound;
    public AudioClip IdleRPM;
    public AudioClip highRPM;
    public AudioClip skid;

    //we need to know is any wheel skidding
    public bool isSkidingFront = false;
    public bool isSkidingRear = false;


    private GameObject ctrlHub;
    private ControlHub outsideControls;

    
	// Use this for initialization
	void Start () {
        ctrlHub = GameObject.Find("gameScenario");
        outsideControls = ctrlHub.GetComponent<ControlHub>();

        //assign sound to audioSource
        highRPMAudio = gameObject.AddComponent<AudioSource>();
        highRPMAudio.loop = true;
        highRPMAudio.playOnAwake = false;
        highRPMAudio.clip = highRPM;
        highRPMAudio.pitch = 0;
        highRPMAudio.volume = 0.0f;

        //same assign for skid sound
        skidSound = gameObject.AddComponent<AudioSource>();
        skidSound.loop = false;
        skidSound.playOnAwake = false;
        skidSound.clip = skid;
        skidSound.pitch = 1.0f;
        skidSound.volume = 1.0f;

        //real-time linking to current bike
        mainBike = this.GetComponent<MainBike>();
        GetComponent< AudioSource > ().PlayOneShot(engineStartSound);
        playEngineWorkSound();
        lastGear = mainBike.CurrentGear;
    }
	
	// Update is called once per frame
	void Update () {

        //Idle plays high at slow speed and highRPM sound play silent at same time. And vice versa.
        GetComponent< AudioSource > ().pitch = Mathf.Abs(mainBike.EngineRPM / mainBike.MaxEngineRPM) + 1.0f;
        GetComponent< AudioSource > ().volume = 1.0f - (Mathf.Abs(mainBike.EngineRPM / mainBike.MaxEngineRPM));
        highRPMAudio.pitch = Mathf.Abs(mainBike.EngineRPM / mainBike.MaxEngineRPM);
        highRPMAudio.volume = Mathf.Abs(mainBike.EngineRPM / mainBike.MaxEngineRPM);

        // all engine sounds stop when restart
        if (outsideControls.fullRestartBike)
        {
            GetComponent< AudioSource > ().Stop();
            GetComponent< AudioSource > ().pitch = 1.0f;
            GetComponent< AudioSource > ().PlayOneShot(engineStartSound);
            playEngineWorkSound();
        }

        //gear change sound
        if (mainBike.CurrentGear != lastGear)
        {
            GetComponent< AudioSource > ().PlayOneShot(gearingSound);
            lastGear = mainBike.CurrentGear;
        }
        //skids sound
        if (mainBike.coll_rearWheel.sidewaysFriction.stiffness < 0.5 && !isSkidingRear && mainBike.bikeSpeed > 1)
        {
            skidSound.Play();
            isSkidingRear = true;
        }
        else if (mainBike.coll_rearWheel.sidewaysFriction.stiffness >= 0.5 && isSkidingRear || mainBike.bikeSpeed <= 1)
        {
            skidSound.Stop();
            isSkidingRear = false;
        }
        if (mainBike.coll_frontWheel.brakeTorque >= (mainBike.frontBrakePower - 10) && !isSkidingFront && mainBike.bikeSpeed > 1)
        {
            skidSound.Play();
            isSkidingFront = true;
        }
        else if (mainBike.coll_frontWheel.brakeTorque < mainBike.frontBrakePower && isSkidingFront || mainBike.bikeSpeed <= 1)
        {
            skidSound.Stop();
            isSkidingFront = false;
        }
    }

    void playEngineWorkSound()
    {
        wait(1.0f); 
        GetComponent< AudioSource > ().clip = IdleRPM;
        GetComponent< AudioSource > ().Play();
        highRPMAudio.Play();
    }


    private IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
    }

}
