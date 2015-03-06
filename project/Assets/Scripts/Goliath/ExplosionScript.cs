using UnityEngine;
using System.Collections;

public class ExplosionScript : MonoBehaviour {

	PoolManager pool;
	float life = 1f;

	// Use this for initialization
	void Start () {

		pool = transform.parent.GetComponent<PoolManager>();

	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if (life <= 0) {
			pool.Deactivate(gameObject);
		}
	}

	void OnEnable(){
		life = 1f;
	}
}
