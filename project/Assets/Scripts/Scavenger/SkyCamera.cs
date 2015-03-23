using UnityEngine;
using System.Collections;

public class SkyCamera : MonoBehaviour {

	public ControllerScript playerController;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.LookAt(transform.position + playerController.facing);
	}
}
