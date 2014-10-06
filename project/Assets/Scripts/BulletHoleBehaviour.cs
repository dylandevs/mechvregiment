using UnityEngine;
using System.Collections;

public class BulletHoleBehaviour : MonoBehaviour {

	float life = 10;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;
		if (life <= 0) {
			Destroy(gameObject);
		}
	}
}
