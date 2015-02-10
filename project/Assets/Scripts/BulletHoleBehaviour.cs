using UnityEngine;
using System.Collections;

public class BulletHoleBehaviour : MonoBehaviour {

	float life = 10;
	PoolManager pool;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if (life <= 0) {
			pool.Deactivate(gameObject);
		}
	}

	public void Initialize(){
		life = 10;
	}

	public void SetPool(PoolManager pooler){
		pool = pooler;
	}
}
