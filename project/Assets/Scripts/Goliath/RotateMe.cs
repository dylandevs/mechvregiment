using UnityEngine;
using System.Collections;

public class RotateMe : MonoBehaviour {

	public float speedX;
	public float speedY;
	public float speedZ;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		transform.Rotate(speedX*Time.deltaTime, speedY*Time.deltaTime, speedZ*Time.deltaTime);
	}
}
