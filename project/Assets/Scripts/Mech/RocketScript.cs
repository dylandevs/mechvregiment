using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {

	public GameObject target;
	public float timer = 0;

	private float speed = 40f;
	private float trnSpeed = 30f;
	private float speedTwo = 60f;
	private float trnSpeedTwo = 200f;
	private Vector3 mustHit;
	// Update is called once per frame
	void Update(){
		timer += Time.deltaTime * 5;
	}
	void FixedUpdate () {

		if(timer <= 2){
			mustHit = target.transform.position;
			transform.Translate (Vector3.forward * speed * Time.deltaTime);
			var q = Quaternion.LookRotation(mustHit - transform.position);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, trnSpeed * Time.deltaTime);
		}

		if(timer >= 2.1){
			mustHit = target.transform.position;
			transform.Translate (Vector3.forward * speedTwo * Time.deltaTime);
			var q = Quaternion.LookRotation(mustHit - transform.position);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, trnSpeedTwo * Time.deltaTime);
		}

	}
}
