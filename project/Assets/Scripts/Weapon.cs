using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {
	
	// Weapon attributes
	public bool Automatic = true;
	public bool PhysicalAmmo = false;
	public float ReloadTime = 2;
	private float InvReloadTime = 1;
	public float BurstTime = 0.1f;
	public float FireRate = 0.3f;
	public int BurstLength = 3;
	
	// Recoil variables
	public Vector2 RecoilPattern = Vector2.zero;
	public Vector2 RecoilVariance = Vector2.zero;
	public float FirstShotRecoilMulti = 2f;
	public float RecoilMoveTime = 0.2f;
	public float RecenteringTime = 0.2f;
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

	// Cosmetic attributes
	public float FlashDuration = 0.05f;

	// Inputs
	public AmmoRenderManager ammoRenderer;
	public GameObject generatorPos;
	public PoolManager projectilePool;
	public PoolManager impactPool;
	public Animator animator;
	public GameObject flash;
	public ParticleSystem smoke;
	
	// Ammo trackers
	private int reserveAmmo;
	private int magAmmo;
	
	// Time trackers
	private float reloadProgress = 0;
	private float burstProgress = 0;
	private float fireProgress = 0;
	private float recoilMoveProgress = 0;
	private float recentringProgress = 0;
	private float flashProgress = 0;
	
	// Spread tracker
	private float spread = 0;
	private float targetSpread = 0;
	private float fireSpread = 0;
	
	// Recoil tracker
	private bool recenterTargetSet = false;
	private Vector2 recoilTarget;
	private Vector3 originalFacing;
	private Vector3 originalOffset;
	private float totalRecenterRotation = 0;
	private float initialFiringElevation = 0;
	private float currentRecenterRotation = 0;
	
	// State trackers
	private bool isReloading = false;
	private bool isBursting = false;
	private bool isRecoiling = false;
	private bool isOnFireInterval = false;
	private bool isAllAmmoDepleted = false;
	private bool isFiring = false;
	private int bulletsOfBurstFired = 0;
	private bool isFirstShot = true;
	private bool isAds = false;
	private bool semiAutoReady = true;
	
	// Cached references
	private Player player;
	private ControllerScript controller;

	void Awake(){
		ammoRenderer.Initialize (MagSize);
	}

	// Use this for initialization
	void Start () {
		reserveAmmo = ReserveSize;
		magAmmo = MagSize;
		InvReloadTime = 1 / ReloadTime;
	}
	
	// Update is called once per frame
	void Update () {

		// Progress through reload
		if (isReloading) {
			reloadProgress -= Time.deltaTime;
			
			// Perform reload action at end if uninterrupted
			if (reloadProgress <= 0){
				Reload();
				StopReloading();
			}
		}
		else{
			if (magAmmo == 0){
				TryReloading();
			}
		}

		// If currently in firing state, attempt to fire burst
		if ((isFiring && Automatic) || (semiAutoReady && !Automatic && isFiring)) {
			if (!isAllAmmoDepleted && !recenterTargetSet){
				SetRecenteringTarget();
			}
			FireBurst();

			if (!Automatic){
				semiAutoReady = false;
				isFiring = false;
			}
		}
		else if (recenterTargetSet && !isRecoiling){
			if (!isAllAmmoDepleted && player.rigidbody.velocity.magnitude <= 0.1f || (!isAllAmmoDepleted && player.rigidbody.velocity.magnitude <= 0.1f && isReloading)){
				CalculateRecenteringSteps();
			}
			else{
				EndRecentering();
			}
		}

		// Adjust recoil
		if (isRecoiling) {
			ApplyRecoil();
			//isRecoiling = false;
		}
		// Recover from recoil
		else if (recentringProgress > 0 && !isFiring) {
			AttemptRecentering();
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

		// Hide flash after duration
		if (flashProgress > 0){
			flashProgress -= Time.deltaTime;

			if (flashProgress <= 0){
				flashProgress = 0;
				flash.SetActive(false);
			}
		}
	}
	
	public void SetPlayerReference(Player player){
		this.player = player;
	}
	
	public void SetControllerReference(ControllerScript controller){
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
			GameObject bullet = projectilePool.Retrieve(bulletOrigin, Quaternion.identity);
			
			// Setting bullet properties
			Bullet bulletScript = bullet.GetComponent<Bullet>();
			bulletScript.setProperties(Damage, player.tag, bulletDirection, BulletSpeed, impactPool);
			bulletScript.playerSource = player;
		}
		else{
			RaycastHit rayHit;
			if (Physics.Raycast(bulletOrigin, bulletDirection, out rayHit, 1000)){
				//print (travelDist);
				
				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					GameObject mark = impactPool.Retrieve(rayHit.point + rayHit.normal * 0.01f, hitRotation);
					mark.GetComponent<BulletHoleBehaviour>().Initialize();
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					Player playerHit = rayHit.collider.GetComponent<Player>();
					playerHit.Damage(Damage, bulletDirection);
					player.TriggerHitMarker();
				}
				else if (rayHit.collider.gameObject.tag == "Enemy"){
					BotAI botHit = rayHit.collider.GetComponent<BotAI>();
					botHit.Damage(Damage);
					player.TriggerHitMarker();
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

		// Activate muzzle flash (if one is given)
		if (flash){
			flash.transform.RotateAround(flash.transform.position, flash.transform.up, Random.Range (0, 360));
			flash.SetActive (true);
			flashProgress = FlashDuration;
		}

		// Create smoke puff
		if (smoke){
			smoke.Play ();
		}
		
		SetRecoilTarget ();
		
		magAmmo--;
		ammoRenderer.ExpendSingleRound ();
	}
	
	// Sets tracking variables for recoil
	private void SetRecoilTarget(){
		isRecoiling = true;
		recoilMoveProgress = RecoilMoveTime;
		recoilTarget = RecoilPattern;
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
		controller.SetFacing (newFacing);
		
		// Finished applying recoil
		if (recoilMoveProgress <= 0) {
			recoilMoveProgress = 0;
			isRecoiling = false;
		}
	}
	
	private void SetRecenteringTarget(){
		EndRecentering();
		
		Quaternion resetRotation = Quaternion.FromToRotation(player.transform.forward, Vector3.forward);
		
		Quaternion offsetRotation = Quaternion.FromToRotation(Vector3.forward, resetRotation * controller.facing);
		initialFiringElevation = offsetRotation.eulerAngles.x;
		
		// Converting to account for quaternion values
		if (initialFiringElevation > 180){
			initialFiringElevation = 360 - initialFiringElevation;
		}
		else{
			initialFiringElevation *= -1;
		}
		
		recenterTargetSet = true;
		recentringProgress = RecenteringTime;
	}
	
	private void CalculateRecenteringSteps(){
		Quaternion resetRotation = Quaternion.FromToRotation(player.transform.forward, Vector3.forward);
		
		Quaternion offsetRotation = Quaternion.FromToRotation(Vector3.forward, resetRotation * controller.facing);
		totalRecenterRotation = offsetRotation.eulerAngles.x;
		currentRecenterRotation = 0;
		
		// Converting to account for quaternion values
		if (totalRecenterRotation > 180){
			totalRecenterRotation = 360 - totalRecenterRotation;
		}
		else{
			totalRecenterRotation *= -1;
		}
		
		// Calculate final rotation to perform for recentering
		totalRecenterRotation = totalRecenterRotation - initialFiringElevation;
		//totalRecenterRotation *= 0.5f;
		recenterTargetSet = false;
	}
	
	private void AttemptRecentering(){
		recentringProgress -= Time.deltaTime;
		Vector3 newFacing = controller.facing;
		float recenteringProg = (RecenteringTime - recentringProgress) / RecenteringTime;
		recenteringProg = Mathf.Min(1, Mathf.Sqrt(recenteringProg));
		
		//print (totalRecenterRotation);
		
		float recenteringStep = Mathf.Lerp(0, totalRecenterRotation, recenteringProg);
		recenteringStep -= currentRecenterRotation;
		currentRecenterRotation += recenteringStep;
		
		//print (recenteringStep);
		
		// Apply rotation step
		newFacing = Quaternion.AngleAxis(recenteringStep, controller.perpFacing) * newFacing;
		controller.SetFacing (newFacing);
		
		// Finished recentering
		if (recentringProgress <= 0) {
			//controller.setFacing (originalFacing);
			EndRecentering();
		}
	}
	
	private void EndRecentering(){
		recentringProgress = 0;
		totalRecenterRotation = 0;
		recenterTargetSet = false;
	}
	
	public void setFiringState(bool firingState){
		if (!isReloading){
			isFiring = firingState;
		}
		else if (isAllAmmoDepleted){
			// TODO: Empty weapon sound
		}

		// Changes firing state based on semi-automatic setting
		if (!semiAutoReady){
			if (!isFiring){
				semiAutoReady = true;
			}
			else{
				isFiring = false;
			}
		}
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
		reloadProgress = ReloadTime;
		isReloading = true;
		isFiring = false;
	}
	
	public void StopReloading(){
		reloadProgress = 0;
		isReloading = false;
	}
	
	private void Reload(){
		int requiredAmmo = MagSize - magAmmo;
		if (reserveAmmo >= requiredAmmo) {
			magAmmo = MagSize;
			reserveAmmo -= requiredAmmo;
		} else {
			magAmmo += reserveAmmo;
			reserveAmmo = 0;
		}

		ammoRenderer.Reload (magAmmo);
	}
	
	// Returns number of bullets in current magazine, and number of bullets in reserve
	public int[] GetAmmoCount(){
		int[] toReturn = new int[2];
		toReturn [0] = magAmmo;
		toReturn [1] = reserveAmmo;
		
		return toReturn;
	}
	
	public float GetSpread(){
		return spread + BaseSpread;
	}
	
	public void SetTargetSpread(float newSpread){
		targetSpread = newSpread;
	}
	
	public void SetAds(bool adsSetting){
		isAds = adsSetting;
	}
	
	public bool IsReloading(){
		return isReloading;
	}

	public void TryReloading(){
		// Only reload if some reserve left
		if (reserveAmmo + magAmmo == 0){
			isAllAmmoDepleted = true;
		}
		else if (magAmmo < MagSize && !isReloading && reserveAmmo > 0){
			StopBursting();
			StopFireInterval();
			if (!isAllAmmoDepleted){
				StartReloading();
			}
		}
	}

	public float GetReloadProgress(){
		return 1 - Mathf.Min(reloadProgress * InvReloadTime, 1);
	}

	public void ResetState(){
		// Reset timers
		reloadProgress = 0;
		burstProgress = 0;
		fireProgress = 0;
		recoilMoveProgress = 0;
		recentringProgress = 0;
		flashProgress = 0;

		// Reset recentring
		recenterTargetSet = false;
		totalRecenterRotation = 0;
		initialFiringElevation = 0;
		currentRecenterRotation = 0;

		// Reset state
		isReloading = false;
		isBursting = false;
		isRecoiling = false;
		isOnFireInterval = false;
		isFiring = false;
		bulletsOfBurstFired = 0;
		isFirstShot = true;
		isAds = false;
		semiAutoReady = true;
	}
}
