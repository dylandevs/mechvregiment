using UnityEngine;
using System.Collections;

public class dummyDamager : MonoBehaviour {

	float health;

	// Use this for initialization
	void Start () {
		health = 100;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(health <= 0){
			gameObject.SetActive(false);
		}

	}

	public void damageDummy(float damage){

		print("ouch");

		health -= damage;

	}
}
