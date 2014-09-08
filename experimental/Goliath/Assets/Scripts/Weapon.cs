using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	// Weapon attributes
	public float RELOAD_TIME = 2;
	public float BURST_TIME = 0.1f;
	public float FIRE_RATE = 0.5f;
	public int BURST_LENGTH = 1;
	public bool AUTOMATIC = true;
	public bool ALTERNATING_RECOIL = false;
	public Vector2 RECOIL_PATTERN = Vector2.zero;

	public float BASE_SPREAD = 10;
	public float SPREAD_RATE = 1;

	public int BULLET_SPEED = 50;
	public int MAG_SIZE = 30;
	public int RESERVE_SIZE = 150;
	public float DAMAGE = 15;

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

	// Recoil tracker
	private float recoil = 0;

	// State trackers
	bool isReloading = false;
	bool isBursting = false;
	bool isOnFireInterval = false;
	int bulletsOfBurstFired = 0;

	// External references
	Player player;
	ControllerScript controller;
	
	// Use this for initialization
	void Start () {
		spread = BASE_SPREAD;
		totalAmmo = MAG_SIZE * 5;
		magAmmo = MAG_SIZE;
	}

	// Update is called once per frame
	void Update () {

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
				Fire ();
				burstProgress = BURST_TIME;

				// Check if burst has finished
				if (bulletsOfBurstFired++ >= BURST_LENGTH){
					StopBursting();
					StartFireInterval();
				}
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
		if (!isBursting && !isOnFireInterval){
			// Begins burst
			if (BURST_LENGTH > 1) {
				Fire ();
				StartBursting();
				bulletsOfBurstFired++;
			}
			// Single shot, no burst firing
			else{
				Fire ();
				StartFireInterval();
			}
		}
	}

	// Generates projectile at specified generation position
	private void Fire(){
		if (magAmmo > 0) {
			// Creates bullet at given position
			Vector3 bulletOrigin = controller.transform.position + controller.facing + controller.cameraOffset;
			GameObject bullet = Instantiate(projectileObject, bulletOrigin, Quaternion.identity) as GameObject;

			// Setting bullet properties
			Bullet bulletScript = bullet.GetComponent<Bullet>();
			bulletScript.setProperties(DAMAGE, player.tag, controller.facing, BULLET_SPEED);

			print (magAmmo--);
		}
		else{
			print ("empty");
		}
	}

	public void StartFireInterval(){
		isOnFireInterval = true;
		fireProgress = FIRE_RATE;
	}

	public void StopFireInterval(){
		isOnFireInterval = false;
		fireProgress = 0;
	}

	public void StartBursting(){
		isBursting = true;
		burstProgress = BURST_TIME;
	}

	public void StopBursting(){
		isBursting = false;
		burstProgress = 0;
		bulletsOfBurstFired = 0;
	}

	public void StartReloading(){
		reloadProgress = RELOAD_TIME;
		isReloading = true;
	}

	public void StopReloading(){
		reloadProgress = 0;
		isReloading = false;
	}

	public void Reload(){
		int requiredAmmo = MAG_SIZE - magAmmo;
		if (totalAmmo >= requiredAmmo) {
			magAmmo = MAG_SIZE;
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
		return spread;
	}

	public float getRecoil(){
		return recoil;
	}
}
