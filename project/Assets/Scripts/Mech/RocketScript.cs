using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {

	private float speed = 8f;
	private float trnSpeed = 15f;

	Vector3 target;
	// Update is called once per frame
	void FixedUpdate () {




		target = GameObject.FindGameObjectWithTag("missleTarget").transform.position;
		
		transform.Translate (Vector3.forward * speed * Time.deltaTime);
		
		var q = Quaternion.LookRotation(target - transform.position);
		transform.rotation = Quaternion.RotateTowards(transform.rotation, q, trnSpeed * Time.deltaTime);


	}
}
