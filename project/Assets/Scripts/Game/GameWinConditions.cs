using UnityEngine;
using System.Collections;

public class GameWinConditions : MonoBehaviour {

	public GameObject flag;
	public GameObject personHit;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		flag.transform.parent = personHit.transform;
		

	}

	void onTriggerEnter(){

	}
}
