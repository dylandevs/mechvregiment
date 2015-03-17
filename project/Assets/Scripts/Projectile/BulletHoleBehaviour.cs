using UnityEngine;
using System.Collections;

public class BulletHoleBehaviour : MonoBehaviour {

	float life = 10;
	PoolManager pool;

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
		life = 10;
	}
}
