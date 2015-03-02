using UnityEngine;
using System.Collections;

public class UIEffectHandling : MonoBehaviour {
	//miniGun
	public GameObject[] muzzleFlash;
	public GameObject[] tracer;
	public GameObject flashLocation;
	public bool flash;
	public bool trace;
	//miniGun Flash
	int flashNumber = 0;
	int tracerNumber = 0;
	



	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

//flash
		if(flash == true ){
			print(flashNumber);
			//miniGunFlash
			muzzleFlash[flashNumber].transform.position = flashLocation.transform.position;
			muzzleFlash[flashNumber].transform.up = flashLocation.transform.forward;
			muzzleFlash[flashNumber].SetActive(true);
			flashNumber ++;

			if(flashNumber >= 8){
				flashNumber = 0;
			}

			flash = false;
		}

	}//end of update
}//end of class
