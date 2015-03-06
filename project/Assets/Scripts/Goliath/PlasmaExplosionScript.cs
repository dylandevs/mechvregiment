using UnityEngine;
using System.Collections;

public class PlasmaExplosionScript : MonoBehaviour {

	PoolManager pool;
	float life = 2f;

	public ParticleEmitter explode1;
	public ParticleEmitter explode2;
	
	// Use this for initialization
	void Start () {
		
		pool = transform.parent.GetComponent<PoolManager>();
		
	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if(life <= 0.8){
			explode1.emit = false;
			explode2.emit = false;
		}
		if (life <= 0) {
			pool.Deactivate(gameObject);
		}
	}
	
	void OnEnable(){
		explode1.emit = true;
		explode2.emit = true;
		life = 2f;
	}
}

