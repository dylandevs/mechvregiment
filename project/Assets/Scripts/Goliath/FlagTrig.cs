using UnityEngine;
using System.Collections;

public class FlagTrig : MonoBehaviour {

	public MechShoot mechShooty;
	public mechMovement health;
	public GoliathNetworking network;

	public bool flagActive;
	float currHealth;
	// Use this for initialization
	void Start () {
		flagActive = false;
	}
	
	// Update is called once per frame
	void Update () {

		currHealth = health.currMechHealth;

		if(flagActive == true && SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.BUMPER)){
			pickedUp();
		}
	}

	void OnTriggerEnter(Collider other){
		if(other.tag == "Goliath" && currHealth >= 1){
			flagActive = true;
			mechShooty.pressToPick = true;
		}
	}

	void OnTriggerExit(Collider other){
		if(other.tag == "Goliath"){
			flagActive = false;
			mechShooty.pressToPick = false;
		}

	}

	void pickedUp(){
		if(currHealth >= 1){
			mechShooty.pressToPick = false;
			mechShooty.carrying = true;
			network.photonView.RPC("GoliathPickedUpFlag",PhotonTargets.All);
			flagActive = false;
		}
	}
}
