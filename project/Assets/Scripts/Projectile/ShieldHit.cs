using UnityEngine;
using System.Collections;

public class ShieldHit : MonoBehaviour {

	PoolManager pool;

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!GetComponent<ParticleSystem>().IsAlive()){
			pool.Deactivate(gameObject);
		}
	}

	void OnEnable(){
		GetComponent<ParticleSystem>().Play();
	}
}
