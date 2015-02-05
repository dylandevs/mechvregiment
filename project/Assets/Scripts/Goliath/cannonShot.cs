using UnityEngine;
using System.Collections;

public class cannonShot : MonoBehaviour {

	public float cannonSpeed;
	int layerMask = 1 << 16;
	float rotSpeed;
	float timer;

	// Use this for initialization
	void Start () {
		cannonSpeed = 20.0f * Time.deltaTime; 
		layerMask = ~layerMask;
		rotSpeed = 1 * Time.deltaTime; 
	}
	
	// Update is called once per frame
	void Update () {
		// turn off object after a certain amount of time
		timer += Time.deltaTime;
		
		if (timer > 8f) {
			gameObject.SetActive(false);
			timer = 0;
		}

		// make it degrade over time********
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
