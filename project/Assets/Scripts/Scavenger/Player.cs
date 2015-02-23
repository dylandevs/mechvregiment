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

	public float HealWait = 5.0f;
	public float MaxHealth = 100;
	private float InvMaxHealth = 1;
	public float RegenInc = 25f;

	// Inputs
	public Camera playerCam;
	public Crosshair crossScript;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;
	public PlayerNetSend NetworkManager;
	public ScavUI display;

	// Status variables
	private float health = 0;
	private float healTimer = 0;
	private bool isAimingDownSights = false;
	private bool isCrouching = false;

	public GameObject[] weaponModels = new GameObject[]{};
	public GameObject[] weaponModels3 = new GameObject[]{};
	private Weapon[] weapons;
	int currentWeaponIndex = 0;

	// Use this for initialization
	void Start () {
		InvMaxHealth = 1 / MaxHealth;
		health = MaxHealth;
	}
	
	// Update is called once per frame
	void Update () {
		TryRegen();
		crossScript.updateSpread (weapons [currentWeaponIndex].GetSpread ());
	}

	public void Initialize(int playerId, float[] window, float uiScale){
		id = playerId;

		weapons = new Weapon[weaponModels.Length];

		// Storing weapon references to component scripts
		for (int i = 0; i < weaponModels.Length; i++) {
			weapons[i] = weaponModels[i].GetComponent<Weapon>();
			weapons[i].SetPlayerReference(this);
			weapons[i].SetControllerReference(this.playerController);
			weapons[i].gameObject.SetActive(false);
		}

		weapons [currentWeaponIndex].gameObject.SetActive (true);

		// Setting player UI
		playerRenderer.InitializePlayerInterface(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].GetSpread());
		display.Initialize(window[0], window[1], window[2], window[3], uiScale, weapons[currentWeaponIndex].GetSpread());

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
				display.UpdateDamageOverlay (1 - health * InvMaxHealth);
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
			display.ActivateNewWeapon(currentWeaponIndex);
		}
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage, Vector3 direction){
		health -= damage;
		healTimer = HealWait;

		health = Mathf.Max (health, 0);

		display.IndicateDamageDirection (direction);
		display.UpdateDamageOverlay (1 - health * InvMaxHealth);

		if (health <= 0){
			// TODO: Trigger death
			print ("Dead.");
		}
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
