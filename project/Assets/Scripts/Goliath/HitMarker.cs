using UnityEngine;
using System.Collections;

public class HitMarker : MonoBehaviour {

	float life;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

		Color tempColour = gameObject.GetComponent<Renderer>().material.color;
		float amount = life / 0.5f;
		tempColour.a = life;
		gameObject.GetComponent<Renderer>().material.color = tempColour;

		if(life >= 0){
			life -= Time.deltaTime;
		}

		if(life <=0){
			gameObject.SetActive(false);
		}
	}

	void OnEnable(){
		life = 0.5f;
	}
}
