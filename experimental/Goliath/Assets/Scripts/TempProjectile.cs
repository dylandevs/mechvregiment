using UnityEngine;
using System.Collections;

public class TempProjectile : MonoBehaviour {

	float speed = 300;
	//Vector3 velocity = new Vector3(0, 0, 0);
	float life = 5;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {
		//transform.position += velocity;
		life -= Time.deltaTime;

		// Delete after a while
		if (life <= 0){
			Destroy(gameObject);
		}
	}
	
	public void setDirection(Vector3 direction){
		rigidbody.velocity = direction.normalized * speed;
	}
}
