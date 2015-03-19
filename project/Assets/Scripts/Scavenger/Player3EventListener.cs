using UnityEngine;
using System.Collections;

public class Player3EventListener : MonoBehaviour {

	public ControllerScript control;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void NewWeaponReady(){
		control.EndWeaponSwap();
	}
	
	public void SwapWeapons(){
		control.ShowNewWeapon();
	}

	public void SwapInDynamic(){
		control.player.GetCurrentWeapon().SwapInDynamic();
	}

	public void SwapOutDynamic(){
		control.player.GetCurrentWeapon().SwapOutDynamic();
	}

	public void PlaceInDynamic(){
		control.player.GetCurrentWeapon().PlaceInDynamic();
	}

	public void TakeOutDynamic(){
		control.player.GetCurrentWeapon().TakeOutDynamic();
	}

	public void ReleaseStatic(){
		control.player.GetCurrentWeapon().ReleaseStatic();
	}

	public void ResetStatic(){
		control.player.GetCurrentWeapon().ResetStatic();
	}
}
