using UnityEngine;
using System.Collections;

public class ExplosiveShot : MonoBehaviour {

	public string fireButton = "Fire2";
	public GameObject explosiveShotDecal;
	public float firePower = 10.0f;
	float explosiveVelocity = 50f;
	// Use this for initialization
	void Start () {
	
	}
	// Update is called once per frame
	void Update () 
	{
		if (Input.GetButtonDown (fireButton)) {
			Camera cam = Camera.main;
			GameObject theExplosive = (GameObject)Instantiate(explosiveShotDecal,transform.position + cam.transform.forward, transform.rotation);
			theExplosive.rigidbody.AddForce(cam.transform.forward * explosiveVelocity ,ForceMode.Impulse);
		}
	}
}
