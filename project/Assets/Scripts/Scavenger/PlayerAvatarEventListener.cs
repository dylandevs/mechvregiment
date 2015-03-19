using UnityEngine;
using System.Collections;

public class PlayerAvatarEventListener : MonoBehaviour {

	public PlayerAvatar avatar;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void NewWeaponReady(){
	}
	
	public void SwapWeapons(){
		avatar.ShowNewWeapon();
	}

	public void SwapInDynamic(){
	}

	public void SwapOutDynamic(){
	}

	public void PlaceInDynamic(){
	}

	public void TakeOutDynamic(){
	}

	public void ReleaseStatic(){
	}

	public void ResetStatic(){
	}
}
