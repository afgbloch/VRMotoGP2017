using UnityEngine;
using System.Collections;

public class SplitScreen4 : MonoBehaviour {

	public Camera topLeft;
	public Camera bottomLeft;
	public Camera topRight;
	public Camera bottomRight;

	// initialization
	void Start () {
		if (topLeft != null)
			topLeft.rect = new Rect (0, .5f, .5f, .5f);

		if (bottomLeft != null)
			bottomLeft.rect = new Rect (0, 0, .5f, .5f);

		if (topRight != null)
			topRight.rect = new Rect (.5f, .5f, .5f, .5f);

		if (bottomRight != null)
			bottomRight.rect = new Rect (.5f, 0, .5f, .5f);
	}

}
