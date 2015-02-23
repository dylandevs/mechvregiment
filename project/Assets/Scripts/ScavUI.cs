﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavUI : MonoBehaviour {

	// Inputs
	public Camera uiCam;
	public PoolManager damageDirPool;
	public Player player;
	public CanvasScaler scaler;

	public Image reloadGraphic;
	public Image damageGraphic;

	public CanvasGroup availableGunsWrapper;
	public CanvasGroup[] availableGuns;
	private Image[] availableGunGraphics;
	public Image currentGun;

	public GameObject ammoManagerWrapper;
	private AmmoRenderManager[] ammoManagers;

	public UnityEngine.UI.Text magCount;
	public UnityEngine.UI.Text reserveCount;

	public float FlashedGunAlpha = 0.7f;
	public float InactiveGunAlpha = 0.5f;
	public float ActiveGunAlpha = 1;
	public float WeaponFlashLength = 0.5f;
	public float WeaponFadeTime = 1;
	private float weaponFadeRate = 1;

	// Time trackers
	float weaponFlashProgress = 0;

	// Render area holder
	private float[] renderWindow = new float[]{0, 0, 1, 1};

	// Use this for initialization
	void Start () {
		// Getting graphics of different weapons
		availableGunGraphics = new Image[availableGuns.Length];
		for (int i = 0; i < availableGuns.Length; i++){
			availableGunGraphics[i] = availableGuns[i].transform.GetChild(0).GetComponent<Image>();
		}

		weaponFadeRate = FlashedGunAlpha / WeaponFadeTime;

		// Getting ammo managers
		ammoManagers = new AmmoRenderManager[ammoManagerWrapper.transform.childCount];
		for (int i = 0; i < ammoManagerWrapper.transform.childCount; i++){
			ammoManagers[i] = ammoManagerWrapper.transform.GetChild(i).GetComponent<AmmoRenderManager>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdateReloadProgress ();

		if (weaponFlashProgress > 0){
			weaponFlashProgress -= Time.deltaTime;

			if (weaponFlashProgress < 0){
				weaponFlashProgress = 0;
			}
		}
		else if (availableGunsWrapper.alpha > 0){
			availableGunsWrapper.alpha -= Time.deltaTime * weaponFadeRate;
		}

		int[] ammoCounts = player.GetCurrentWeapon().GetAmmoCount();
		UpdateAmmoCount(ammoCounts[0], ammoCounts[1]);
	}

	public void IndicateDamageDirection(Vector3 hitVector){

		// Determining rotation relative to player forward
		Quaternion rot = Quaternion.identity;
		Vector3 flatVector = hitVector;
		flatVector.y = 0;

		rot = Quaternion.FromToRotation (player.transform.forward, flatVector);

		GameObject indicator = damageDirPool.Retrieve ();
		DamageDir indicatorScript = indicator.GetComponent<DamageDir> ();
		indicatorScript.Initialize (player, rot);
	}

	public void Initialize(float x, float y, float width, float height, float uiScale = 1, float baseWeaponSpread = 0){
		uiCam.camera.rect = new Rect(x, y, width, height);
		scaler.referenceResolution = scaler.referenceResolution * uiScale;
	}

	public void UpdateReloadProgress(){
		Weapon currWeapon = player.GetCurrentWeapon ();
		if (currWeapon.IsReloading()){
			float progress = currWeapon.GetReloadProgress();
			reloadGraphic.fillAmount = progress;
			reloadGraphic.color = new Color(1, 1, 1, 1 - progress);
		}
		else{
			reloadGraphic.color = new Color(1, 1, 1, 0);
		}
	}

	public void UpdateDamageOverlay(float newVal){
		damageGraphic.color = new Color (1, 1, 1, newVal);
	}

	// Switch weapon, show all available choices
	public void ActivateNewWeapon(int newWeaponIndex){
		availableGunsWrapper.alpha = FlashedGunAlpha;

		foreach (CanvasGroup gun in availableGuns){
			gun.alpha = InactiveGunAlpha;
		}

		availableGuns [newWeaponIndex].alpha = ActiveGunAlpha;
		currentGun.sprite = availableGunGraphics [newWeaponIndex].sprite;
		weaponFlashProgress = WeaponFlashLength;

		foreach (AmmoRenderManager ammo in ammoManagers){
			ammo.DeactivateRender();
		}

		ammoManagers [newWeaponIndex].ActivateRender ();
	}

	public void UpdateAmmoCount(int magAmmo, int reserveAmmo){
		magCount.text = magAmmo.ToString();
		reserveCount.text = reserveAmmo.ToString();
	}
}
