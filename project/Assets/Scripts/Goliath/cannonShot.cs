﻿using UnityEngine;
using System.Collections;

public class cannonShot : MonoBehaviour {

	public Vector3 constantSpeed;
	public float explosionRadius = 8f;

	public PoolManager plasmaExplodePool;
	public PoolManager pool;

	public ParticleEmitter explode1;
	public ParticleEmitter explode2;

	public Vector3 explosionLocation;
	public Vector3 remainsLocation;

	int layerMask = 1 << 3;
	float timer;
	float speed = 30;
	float waitOutTimer;
	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
		layerMask = ~layerMask;
	}
	
	// Update is called once per frame
	void Update () {
		// turn off object after a certain amount of time
		if(timer >=0){
			timer -= Time.deltaTime;
		}
		if (timer <= 0f) {
			timer = 50;
			explode1.emit = false;
			explode2.emit = false;
			waitOutTimer = 4;
		}

		transform.Translate(Vector3.forward * speed * Time.deltaTime);

		Ray ray = new Ray(transform.position,transform.forward);
		RaycastHit hit;

		explosionLocation = transform.position;
		//hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
		//remainsLocation = hit.point + hit.normal;

		//not decreasing when things are not hit
		if(waitOutTimer > 0){
			waitOutTimer -= Time.deltaTime;
			if(waitOutTimer <= 0){
				pool.Deactivate(gameObject);
			}
		}

		if (Physics.Raycast (ray,out hit, 35 * Time.deltaTime ,layerMask)) 
		{
			if(hit.collider.tag == "Player"){
				doDamageCannon();
			}
			else{
				GameObject plasmaExplosion = plasmaExplodePool.Retrieve(hit.point);

				explode1.emit = false;
				explode2.emit = false;
				waitOutTimer = 4;

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

				}
			}

		}
	}

	void OnEnable(){
		waitOutTimer = 0;
		timer = 5;
		explode1.emit = true;
		explode2.emit = true;
	}

	void doDamageCannon(){
		//do damgy thingys
	}
}
