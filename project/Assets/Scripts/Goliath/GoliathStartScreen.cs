using UnityEngine;
using System.Collections;

public class GoliathStartScreen : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
		if(SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.START)){
			StartGame ();
		}

	}

	public void StartGame(){
		Application.LoadLevel("GoliathScene"); 
	}
}
