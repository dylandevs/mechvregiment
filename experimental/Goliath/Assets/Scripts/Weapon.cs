using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	// Weapon attributes
	public float RELOAD_TIME = 2;
	public float BURST_TIME = 0;
	public float FIRE_RATE = 0.5f;
	public int BURST_LENGTH = 1;
	public bool AUTOMATIC = false;
	public Vector2 RECOIL_PATTERN = Vector2.zero;
	public float BASE_SPREAD = 0;
	public bool ALTERNATING_RECOIL = false;

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
	
	// Use this for initialization
	void Start () {
		totalAmmo = MAG_SIZE * 5;
		magAmmo = MAG_SIZE;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Generates projectile at specified generation position
	public void Fire(){
		if (magAmmo > 0) {
			//Instantiate (projectileObject, generatorPos.transform.position, transform.rotation);
			magAmmo--;
			print (magAmmo);
		}
		else{
			print ("empty");
		}
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
