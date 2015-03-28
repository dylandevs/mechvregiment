using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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
	private Vector2 deltaScreenRatio = Vector2.zero;
	private Vector2 cachedSizeDelta;
	private Vector2 addScreenRatio;
	private Vector2 addSizeDelta;
	private List<InGameMarker> markers = new List<InGameMarker>();

	public GameObject markerPrefab;
	public Transform markerGroup;
	public Sprite objectiveMarker;
	public Sprite goliathMarker;
	public Sprite scavengerMarker;

	[HideInInspector]
	public Camera skyCam;

	// Time trackers
	float weaponFlashProgress = 0;

	public RectTransform markerTransform;

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
	
		// Create in-game markers
		GameObject marker;
		InGameMarker markerScript;

		marker = CreateInGameMarker(objectiveMarker);
		markerScript = marker.GetComponent<InGameMarker>();
		markerScript.associatedObject = minimap.objective;
		markers.Add(markerScript);

		marker = CreateInGameMarker(goliathMarker);
		markerScript = marker.GetComponent<InGameMarker>();
		markerScript.associatedObject = minimap.goliath;
		markers.Add(markerScript);

		foreach (Transform scavenger in minimap.playerGroup.transform){
			// Verify that current player is not selected
			if (scavenger.gameObject != player.gameObject){
				marker = CreateInGameMarker(scavengerMarker);
				markerScript = marker.GetComponent<InGameMarker>();
				markerScript.associatedObject = scavenger.gameObject;
				markers.Add(markerScript);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		// First-time value calculation after initialization
		if (deltaScreenRatio == Vector2.zero){
			deltaScreenRatio = Vector2.Scale(addScreenRatio, new Vector2(markerTransform.sizeDelta.x / Screen.width, markerTransform.sizeDelta.y / Screen.height));
			cachedSizeDelta = markerTransform.sizeDelta * 0.5f + Vector2.Scale(markerTransform.sizeDelta, addSizeDelta);
		}

		UpdateReloadProgress ();
		UpdateMarkerPositions ();

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

	private void UpdateMarkerPositions(){
		foreach(InGameMarker marker in markers){
			Vector3 targetPos = marker.associatedObject.transform.position + Vector3.up * 3;
			Vector3 difference = targetPos - playerCam.transform.position;

			if (Vector3.Dot(playerCam.transform.forward, difference) > 0 && !player.isDead && marker.associatedObject.GetActive()){
				marker.gameObject.SetActive(true);
				Vector2 initScreenPos = RectTransformUtility.WorldToScreenPoint (playerCam, targetPos);
				initScreenPos.x *= deltaScreenRatio.x;
				initScreenPos.y *= deltaScreenRatio.y;
				Vector2 screenPos = initScreenPos - cachedSizeDelta;

				marker.rectTransform.anchoredPosition = screenPos;
			}
			else{
				marker.gameObject.SetActive(false);
			}
		}
	}

	public GameObject CreateInGameMarker(Sprite sprite){
		GameObject marker = Instantiate (markerPrefab) as GameObject;
		marker.transform.SetParent (markerGroup);
		marker.transform.localPosition = Vector3.zero;
		marker.GetComponent<Image> ().sprite = sprite;

		return marker;
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
		addScreenRatio = new Vector2 (1 / width, 1 / height);
		addSizeDelta = new Vector2 (x * 2, y * 2);
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
