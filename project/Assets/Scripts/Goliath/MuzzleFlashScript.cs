using UnityEngine;
using System.Collections;

public class MuzzleFlashScript : MonoBehaviour {

	float life = 0.11f;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		life -= Time.deltaTime;
		if (life <= 0) {
			gameObject.SetActive(false);
		}
	}

	void OnEnable(){
		life = 0.11f;
	}

}
