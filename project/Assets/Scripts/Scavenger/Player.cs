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
	public float RespawnWait = 5.0f;

	// Inputs
	public Camera playerCam;
	public ControllerScript playerController;
	public PlayerNetSend NetworkManager;
	public ScavUI display;
	public LayerMask shootableLayer;

	// Status variables
	private float health = 0;
	private float healTimer = 0;
	private float respawnTimer = 0;
	private bool isAimingDownSights = false;
	private bool isCrouching = false;
	public bool isDead = true;

	public GameObject weaponWrapper;
	public GameObject weaponWrapper3;
	private GameObject[] weaponModels;
	private GameObject[] weaponModels3;
	private Weapon[] weapons;
	int currentWeaponIndex = 0;

	// Use this for initialization
	void Start () {
		InvMaxHealth = 1 / MaxHealth;
		health = MaxHealth;
	}
	
	// Update is called once per frame
	void Update () {
		if (!isDead){
			TryRegen();
		}
		else{
			TryRespawn();
		}
		//crossScript.updateSpread (weapons [currentWeaponIndex].GetSpread ());
		display.UpdateCrosshairSpread(weapons [currentWeaponIndex].GetSpread ());
	}

	public void Initialize(int playerId, float[] window, float uiScale){
		id = playerId;

		weaponModels = new GameObject[weaponWrapper.transform.childCount];
		for(int i = 0; i < weaponWrapper.transform.childCount; i++){
			weaponModels[i] = weaponWrapper.transform.GetChild(i).gameObject;
		}

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
		//playerRenderer.InitializePlayerInterface(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].GetSpread());
		display.Initialize(window[0], window[1], window[2], window[3], uiScale, weapons[currentWeaponIndex].GetSpread());

		// Setting controller
		playerController.SetController(id);
	}

	public void SetToKeyboard(){
		playerController.isKeyboard = true;
	}

	private void TryRespawn(){
		if (respawnTimer > 0){
			respawnTimer -= Time.deltaTime;
		}
		else{
			Respawn();
		}
	}

	private void Respawn(){
		isDead = false;
	}

	// Regenerates if healing timer is depleted and health is below maximum
	private void TryRegen(){
		if (health < MaxHealth){
			if (healTimer > 0){
				healTimer -= Time.deltaTime;

			}
			else{
				Regen();
			}
		}
	}

	private void Regen(){
		health = Mathf.Min(MaxHealth, health + RegenInc * Time.deltaTime);
		display.UpdateDamageOverlay (1 - health * InvMaxHealth);
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
		if (!isDead){
			health -= damage;
			healTimer = HealWait;

			health = Mathf.Max (health, 0);

			display.IndicateDamageDirection (direction);
			display.UpdateDamageOverlay (1 - health * InvMaxHealth);

			if (health <= 0){
				// TODO: Trigger death
				isDead = true;
			}
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

	public void TriggerHitMarker(){
		display.TriggerHitMarker();
	}
}
