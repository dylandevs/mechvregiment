using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavUI : MonoBehaviour {

	// Inputs
	public Camera uiCam;
	public PoolManager damageDirPool;
	public Player player;

	public Image reloadGraphic;
	public Image damageGraphic;

	private float[] renderWindow = new float[]{0, 0, 1, 1};

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		UpdateReloadProgress ();
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

	public void Initialize(float xLow, float xHigh, float yLow, float yHigh, float baseWeaponSpread = 0){
		renderWindow[0] = xLow;
		renderWindow[1] = xHigh;
		renderWindow[2] = yLow;
		renderWindow[3] = yHigh;

		uiCam.camera.rect = new Rect(renderWindow[0], renderWindow[2], renderWindow[1] - renderWindow[0], renderWindow[3] - renderWindow[2]);
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
}
