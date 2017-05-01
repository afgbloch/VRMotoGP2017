using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class toMainMenu : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if  (Input.GetKeyDown(KeyCode.Escape)){
            SceneManager.LoadScene("VRMotoGP2017_MainMenu");
        }
	}
}
