using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	// Weapon attributes
	public float ReloadTime = 2;
	public float BurstTime = 0.1f;
	public float FireRate = 0.3f;
	public int BurstLength = 3;
	public bool Automatic = true;
	public bool AlternatingRecoil = false;
	public Vector2 RecoilPattern = Vector2.zero;

	// Spread variables and adjustments in different states
	public float BaseSpread = 10;
	public float SpreadRate = 1;
	public float SpreadAdjustmentRate = 0.5f;
	public float AdsSpreadAdjust = -4;
	public float CrouchSpreadAdjust = -5;
	public float RunSpreadAdjust = 10;
	public float JumpSpreadAdjust = 10;
	public float SprintSpreadAdjust = 20;

	public int BulletSpeed = 50;
	public int MagSize = 30;
	public int ReserveSize = 120;
	public float Damage = 15;

	// Inputs
	public GameObject generatorPos;
	public GameObject projectileObject;

	// Ammo trackers
	private int totalAmmo;
	private int magAmmo;

	// Time trackers
	private float reloadProgress = 0;
	private float burstProgress = 0;
	private float fireProgress = 0;

	// Spread tracker
	private float spread = 0;
	private float targetSpread = 0;

	// Recoil tracker
	private float recoil = 0;

	// State trackers
	bool isReloading = false;
	bool isBursting = false;
	bool isOnFireInterval = false;
	bool isAllAmmoDepleted = false;
	int bulletsOfBurstFired = 0;

	// External references
	Player player;
	ControllerScript controller;
	
	// Use this for initialization
	void Start () {
		totalAmmo = MagSize * 5;
		magAmmo = MagSize;
	}

	// Update is called once per frame
	void Update () {

		// Adjust spread if not at target
		if (spread != targetSpread) {
			float spreadDiff = targetSpread - spread;
			// Either set spread to target, or approach
			if (Mathf.Abs(spreadDiff) < 0.05f ){
				spread = targetSpread;
			}
			else{
				spread += spreadDiff * SpreadAdjustmentRate;
			}
		}

		// Progress through fire interval (shortest time between shots)
		if (isOnFireInterval) {
			fireProgress -= Time.deltaTime;

			// Can fire again after
			if (fireProgress <= 0){
				StopFireInterval();
			}
		}

		// Progress through reload
		if (isReloading) {
			reloadProgress -= Time.deltaTime;

			// Perform reload action at end if uninterrupted
			if (reloadProgress <= 0){
				Reload();
				StopReloading();
			}
		}

		// Progress through burst, firing bullets automatically as time elapses
		if (isBursting) {
			burstProgress -= Time.deltaTime;

			// Check if time to fire bullet automatically
			if (burstProgress <= 0){

				// Check if burst has finished
				if (bulletsOfBurstFired++ >= BurstLength){
					StopBursting();
					StartFireInterval();
				}
				else{
					Fire ();
					burstProgress = BurstTime;
				}
			}
		}

		// Attempt to reload if possible
		if (!isReloading){
			if (magAmmo == 0){
				StopBursting();
				StopFireInterval();
				StartReloading();
			}
		}
	}

	public void setPlayerReference(Player player){
		this.player = player;
	}

	public void setControllerReference(ControllerScript controller){
		this.controller = controller;
	}

	public void FireBurst(){
		if (!isBursting && !isOnFireInterval && !isReloading && !isAllAmmoDepleted){
			// Begins burst
			print (BurstLength);
			if (BurstLength > 1) {
				StartBursting();
				Fire ();
				bulletsOfBurstFired++;
			}
			// Single shot, no burst firing
			else{
				StartFireInterval();
				Fire ();
			}
		}
	}

	// Generates projectile at specified generation position
	private void Fire(){
		// Creates bullet at given position
		Vector3 bulletOrigin = controller.transform.position + controller.facing + controller.cameraOffset;
		GameObject bullet = Instantiate(projectileObject, bulletOrigin, Quaternion.identity) as GameObject;

		// Setting bullet properties
		Bullet bulletScript = bullet.GetComponent<Bullet>();
		bulletScript.setProperties(Damage, player.tag, controller.facing, BulletSpeed);

		// Update spread
		spread += SpreadRate;

		magAmmo--;
	}

	public void StartFireInterval(){
		isOnFireInterval = true;
		fireProgress = FireRate;
	}

	public void StopFireInterval(){
		isOnFireInterval = false;
		fireProgress = 0;
	}

	public void StartBursting(){
		isBursting = true;
		burstProgress = BurstTime;
	}

	public void StopBursting(){
		isBursting = false;
		burstProgress = 0;
		bulletsOfBurstFired = 0;
	}

	public void StartReloading(){
		// Only reload if some reserve left
		if (totalAmmo > 0){
			reloadProgress = ReloadTime;
			isReloading = true;
			print ("Reloading...");
		}
		else{
			print ("All empty!");
			isAllAmmoDepleted = true;
		}
	}

	public void StopReloading(){
		reloadProgress = 0;
		isReloading = false;
		print ("Reloaded");
	}

	public void Reload(){
		int requiredAmmo = MagSize - magAmmo;
		if (totalAmmo >= requiredAmmo) {
			magAmmo = MagSize;
			totalAmmo -= requiredAmmo;
		} else {
			magAmmo += totalAmmo;
			totalAmmo = 0;
		}
	}

	// Returns number of bullets in current magazine, and number of bullets in reserve
	public int[] getAmmoCount(){
		int[] toReturn = new int[2];
		toReturn [0] = magAmmo;
		toReturn [1] = totalAmmo;

		return toReturn;
	}

	public float getSpread(){
		return spread + BaseSpread;
	}

	public void setTargetSpread(float newSpread){
		targetSpread = newSpread;
	}

	public float getRecoil(){
		return recoil;
	}
}
