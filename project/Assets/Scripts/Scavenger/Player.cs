/**
 * 
 * Tracks player attributes and handles behaviours
 * 
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	public int id = -1;

	const float HealWait = 5.0f;
	const float MaxHealth = 100;
	const float RegenInc = 0.7f;

	// Inputs
	public Camera playerCam;
	public Crosshair crossScript;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;
	public PlayerNetSend NetworkManager;
	public ScavUI display;

	// Status variables
	private float health = MaxHealth;
	private float healTimer = 0;
	private bool isAimingDownSights = false;
	private bool isCrouching = false;

	public GameObject[] weaponModels = new GameObject[]{};
	public GameObject[] weaponModels3 = new GameObject[]{};
	private Weapon[] weapons;
	int currentWeaponIndex = 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		TryRegen();
		crossScript.updateSpread (weapons [currentWeaponIndex].GetSpread ());
	}

	public void Initialize(int playerId, float[] window){
		id = playerId;

		weapons = new Weapon[weaponModels.Length];

		// Storing weapon references to component scripts
		for (int i = 0; i < weaponModels.Length; i++) {
			weapons[i] = weaponModels[i].GetComponent<Weapon>();
			weapons[i].setPlayerReference(this);
			weapons[i].setControllerReference(this.playerController);
		}

		weapons [currentWeaponIndex].gameObject.SetActive (true);

		// Setting player UI
		playerRenderer.InitializePlayerInterface(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].GetSpread());
		display.Initialize(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].GetSpread());

		// Setting controller
		playerController.SetController(id);
	}

	public void SetToKeyboard(){
		playerController.isKeyboard = true;
	}

	// Regenerates if healing timer is depleted and health is below maximum
	void TryRegen(){
		if (health < MaxHealth){
			if (healTimer <= 0){
				health = Mathf.Min(MaxHealth, health + RegenInc * Time.deltaTime);
			}
			else{
				healTimer -= Time.deltaTime;
			}
		}
	}

	public bool ToggleADS(bool? setADS = null){
		if (setADS != null) {
			isAimingDownSights = (bool)setADS;
			weapons[currentWeaponIndex].SetAds(isAimingDownSights);
			if (NetworkManager){
				NetworkManager.TogglePlayerADS(id, (bool)setADS);
			}
		}
		else{
			isAimingDownSights = !isAimingDownSights;
			weapons[currentWeaponIndex].SetAds(isAimingDownSights);
		}

		return isAimingDownSights;
	}

	public void SetCrouching(bool crouchState){
		isCrouching = crouchState;
	}

	// Changes currently selected weapon
	public void CycleWeapons(int adjustment){
		int prevWeaponIndex = currentWeaponIndex;
		currentWeaponIndex += adjustment;
		if (currentWeaponIndex >= weapons.Length){
			currentWeaponIndex = 0;
		}
		else if (currentWeaponIndex < 0){
			currentWeaponIndex = weapons.Length - 1;
		}

		// TODO: Play weapon change animation

		// Activate new weapon
		if (prevWeaponIndex != currentWeaponIndex){
			weapons [prevWeaponIndex].StopReloading();
			weapons [prevWeaponIndex].gameObject.SetActive (false);
			weapons [currentWeaponIndex].gameObject.SetActive (true);
		}
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage, Vector3 direction){
		health -= damage;
		healTimer = HealWait;

		display.IndicateDamageDirection (direction);
	}

	public Weapon GetCurrentWeapon(){
		return weapons [currentWeaponIndex];
	}

	// Attempts to fire bullet
	public void SetFiringState(bool isFiring){
		weapons [currentWeaponIndex].setFiringState (isFiring);
	}

	public void TriggerReload(){
		weapons [currentWeaponIndex].TryReloading ();
	}
}
