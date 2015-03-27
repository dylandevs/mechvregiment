using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavUI : MonoBehaviour {

	// Inputs
	public Camera playerCam;
	public Camera uiCam;
	public Camera gunCam;
	public Camera deathCam;
	public PoolManager damageDirPool;
	public Player player;
	public CanvasScaler scaler;
	public Minimap minimap;

	public Image reloadGraphic;
	public Image damageGraphic;

	public CanvasGroup availableGunsWrapper;
	public CanvasGroup[] availableGuns;
	private Image[] availableGunGraphics;
	public Image currentGun;

	public float FlashedGunAlpha = 0.7f;
	public float InactiveGunAlpha = 0.5f;
	public float ActiveGunAlpha = 1;
	public float WeaponFlashLength = 0.5f;
	public float WeaponFadeTime = 1;
	private float weaponFadeRate = 1;
	
	public GameObject ammoManagerWrapper;
	private AmmoRenderManager[] ammoManagers;
	
	public UnityEngine.UI.Text magCount;
	public UnityEngine.UI.Text reserveCount;

	public GameObject crossTop;
	public GameObject crossRgt;
	public GameObject crossBot;
	public GameObject crossLft;

	public float HitMarkerFadeTime = 0.3f;
	private float hitMarkerFadeRate = 1;
	public CanvasGroup hitMarker;

	// Respawn elements
	public GameObject respawnPanel;
	public UnityEngine.UI.Text respawnTimer;
	private bool respawning = false;

	// In-game markers
	public RectTransform testMarker;

	[HideInInspector]
	public Camera skyCam;

	// Time trackers
	float weaponFlashProgress = 0;

	public RectTransform ownTransform;

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

		hitMarkerFadeRate = 1 / HitMarkerFadeTime;
	}
	
	// Update is called once per frame
	void Update () {
		print (ownTransform.sizeDelta);
		testMarker.anchoredPosition = (RectTransformUtility.WorldToScreenPoint (playerCam, minimap.objective.transform.position) - ownTransform.sizeDelta / 2f)  * (Screen.height / ownTransform.sizeDelta.y);
		print (testMarker.position);
		//testMarker.position = new Vector3(testMarker.position.x, testMarker.position.y, 0);

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

		if (hitMarker.alpha > 0){
			hitMarker.alpha -= Time.deltaTime * hitMarkerFadeRate;
		}

		if (respawning){
			respawnTimer.text = Mathf.Ceil(player.respawnTimer).ToString();
		}
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
		// Setting camera and ui scale
		uiCam.camera.rect = new Rect(x, y, width, height);
		playerCam.camera.rect = new Rect(x, y, width, height);
		gunCam.camera.rect = new Rect(x, y, width, height);
		skyCam.camera.rect = new Rect(x, y, width, height);
		deathCam.camera.rect = new Rect(x, y, width, height);
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

	public void UpdateCrosshairSpread(float spread){
		crossTop.transform.localPosition = new Vector3 (0, spread, 0);
		crossRgt.transform.localPosition = new Vector3 (spread, 0, 0);
		crossBot.transform.localPosition = new Vector3 (0, -spread, 0);
		crossLft.transform.localPosition = new Vector3 (-spread, 0, 0);
	}

	public void TriggerHitMarker(){
		hitMarker.alpha = 1;
	}

	public void StartRespawnSequence(float startTime){
		respawnPanel.SetActive (true);
		respawnTimer.text = Mathf.Ceil (startTime).ToString();
		respawning = true;
		deathCam.gameObject.SetActive (true);
	}

	public void EndRespawnSequence(){
		respawnPanel.SetActive (false);
		respawning = false;
		deathCam.gameObject.SetActive (false);
	}
}
