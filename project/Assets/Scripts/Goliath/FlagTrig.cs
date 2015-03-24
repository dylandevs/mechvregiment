using UnityEngine;
using System.Collections;

public class FlagTrig : MonoBehaviour {

	public MechShoot mechShooty;
	public GoliathNetworking network;

	public bool flagActive;
	// Use this for initialization
	void Start () {
		flagActive = false;
	}
	
	// Update is called once per frame
	void Update () {
		if(flagActive == true && SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.BUMPER)){
			pickedUp();
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Player"){
			flagActive = true;
		}
		//display Right bumper to pick up UI
	}

	void OnTriggerExit(Collider other){
		if(other.tag == "Player"){
			flagActive = false;
		}
	}

	void pickedUp(){
		mechShooty.carrying = true;
		network.photonView.RPC("GoliathPickedUpFlag",PhotonTargets.All);
		flagActive = false;
	}
}
