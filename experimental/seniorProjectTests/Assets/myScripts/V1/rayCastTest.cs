using UnityEngine;
using System.Collections;

public class rayCastTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

	}

	void hasBeenShot(RaycastHit info)
	{
		Debug.Log ("I'm Shot", gameObject);
	}
}
