using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class TransformScene : MonoBehaviour {
	public Transform referenceTransform;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (referenceTransform!=null){
			this.transform.localScale = referenceTransform.localScale;	
			this.transform.position = referenceTransform.position;
			this.transform.rotation = referenceTransform.rotation;
		}

	}
}
