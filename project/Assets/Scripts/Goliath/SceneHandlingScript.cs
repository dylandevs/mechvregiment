using UnityEngine;
using System.Collections;

public class SceneHandlingScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//add a UI that says push both trigs to ready up 

		float lTrig = SixenseInput.Controllers[0].Trigger;
		float rTrig = SixenseInput.Controllers[1].Trigger;

		if(lTrig > 0.7f && rTrig > 0.7f){
			Application.LoadLevel("GoliathScene");
		}

	}
}
