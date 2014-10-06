using UnityEngine;
using System.Collections;

public class topRotate : MonoBehaviour {

	// Use this for initialization
	void Start () {
		 
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 mechTopFacing = new Vector3(0,0,0);
		float rotSpeed = Time.deltaTime * 10;

		if (Input.GetKey (KeyCode.DownArrow)) {
			transform.Rotate(Vector3.right * rotSpeed, Space.Self);
		}
		if (Input.GetKey (KeyCode.UpArrow)) {
			transform.Rotate(Vector3.left * rotSpeed,Space.Self);
        }

		if (Input.GetKey (KeyCode.RightArrow)) {
			transform.Rotate(Vector3.up*rotSpeed,Space.Self);
		}

		if (Input.GetKey (KeyCode.LeftArrow)) {
			transform.Rotate(Vector3.down*rotSpeed,Space.Self);
		}

		Vector3 currentRot = transform.eulerAngles;

	}
}
