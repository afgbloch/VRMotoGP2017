using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    
    private Texture2D tex;
    private Texture2D texAlpha;

    // Get the game controls objects
    private GameObject ctrlHub;
    private controlHub outsideControls;
    private readonly string[] CONTROL_MODE = { "keyboard only", "use body tilt", "use hand tilt" };

    void OnGUI ()
	{
		//Draw The titles
		GUIStyle biggerText = new GUIStyle ();
        biggerText.alignment = TextAnchor.UpperCenter;
        biggerText.fontSize = 40;
		biggerText.normal.textColor = Color.white;
		GUI.Label (new Rect (Screen.width / 2 - 50, 50, 100, 90), "MotoGP Simulator", biggerText);
		GUI.Label (new Rect (Screen.width / 2 - 50, 120, 100, 90), "Virtual Reality Project", biggerText);
        biggerText.fontSize = 30;
        GUI.Label (new Rect (Screen.width / 2 - 50, 180, 100, 90), "Bastien Chatelain & Aurélien Bloch", biggerText);


        // Draw the Menu and definding style
        GUIStyle mediumText = new GUIStyle ();
		mediumText.fontSize = 30;
        mediumText.alignment = TextAnchor.UpperLeft;
        mediumText.normal.textColor = Color.white;
        mediumText.padding = new RectOffset(5, 5, 5, 5);

        //TODO : Replace the mouse position with metaphore on the leap motion

        // Start Item
        mediumText.normal.background = outsideControls.menuStartStop ? tex : texAlpha;
        
        if (GUI.Button (new Rect (Screen.width -300, 350, 300, 40), "Start / Resume", mediumText) || (outsideControls.menuStartStop && outsideControls.menuClick))
        {
            resume();
        }

        // One other item
        mediumText.normal.background = outsideControls.menuMode ? tex : texAlpha;

        if (GUI.Button(new Rect(Screen.width - 300, 400, 300, 40), CONTROL_MODE[(int)outsideControls.controlMode], mediumText) || (outsideControls.menuMode && outsideControls.menuClick))
        {
            // Ready to add new menu option

            print(outsideControls);
            outsideControls.nextControlMode(); 
        }

        // Exit item
        mediumText.normal.background = outsideControls.menuExit ? tex : texAlpha;
        
        if (GUI.Button(new Rect(Screen.width - 300, 450, 300, 40), "Exit", mediumText) || (outsideControls.menuExit && outsideControls.menuClick))
        {
            Application.Quit();
        }
    }


	// Use this for initialization
	void Start () {
        tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texAlpha = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        tex.SetPixel(0, 0, new Color(186.0f / 255, 119.0f / 255, 42.0f / 255, 1.0f));
        tex.Apply();
        texAlpha.SetPixel(0, 0, new Color(186.0f / 255, 119.0f / 255, 42.0f / 255, 0.0f));
        texAlpha.Apply();


        // Get the controlhub from the main scene. 
        ctrlHub = GameObject.Find("gameScenario");//link to GameObject with script "controlHub"
        outsideControls = ctrlHub.GetComponent<controlHub>();// making a link to corresponding bike's script


    }

    // Resume the game by destroying the menu 
    // and setting the speed to normal speed
    private void resume()
    {
        Time.timeScale = 1;
        Destroy(gameObject);
        outsideControls.menuOn = false;
    }
}
