using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class startScript : MonoBehaviour {
    
    private Texture2D tex;
    private Texture2D texAlpha;

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
        mediumText.normal.background = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
            && Input.mousePosition.y < Screen.height - 350 && Input.mousePosition.y > Screen.height - 350 - 40 ? tex : texAlpha; 
        if (GUI.Button (new Rect (Screen.width -300, 350, 300, 40), "Start / Resume", mediumText))
        {
            Time.timeScale = 1;
            Destroy(gameObject) ;
        }

        // One other item
        mediumText.normal.background = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
            && Input.mousePosition.y < Screen.height - 400 && Input.mousePosition.y > Screen.height - 400 - 40 ? tex : texAlpha;
        if (GUI.Button(new Rect(Screen.width - 300, 400, 300, 40), "Other Items", mediumText))
        {
            // Ready to add new menu option
        }

        // Exit item
        mediumText.normal.background = Input.mousePosition.x > Screen.width - 300 && Input.mousePosition.x < Screen.width
            && Input.mousePosition.y < Screen.height - 450 && Input.mousePosition.y > Screen.height - 450 - 40 ? tex : texAlpha;
        if (GUI.Button(new Rect(Screen.width - 300, 450, 300, 40), "Exit", mediumText))
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
    }

    // Update is called once per frame
    void Update () {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 1;
            Destroy(gameObject);
        }



    }
}
