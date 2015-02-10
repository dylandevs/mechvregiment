using UnityEngine;
using System.Collections;

public class PlayerAvatar : MonoBehaviour {

	public GameObject NetworkManager;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetAim(bool ADS){
		if(ADS){
			print("aiming");
		}
		else {
			print("not aiming");
		}
	}
}
