using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	//misse target area
	public GameObject target;

	//stuff fr detonation
	public GameObject explosionPrefab;
	public float damage = 200f;  //damage at center
	public float explosionRadius = 3f;

	//speeds for rockets
	private float speed = 20f;
	private float trnSpeed = 20f;
	private float speedTwo = 50f;
	private float trnSpeedTwo = 200f;
	private Vector3 mustHit;

	float timer = 0;

	void Start(){

	}

	// Update is called once per frame
	void Update(){
		timer += Time.deltaTime * 5;
	}
	void FixedUpdate () {

		if(timer <= 7){
			mustHit = target.transform.position;
			transform.Translate (Vector3.forward * speed * Time.deltaTime);
			var q = Quaternion.LookRotation(mustHit - transform.position);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, trnSpeed * Time.deltaTime);
		}

		if(timer >= 7.1){
			mustHit = target.transform.position;
			transform.Translate (Vector3.forward * speedTwo * Time.deltaTime);
			var q = Quaternion.LookRotation(mustHit - transform.position);
			transform.rotation = Quaternion.RotateTowards(transform.rotation, q, trnSpeedTwo * Time.deltaTime);
		}

		//fire a raycast ahead to ensure you wont miss and go through a collider
		Ray ray = new Ray(transform.position,transform.forward);
		if (Physics.Raycast (ray, 30 * Time.deltaTime)) 
		{
			Detonate();
		}
		
	}// end of fixed update

	void Detonate()
	{
		//makes the boom effects if there is one loaded
		if (explosionPrefab != null) 
		{
			Instantiate(explosionPrefab, transform.position,Quaternion.identity);
		}
		
		//hurts whats near the boom depending on a overlap sphere function
		Collider[] colliders = Physics.OverlapSphere (transform.position, explosionRadius);
		foreach (Collider c in colliders) 
		{
			/*objectHealth hp = c.GetComponent<objectHealth>();

			if(hp != null)
			{
				float dist = Vector3.Distance(transform.position, c.transform.position);
				float damageRatio = 1f - (dist / explosionRadius);
				hp.ReciveDamage(damage * damageRatio);
			}*/
			
			
			//turns it off nd rests rotation
			//reset the object
			gameObject.SetActive(false);
			timer = 0;
			
		}
	}
}
