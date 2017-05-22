using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Speedometer : MonoBehaviour {

    //speedometer
    public Texture2D GUIDashboard;
    public Texture2D dashboardArrow;
    private float topSpeed;
    private float stopAngle;
    private float topSpeedAngle;
    public float speed;

    //tachometer
    public Texture2D chronoTex;
    private float topRPM;
    private float stopRPMAngle;
    private float topRPMAngle;
    public float RPM;

    //link to bike script
    public MainBike mainBike;


	// Use this for initialization
	void Start () {
        mainBike = GameObject.Find("rigid_bike").GetComponent<MainBike>();

        topSpeed = 210;
        stopAngle = -215;
        topSpeedAngle = 0;
        topRPM = 12000;
        stopRPMAngle = -200;
        topRPMAngle = 0;
        wait(0.5f);
        MainBike linkToBike1 = GameObject.Find("rigid_bike").GetComponent<MainBike>();
        mainBike = linkToBike1;
    }

    private void OnGUI()
    {
        // draw the speedometer interface only if the menu is not open (which is equivalent of not existing)
        if (GameObject.Find("menuCamera") == null)
        {
            // speedometer
            GUI.DrawTexture(new Rect(Screen.width * 0.85f, Screen.height * 0.8f, GUIDashboard.width / 2, GUIDashboard.height / 2), GUIDashboard);
            Vector2 centre = new Vector2(Screen.width * 0.85f + GUIDashboard.width / 4, Screen.height * 0.8f + GUIDashboard.height / 4);
            Matrix4x4 savedMatrix = GUI.matrix;
            float speedFraction = speed / topSpeed;
            float needleAngle = Mathf.Lerp(stopAngle, topSpeedAngle, speedFraction);
            GUIUtility.RotateAroundPivot(needleAngle, centre);
            GUI.DrawTexture(new Rect(centre.x, centre.y - dashboardArrow.height / 4, dashboardArrow.width / 2, dashboardArrow.height / 2), dashboardArrow);
            GUI.matrix = savedMatrix;

            //tachometer
            GUI.DrawTexture(new Rect(Screen.width * 0.70f, Screen.height * 0.7f, chronoTex.width / 1.5f, chronoTex.height / 1.5f), chronoTex);
            Vector2 centreTacho = new Vector2(Screen.width * 0.70f + chronoTex.width / 3, Screen.height * 0.7f + chronoTex.height / 3);
            Matrix4x4 savedTachoMatrix = GUI.matrix;
            float tachoFraction = RPM / topRPM;
            float needleTachoAngle = Mathf.Lerp(stopRPMAngle, topRPMAngle, tachoFraction);
            GUIUtility.RotateAroundPivot(needleTachoAngle, centreTacho);
            GUI.DrawTexture(new Rect(centreTacho.x, centreTacho.y - dashboardArrow.height / 3, dashboardArrow.width / 1.5f, dashboardArrow.height / 1.5f), dashboardArrow);
            GUI.matrix = savedTachoMatrix;
        }
    }


    // Update is called once per frame
    void FixedUpdate () {
        speed = mainBike.bikeSpeed;
        RPM = mainBike.EngineRPM;
    }


    // Local waiting method
    private IEnumerator wait(float time)
    {
        yield return new WaitForSeconds(time);
    }
}
