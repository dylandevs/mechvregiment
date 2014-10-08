﻿using UnityEngine;
using System.Collections;

public class DetonateOnHit : MonoBehaviour {

	public GameObject explosionPrefab;
	public float damage = 200f;  //damage at center
	public float explosionRadius = 3f;


	// on update fire a ray of length that rocket moves per unit to use the detonate
	void FixedUpdate()
	{
		Ray ray = new Ray(transform.position,transform.forward);
		if (Physics.Raycast (ray, 70 * Time.deltaTime)) 
		{
			Detonate();
		}
	
	}
	void Detonate()
	{
		//makes the boom
		if (explosionPrefab != null) 
		{
			Instantiate(explosionPrefab, transform.position,Quaternion.identity);
		}
		//turns it off nd rests rotation
		transform.LookAt(new Vector3 (270, 0, 0));
		print (transform.eulerAngles);
		gameObject.SetActive(false);
	

		//hurts whats near the boom
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
