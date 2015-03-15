using UnityEngine;
using System.Collections;

public class miniMapIconScript : MonoBehaviour {

	public float life;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(life >= 0){
			life -= Time.deltaTime;
		}

		if(life < 0){
			gameObject.SetActive(false);
		}
		Color tempColour = gameObject.renderer.material.color;
		tempColour.a = life/2f;
		gameObject.renderer.material.color = tempColour;

	}

	void OnEnable(){
		life = 3f;
	}
}
