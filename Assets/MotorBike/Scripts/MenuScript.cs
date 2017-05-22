using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour {
    
    // Colored texture for the highlighted menu item
    private Texture2D tex;
    // Transparant texture for non highlighted items
    private Texture2D texAlpha;

    // Get the game controls objects
    private GameObject ctrlHub;
    private ControlHub outsideControls;
   

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


        // For each menu item the background is selected following the corresponding boolean item
        // each item call specific method(s)

        // Start Item
        mediumText.normal.background = outsideControls.menuStartStop ? tex : texAlpha;
        
        if (GUI.Button (new Rect (Screen.width -300, Screen.height - 350, 300, 40), "Start / Resume", mediumText) || (outsideControls.menuStartStop && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            resume();
        }


        // Full restart 
        mediumText.normal.background = outsideControls.menuFullRestart ? tex : texAlpha;

        if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 300, 300, 40), "Full Restart", mediumText) || (outsideControls.menuFullRestart && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            outsideControls.fullRestartBike = true;
            resume(); 
        }
        

        // Controle Mode
        mediumText.normal.background = outsideControls.menuMode ? tex : texAlpha;

        if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 250, 300, 40), outsideControls.CONTROL_MODE[(int)outsideControls.controlMode], mediumText) || (outsideControls.menuMode && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            outsideControls.nextControlMode(); 
        }


        // view mode
        mediumText.normal.background = outsideControls.menuView ? tex : texAlpha;

        if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 200, 300, 40), outsideControls.VIEW_MODE[(int)outsideControls.cameraMode], mediumText) || (outsideControls.menuView && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            outsideControls.switchCamera();
        }


        // Help 
        mediumText.normal.background = outsideControls.menuHelp ? tex : texAlpha;

        // simply convert the boolean into an integer
        int helpMode = outsideControls.help ? 1 : 0; 

        if (GUI.Button(new Rect(Screen.width - 300, Screen.height - 150, 300, 40), outsideControls.HELP_MODE[helpMode], mediumText) || (outsideControls.menuHelp && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            outsideControls.help = !outsideControls.help; 
        }


        // Exit item
        mediumText.normal.background = outsideControls.menuExit ? tex : texAlpha;
        
        if (GUI.Button(new Rect(Screen.width - 300, Screen.height-100, 300, 40), "Exit", mediumText) || (outsideControls.menuExit && outsideControls.menuClick))
        {
            outsideControls.menuClick = false;
            // This methode work only on the final build and not in the unity GUI
            Application.Quit();
        }
    }


	// Use this for initialization
	void Start () {

        // Create once the textures
        tex = new Texture2D(1, 1, TextureFormat.ARGB32, false);
        texAlpha = new Texture2D(1, 1, TextureFormat.ARGB32, false);

        tex.SetPixel(0, 0, new Color(186.0f / 255, 119.0f / 255, 42.0f / 255, 1.0f));
        tex.Apply();
        texAlpha.SetPixel(0, 0, new Color(186.0f / 255, 119.0f / 255, 42.0f / 255, 0.0f));
        texAlpha.Apply();


        // Get the ControlHub from the main scene. 
        ctrlHub = GameObject.Find("gameScenario");
        outsideControls = ctrlHub.GetComponent<ControlHub>();


    }

    // Resume the game by destroying the menu root
    // and setting the speed to normal speed
    private void resume()
    {
        Time.timeScale = 1;
        Destroy(gameObject);
        outsideControls.menuOn = false;
    }
}
