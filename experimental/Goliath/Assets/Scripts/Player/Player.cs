﻿/**
 * 
 * Tracks player attributes and handles behaviours
 * 
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	// Faction to determine friendly fire and other mechanics
	public string faction = "Player";
	int id = 0;

	const float HEAL_WAIT = 5.0f;
	const float MAX_HEALTH = 100;
	const float REGEN_INC = 0.7f;
	const float FIRE_RATE = 0.2f;

	public Camera playerCam;
	public Crosshair crossScript;
	public GameObject ammunition;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;

	// Character models
	public GameObject firstPersonWrapper;
	public GameObject thirdPersonWrapper;
	public GameObject shadowCasterWrapper;
	public GameObject weaponModel;
	private List<GameObject> firstPersonModel = new List<GameObject>();
	private List<GameObject> thirdPersonModel  = new List<GameObject>();
	private List<GameObject> shadowCasterModel  = new List<GameObject>();

	public Shader invisShadowCastShader;

	float health = MAX_HEALTH;
	float crossJitter = 0;
	float healTimer = 0;
	float fireTimer = 0;
	bool isAimingDownSights = false;

	public GameObject[] weaponModels = new GameObject[]{};
	private Weapon[] weapons;
	int currentWeaponIndex = 0;

	// Use this for initialization
	void Start () {
		weapons = new Weapon[weaponModels.Length];

		// Getting models from parents
		foreach (Transform child in firstPersonWrapper.transform){
			firstPersonModel.Add(child.gameObject);
		}
		foreach (Transform child in thirdPersonWrapper.transform){
			thirdPersonModel.Add(child.gameObject);
		}

		Initialize("Player", 1, new float[]{0, 1, 0, 1});
		//anim.Play("PlayerIdle");
	}
	
	// Update is called once per frame
	void Update () {
		tryRegen();
		if (fireTimer > 0){
			fireTimer -= Time.deltaTime;
		}
		//Debug.Log(anim.GetFloat(speedHash));
	}

	public void Initialize(string newFaction, int playerId, float[] window){
		id = playerId;
		faction = newFaction;

		// Setting controller and render area
		playerController.setController(playerId);
		playerRenderer.setWindow(window[0], window[1], window[2], window[3]);

		// Settings layers for models and hiding/showing to camera
		//print(id);
		setModelLayer(firstPersonModel, "PlayerView1_" + id);
		setModelLayer(thirdPersonModel, "PlayerView3_" + id);

		// NOTE: TEMPORARY
		for (int i = 0; i < weaponModels.Length; i++) {
			weaponModels[i].layer = LayerMask.NameToLayer("PlayerView1_" + id);
			weapons[i] = weaponModels[i].GetComponent<Weapon>();
		}

		weaponModel.layer = LayerMask.NameToLayer("PlayerView3_" + id);

		//firstPersonModel.layer = LayerMask.NameToLayer("PlayerView1_" + id);
		//thirdPersonModel.layer = LayerMask.NameToLayer("PlayerView3_" + id);
		playerCam.cullingMask = ~(1 << thirdPersonModel[0].layer);
		playerCam.cullingMask |= (1 << firstPersonModel[0].layer);

		createShadowCasterModel();
	}

	// Regenerates if healing timer is depleted and health is below maximum
	void tryRegen(){
		if (health < MAX_HEALTH){
			if (healTimer <= 0){
				health = Mathf.Min(MAX_HEALTH, health + REGEN_INC * Time.deltaTime);
				//print (health);
			}
			else{
				healTimer -= Time.deltaTime;
			}
		}
	}

	public bool toggleADS(bool? setADS = null){
		if (setADS != null) {
			isAimingDownSights = (bool)setADS;
		}
		else{
			isAimingDownSights = !isAimingDownSights;
		}

		return isAimingDownSights;
	}

	// Deals damage to player and resets healing timer
	public void Damage(float damage){
		//print (damage);
		health -= damage;
		healTimer = HEAL_WAIT;
		//print (health);
	}

	// Attempts to fire bullet
	public void tryFire(Vector3 fireDir, Vector3 bulletPos){
		if (fireTimer <= 0){

			GameObject bullet = Instantiate(ammunition, bulletPos, Quaternion.identity) as GameObject;
			Bullet bulletScript = bullet.GetComponent<Bullet>();
			bulletScript.setProperties(15, gameObject.tag, fireDir, 40);
			
			fireTimer = FIRE_RATE;
			weapons[currentWeaponIndex].Fire();
		}
	}

	private void setModelLayer(List<GameObject> model, string newLayer){
		foreach(GameObject obj in model){
			obj.layer = LayerMask.NameToLayer(newLayer);
		}
	}

	private void setModelShader(List<GameObject> model, Shader newShader){
		foreach(GameObject obj in model){
			obj.renderer.material.shader = newShader;
		}
	}

	private void createShadowCasterModel(){
		// Duplicating third-person model
		// shadowCasterWrapper = Instantiate(thirdPersonWrapper) as GameObject;

		// Getting model from parent
		foreach (Transform child in shadowCasterWrapper.transform){
			shadowCasterModel.Add(child.gameObject);
		}

		//shadowCasterWrapper.transform.parent = transform;
		setModelLayer(shadowCasterModel, "PlayerView1_" + id);
		setModelShader(shadowCasterModel, invisShadowCastShader);
	}
}
