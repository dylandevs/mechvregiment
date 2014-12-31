using UnityEngine;
using System.Collections;

public class cannonShot : MonoBehaviour {

	public float cannonSpeed;
	int layerMask = 1 << 16;
	float rotSpeed;

	// Use this for initialization
	void Start () {
		cannonSpeed = 20.0f * Time.deltaTime; 
		layerMask = ~layerMask;
		rotSpeed = 5 * Time.deltaTime; 
	}
	
	// Update is called once per frame
	void Update () {
		// make it degrade over time********8
		gameObject.transform.Translate(transform.forward * cannonSpeed,Space.World);
		gameObject.transform.Rotate(gameObject.transform.right* rotSpeed, Space.World);
	}

	void FixedUpdate(){
		Ray ray = new Ray(transform.position,transform.forward);

		if (Physics.Raycast (ray, 50 * Time.deltaTime,layerMask)) 
		{
			gameObject.SetActive(false);
			doDamageCannon();
		}
	}

	//*** add a self destruct to this and then make the ammo show up and dissapear accordingly*********

	void doDamageCannon(){
		//do damgy thingys
	}
}
