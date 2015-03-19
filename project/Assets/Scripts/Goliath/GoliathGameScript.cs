using UnityEngine;
using System.Collections;

public class GoliathGameScript : MonoBehaviour {
	//mech stuff
	public GameObject spawnPoint;
	public GameObject mech;
	public GameObject oculusCam;

	public GameObject restartMatch;

	public bool allConditions; 
	public bool netWorkReady;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(restartMatch == true){
			restartMatchFunction();
		}
	}

	public void restartMatchFunction(){
		mech.SetActive(false);
		mech.transform.position = spawnPoint.transform.position;

		//turn on mech menues and wait to see if they go through them 

		//check is they have clicked left fire then right fire then start
	}

	public void readyToStart(){
		if(allConditions = true){
			mech.SetActive(true);
		}
	}
}
