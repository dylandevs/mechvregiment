using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	// Weapon attributes
	public bool Automatic = true;
	public bool PhysicalAmmo = false;
	public float ReloadTime = 2;
	public float BurstTime = 0.1f;
	public float FireRate = 0.3f;
	public int BurstLength = 3;

	// Recoil variables
	public Vector2 RecoilPattern = Vector2.zero;
	public Vector2 RecoilVariance = Vector2.zero;
	public float FirstShotRecoilMulti = 2f;
	public float RecoilMoveTime = 0.2f;
	public float RecoilRecoverTime = 0.2f;
	public float AdsRecoilMoveFactor = 0.7f;

	// Spread variables and adjustments in different states
	public float BaseSpread = 15;
	public float FireSpreadRate = 6;
	public float FireSpreadRecoveryRate = 0.8f;
	public float SpreadAdjustmentRate = 0.4f;
	public float AdsSpreadAdjustmentFactor = 0.5f;
	public float AdsSpreadAdjust = -8;
	public float CrouchSpreadAdjust = -5;
	public float WalkSpreadAdjust = 5;
	public float RunSpreadAdjust = 10;
	public float JumpSpreadAdjust = 20;
	public float SprintSpreadAdjust = 20;

	public int BulletSpeed = 50;
	public int MagSize = 30;
	public int ReserveSize = 120;
	public float Damage = 15;

	// Inputs
	public GameObject generatorPos;
	public GameObject projectileObject;
	public GameObject impactObject;

	// Ammo trackers
	private int totalAmmo;
	private int magAmmo;

	// Time trackers
	private float reloadProgress = 0;
	private float burstProgress = 0;
	private float fireProgress = 0;
	private float recoilMoveProgress = 0;
	private float recoilRecoveryProgress = 0;

	// Spread tracker
	private float spread = 0;
	private float targetSpread = 0;
	private float fireSpread = 0;

	// Recoil tracker
	private Vector2 recoilTarget;
	private Vector3 originalFacing;
	private Quaternion totalRecoverRot;

	// State trackers
	private bool isReloading = false;
	private bool isBursting = false;
	private bool isOnFireInterval = false;
	private bool isAllAmmoDepleted = false;
	private bool isFiring = false;
	private int bulletsOfBurstFired = 0;
	private bool isFirstShot = true;
	private bool isAds = false;

	// External references
	private Player player;
	private ControllerScript controller;
	
	// Use this for initialization
	void Start () {
		totalAmmo = MagSize * 5;
		magAmmo = MagSize;
	}

	// Update is called once per frame
	void Update () {

		// If currently in firing state, attempt to fire burst
		if (isFiring) {
			FireBurst();
		}

		// Adjust recoil
		if (recoilMoveProgress > 0) {
			ApplyRecoil();
		}

		// Recover from recoil
		if (recoilRecoveryProgress > 0) {
			AttemptRecoilRecovery();
		}

		// Decrease fire-related spread quickly over time
		if (fireSpread > 0) {
			fireSpread -= FireSpreadRecoveryRate;

			if (fireSpread < 0){
				fireSpread = 0;
			}
		}

		// Adjust spread if not at target
		if (spread != targetSpread + fireSpread) {
			float spreadDiff = targetSpread + fireSpread - spread;
			// Either set spread to target, or approach
			if (Mathf.Abs(spreadDiff) < 0.05f ){
				spread = targetSpread + fireSpread;
			}
			else{
				spread += spreadDiff * SpreadAdjustmentRate;
			}
		}

		// Progress through fire interval (shortest time between shots)
		if (isOnFireInterval) {
			fireProgress -= Time.deltaTime;

			// Can fire again after
			if (fireProgress <= 0 && (Automatic || (!Automatic && !isFiring))){
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
			isFirstShot = true;
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
		Vector3 bulletOrigin = player.playerCam.transform.position;

		// Potentially randomize direction slightly
		Vector3 bulletDirection = controller.facing;

		// Apply spread variation
		if (spread > 0) {
			Quaternion vectorRotation = Quaternion.FromToRotation(Vector3.forward, bulletDirection);
			Vector2 newTarget = Random.insideUnitCircle * Mathf.Sqrt(spread) * 0.01f;
			Vector3 unrotatedFacing = new Vector3(newTarget.x, newTarget.y, 1);
			bulletDirection = (vectorRotation * unrotatedFacing).normalized;
		}

		// Either generate physical bullet or just have raycast
		if (PhysicalAmmo){
			GameObject bullet = Instantiate(projectileObject, bulletOrigin, Quaternion.identity) as GameObject;

			// Setting bullet properties
			Bullet bulletScript = bullet.GetComponent<Bullet>();
			bulletScript.setProperties(Damage, player.tag, bulletDirection, BulletSpeed);
		}
		else{
			RaycastHit rayHit;
			if (Physics.Raycast(bulletOrigin, bulletDirection, out rayHit, 1000)){
				//print (travelDist);
				
				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					Instantiate(impactObject, rayHit.point + rayHit.normal * 0.01f, hitRotation);
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					Player playerHit = rayHit.collider.GetComponent<Player>();
					playerHit.Damage(Damage);
					//print("hit player");
				}
				else if (rayHit.collider.gameObject.tag == "Enemy"){
					BotAI botHit = rayHit.collider.GetComponent<BotAI>();
					botHit.Damage(Damage);
					//print("hit enemy");
				}
			}
		}

		// Update fire-related spread
		if (isAds) {
			fireSpread += FireSpreadRate * AdsSpreadAdjustmentFactor;
		}
		else{
			fireSpread += FireSpreadRate;
		}


		SetRecoilTarget ();

		magAmmo--;
	}

	// Sets tracking variables for recoil
	private void SetRecoilTarget(){
		recoilMoveProgress = RecoilMoveTime;
		recoilTarget = RecoilPattern;
		originalFacing = controller.facing;
		if (isAds) {
			recoilTarget *= AdsRecoilMoveFactor;
		}
		if (isFirstShot) {
			recoilTarget *= FirstShotRecoilMulti;
			isFirstShot = false;
		}
	}

	private void ApplyRecoil(){
		recoilMoveProgress -= Time.deltaTime;
		Vector3 newFacing = controller.facing;

		float recoilProg = (RecoilMoveTime - recoilMoveProgress) / RecoilMoveTime;
		recoilProg = Mathf.Sqrt(recoilProg);

		float yRaw = recoilTarget.y + Random.Range(-RecoilVariance.y, RecoilVariance.y);
		float xRaw = recoilTarget.x + Random.Range(-RecoilVariance.x, RecoilVariance.x);

		float yAdjust = Mathf.Lerp (0, yRaw, recoilProg);
		float xAdjust = Mathf.Lerp (0, xRaw, recoilProg);

		Quaternion verticalAdjust = Quaternion.identity;
		float newVertAngle = Vector3.Angle(newFacing, Vector3.up) - yAdjust;
		if (newVertAngle > 170){
			// Do nothing
		}
		else if (newVertAngle < 10){
			// Do nothing
		}
		else{
			verticalAdjust = Quaternion.AngleAxis (-yAdjust, controller.perpFacing);
		}

		Quaternion horizontalAdjust = Quaternion.AngleAxis (xAdjust, Vector3.up);

		newFacing = verticalAdjust * horizontalAdjust * newFacing;
		controller.setFacing (newFacing);

		// Finished applying recoil, begin recoil recovery
		if (recoilMoveProgress <= 0) {
			recoilMoveProgress = 0;
			recoilRecoveryProgress = RecoilRecoverTime;
		}
	}

	private void AttemptRecoilRecovery(){
		recoilRecoveryProgress -= Time.deltaTime;
		Vector3 newFacing = controller.facing;

		float recoveryProg = (RecoilRecoverTime - recoilRecoveryProgress) / RecoilRecoverTime;
		recoveryProg = Mathf.Sqrt(recoveryProg);

		//newFacing = (newFacing + originalFacing) * RecoilRecoverRate;

		totalRecoverRot = Quaternion.FromToRotation (newFacing, originalFacing);
		Quaternion currentRecoverRot = Quaternion.Lerp(Quaternion.identity, totalRecoverRot, recoveryProg);

		newFacing = currentRecoverRot * newFacing;
		controller.setFacing (newFacing);

		//Vector3 diff = newFacing - originalFacing;

		// Finished recovering recoil
		if (recoilRecoveryProgress <= 0) {
			recoilRecoveryProgress = 0;
			controller.setFacing (originalFacing);
		}
	}

	public void setFiringState(bool firingState){
		//player.setFiringState(true);
		isFiring = firingState;
	}

	private void StartFireInterval(){
		isOnFireInterval = true;
		fireProgress = FireRate;
	}

	private void StopFireInterval(){
		isOnFireInterval = false;
		fireProgress = 0;
	}

	private void StartBursting(){
		isBursting = true;
		burstProgress = BurstTime;
	}

	private void StopBursting(){
		isBursting = false;
		burstProgress = 0;
		bulletsOfBurstFired = 0;
	}

	private void StartReloading(){
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

	private void StopReloading(){
		reloadProgress = 0;
		isReloading = false;
		print ("Reloaded");
	}

	private void Reload(){
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

	public void setAds(bool adsSetting){
		isAds = adsSetting;
	}

}
