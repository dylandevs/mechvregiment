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
	public PlayerNetSend networkManager;
	public ScavUI display;
	public LayerMask shootableLayer;
	public Animator anim;
	public ScavLayer initializer;

	// Status variables
	private float health = 0;
	private float healTimer = 0;
	private float respawnTimer = 0;
	private bool isAimingDownSights = false;
	public bool isDead = true;

	public GameObject weaponWrapper;
	public GameObject weaponWrapper3;
	private GameObject[] weaponModels;
	private GameObject[] weaponModels3;
	private Weapon[] weapons;

	[HideInInspector]
	public int currentWeaponIndex = 0;

	// Recorded variables
	private Vector3 startingPos;
	private int flinchHash = Animator.StringToHash("Flinch");
	private int resetHash = Animator.StringToHash("Reset");
	private int fwdDeadHash = Animator.StringToHash("DieFwd");
	private int bckDeadHash = Animator.StringToHash("DieBck");

	// Use this for initialization
	void Start () {
		InvMaxHealth = 1 / MaxHealth;
		health = MaxHealth;
		startingPos = transform.position;
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

		int weaponCount = weaponWrapper.transform.childCount;
		weaponModels = new GameObject[weaponCount];
		for(int i = 0; i < weaponCount; i++){
			weaponModels[i] = weaponWrapper.transform.GetChild(i).gameObject;
		}

		int weaponCount3 = weaponWrapper3.transform.childCount;
		weaponModels3 = new GameObject[weaponCount3];
		for(int i = 0; i < weaponCount3; i++){
			weaponModels3[i] = weaponWrapper3.transform.GetChild(i).gameObject;
		}

		weapons = new Weapon[weaponCount];

		// Storing weapon references to component scripts
		for (int i = 0; i < weaponCount; i++) {
			weapons[i] = weaponModels[i].GetComponent<Weapon>();
			weapons[i].SetPlayerReference(this);
			weapons[i].SetControllerReference(this.playerController);
			weapons[i].gameObject.SetActive(false);
		}

		weapons [currentWeaponIndex].gameObject.SetActive (true);

		// Setting player UI
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
		transform.position = startingPos;
		health = MaxHealth;
		healTimer = 0;
		display.UpdateDamageOverlay (0);

		// Enable firing layer
		anim.SetLayerWeight(1, 1);
		anim.SetTrigger(resetHash);

		networkManager.photonView.RPC ("PlayerRespawn", PhotonTargets.All, initializer.Layer - 1);

		foreach (Weapon weapon in weapons){
			weapon.ReplenishWeapon();
		}
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
		}
		else{
			isAimingDownSights = !isAimingDownSights;
			weapons[currentWeaponIndex].SetAds(isAimingDownSights);
		}

		return isAimingDownSights;
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
			weaponModels3[prevWeaponIndex].SetActive(false);
			weapons [currentWeaponIndex].gameObject.SetActive (true);
			weaponModels3 [currentWeaponIndex].SetActive(true);
			display.ActivateNewWeapon(currentWeaponIndex);
		}
		networkManager.photonView.RPC ("PlayerCycleWeapon", PhotonTargets.All, initializer.Layer - 1, currentWeaponIndex);
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage, Vector3 direction){
		print (health);

		if (!isDead){
			health -= damage;
			healTimer = HealWait;

			health = Mathf.Max (health, 0);

			display.IndicateDamageDirection (direction);
			display.UpdateDamageOverlay (1 - health * InvMaxHealth);

			if (health <= 0){
				isDead = true;
				respawnTimer = RespawnWait;

				// Disable firing layer
				anim.SetLayerWeight(1, 0);

				if (Vector3.Angle(direction, transform.forward) < 90){
					anim.SetTrigger(fwdDeadHash);
					networkManager.photonView.RPC ("PlayerDeath", PhotonTargets.All, initializer.Layer - 1, true);
				}
				else{
					anim.SetTrigger(bckDeadHash);
					networkManager.photonView.RPC ("PlayerDeath", PhotonTargets.All, initializer.Layer - 1, false);
				}
			}

			anim.SetTrigger(flinchHash);
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
