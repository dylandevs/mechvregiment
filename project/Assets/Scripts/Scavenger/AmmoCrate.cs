using UnityEngine;
using System.Collections;

public class AmmoCrate : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider collider){
		if (collider.tag == "Player"){
			Player player = collider.GetComponent<Player>();
			player.isReplenishingAmmo = true;
		}
	}

	void OnTriggerExit(Collider collider){
		if (collider.tag == "Player"){
			Player player = collider.GetComponent<Player>();
			player.isReplenishingAmmo = false;
		}
	}
}
