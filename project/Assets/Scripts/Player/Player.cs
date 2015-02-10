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

	public Camera playerCam;
	public Crosshair crossScript;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;
	public GameObject NetworkManager;
	
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
		tryRegen();
		crossScript.updateSpread (weapons [currentWeaponIndex].getSpread ());
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
		playerRenderer.InitializePlayerInterface(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].getSpread());

		// Setting controller
		playerController.setController(id);
	}

	public void SetToKeyboard(){
		playerController.isKeyboard = true;
	}

	// Regenerates if healing timer is depleted and health is below maximum
	void tryRegen(){
		if (health < MaxHealth){
			if (healTimer <= 0){
				health = Mathf.Min(MaxHealth, health + RegenInc * Time.deltaTime);
			}
			else{
				healTimer -= Time.deltaTime;
			}
		}
	}

	public bool toggleADS(bool? setADS = null){
		if (setADS != null) {
			isAimingDownSights = (bool)setADS;
			weapons[currentWeaponIndex].setAds(isAimingDownSights);
			NetworkManager.GetComponent<PlayerNetSend>().TogglePlayerADS(id, (bool)setADS);
		}
		else{
			isAimingDownSights = !isAimingDownSights;
			weapons[currentWeaponIndex].setAds(isAimingDownSights);
		}

		return isAimingDownSights;
	}

	public void setCrouching(bool crouchState){
		isCrouching = crouchState;
	}

	// Changes currently selected weapon
	public void cycleWeapons(int adjustment){
		int prevWeaponIndex = currentWeaponIndex;
		currentWeaponIndex += adjustment;
		if (currentWeaponIndex >= weapons.Length){
			currentWeaponIndex = 0;
		}
		else if (currentWeaponIndex < 0){
			currentWeaponIndex = weapons.Length - 1;
		}

		// Play weapon change animation

		// Activate new weapon
		if (prevWeaponIndex != currentWeaponIndex){
			weapons [prevWeaponIndex].gameObject.SetActive (false);
			weapons [currentWeaponIndex].gameObject.SetActive (true);
		}
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage){
		health -= damage;
		healTimer = HealWait;
	}

	public Weapon getCurrentWeapon(){
		return weapons [currentWeaponIndex];
	}

	// Attempts to fire bullet
	public void setFiringState(bool isFiring){
		weapons [currentWeaponIndex].setFiringState (isFiring);
	}
}
