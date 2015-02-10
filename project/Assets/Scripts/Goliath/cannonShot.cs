using UnityEngine;
using System.Collections;

public class cannonShot : MonoBehaviour {

	public Vector3 constantSpeed;
	public float explosionRadius = 3f;
	public GameObject plasmaExplodePrefab;

	int layerMask = 1 << 16;
	float timer;

	// Use this for initialization
	void Start () {
		rigidbody.velocity = gameObject.transform.forward * 10;
		layerMask = ~layerMask;
	}
	
	// Update is called once per frame
	void Update () {
		// turn off object after a certain amount of time
		timer += Time.deltaTime;
		
		if (timer > 5f) {
			gameObject.SetActive(false);
			timer = 0;
		}

		// make it degrade over time********
		//gameObject.transform.Translate(transform.forward * cannonSpeed,Space.World);

		/*if(timer > 1.5){
			gameObject.transform.Rotate(gameObject.transform.right* rotSpeed, Space.World);
		}*/

	}

	void FixedUpdate(){
		Ray ray = new Ray(transform.position,transform.forward);

		if (Physics.Raycast (ray, 50 * Time.deltaTime,layerMask)) 
		{
			if(collider.tag == "Player"){
				gameObject.SetActive(false);
				doDamageCannon();
			}

			else{
				if (plasmaExplodePrefab != null) 
				{
					Instantiate(plasmaExplodePrefab, transform.position,Quaternion.identity);
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

					gameObject.SetActive(false);
				}
			}

		}
	}

	void doDamageCannon(){
		//do damgy thingys
	}
}
