using UnityEngine;
using System.Collections;

public class GoliathSkyCamera : MonoBehaviour {

	public Transform followCamera;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.rotation = followCamera.rotation;
	}
}
