using UnityEngine;
using System.Collections;

public class BulletHoleBehaviour : MonoBehaviour {

	float life = 10;
	PoolManager pool;
	private bool initialized = false;

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

	void OnEnable(){
		if (initialized){
			if (!pool){
				pool = transform.parent.GetComponent<PoolManager>();
				this.GetComponent<AudioSource>().pitch = 1 + Random.Range(-0.2f, 0.2f);
				if (pool.splitListener){
					pool.splitListener.StoreAudioSource(this.GetComponent<AudioSource>());
				}
			}

			life = 10;
			if(pool.splitListener){
				pool.splitListener.PlayAudioSource(this.GetComponent<AudioSource>(), transform.position);
			}
			else{
				this.GetComponent<AudioSource>().Play();
			}
		}

		initialized = true;
	}
}
