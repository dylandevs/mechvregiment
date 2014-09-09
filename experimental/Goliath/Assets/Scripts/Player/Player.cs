/**
 * 
 * Tracks player attributes and handles behaviours
 * 
 **/

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour {

	int id = 0;

	const float HealWait = 5.0f;
	const float MaxHealth = 100;
	const float RegenInc = 0.7f;

	public Camera playerCam;
	public Crosshair crossScript;
	public PlayerViewport playerRenderer;
	public ControllerScript playerController;

	// Character models
	public GameObject firstPersonWrapper;
	public GameObject thirdPersonWrapper;
	public GameObject shadowCasterWrapper;
	public GameObject weaponModel;
	private List<GameObject> firstPersonModel = new List<GameObject>();
	private List<GameObject> thirdPersonModel = new List<GameObject>();
	private List<GameObject> shadowCasterModel = new List<GameObject>();

	public Shader invisShadowCastShader;

	float health = MaxHealth;
	float crossJitter = 0;
	float healTimer = 0;
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

		Initialize(1, new float[]{0, 1, 0, 1});
	}
	
	// Update is called once per frame
	void Update () {
		tryRegen();
	}

	public void Initialize(int playerId, float[] window){
		id = playerId;

		// Settings layers for models and hiding/showing to camera
		setModelLayer(firstPersonModel, "PlayerView1_" + id);
		setModelLayer(thirdPersonModel, "PlayerView3_" + id);

		// Setting weapon layers and storing references to component scripts
		for (int i = 0; i < weaponModels.Length; i++) {
			weaponModels[i].layer = LayerMask.NameToLayer("PlayerView1_" + id);
			weapons[i] = weaponModels[i].GetComponent<Weapon>();
			weapons[i].setPlayerReference(this);
			weapons[i].setControllerReference(this.playerController);
		}

		// Setting player UI
		playerRenderer.InitializePlayerInterface(window[0], window[1], window[2], window[3], weapons[currentWeaponIndex].getSpread());

		// TEMP
		weaponModel.layer = LayerMask.NameToLayer("PlayerView3_" + id);

		// Hiding/showing layers on player camera
		playerCam.cullingMask = ~(1 << thirdPersonModel[0].layer);
		playerCam.cullingMask |= (1 << firstPersonModel[0].layer);

		// Setting controller
		playerController.setController(playerId);

		createShadowCasterModel();
	}

	// Regenerates if healing timer is depleted and health is below maximum
	void tryRegen(){
		if (health < MaxHealth){
			if (healTimer <= 0){
				health = Mathf.Min(MaxHealth, health + RegenInc * Time.deltaTime);
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
		healTimer = HealWait;
		//print (health);
	}

	public Weapon getCurrentWeapon(){
		return weapons [currentWeaponIndex];
	}

	// Attempts to fire bullet
	public void tryFire(){
		weapons[currentWeaponIndex].FireBurst();
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
