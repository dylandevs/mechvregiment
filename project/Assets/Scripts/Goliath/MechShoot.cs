using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//minigun things
	public float range = 100000000000.0f;
	public float damage = 50f;

	//variables for rocketFire
	public float coolDownRocket = 20f;
	public float cooldownRemainingRocket = 0;	
	public float rocketAimSpeed;

	//modes
	public bool rocketMode = false;
	public bool minionMode = false;
	public bool miniGunMode = true;
	public bool carrying = false;

	//particle emitters for flag carry
	public GameObject leftEmitter;
	public GameObject rightEmitter;

	//flag stuff
	public GameObject flag;
	public GameObject flagCarried;

	//aiming stuff
	public GameObject miniGunAimer;
	public GameObject missleReticle;
	public GameObject missleTargetArea;
	public GameObject rocketAimer;
	public GameObject miniGunReticle;
	public GameObject cameraPlace;
	public GameObject retWall;
	public GameObject lightBeam;
	public GameObject notLightBeam;
	public GameObject miniGunArm;
	public GameObject cannonAimer;
	public GameObject cannonArm;
	public GameObject cannonRet;
	public GameObject cannonEffect;
	public GameObject cannonEffect2;
	public GameObject cannonEffectParent;
	public GameObject cannonEffect2Parent;
	public GameObject rangeIndicator;
	public GameObject outOfRange;
	public GameObject minionFlag;
	public GameObject cannonShotStart;
	//masks
	public LayerMask mask;
	public LayerMask maskForRet;
	//make it ignore players
	public LayerMask maskRocket;

	public GameObject hydraLeft;
	public GameObject hydraRight;

	//firing objcts
	public MinigunFirer miniGunFirer;
	public RocketFirer rocketScript;
	
	//hydra variables
	const int left = 0;
	const int right = 1;

	//pilot movement stuff
	public GameObject leftArm;
	public GameObject rightArm;

	public Animator pilotAnimator;

	//shooting audio stuff
	public AudioSource cannonSoundEmitter;
	public AudioSource aiDirectorEmitter;

	public GoliathNetworking networkManager;

	public bool forceKeyboard = false;
	public bool allowedToShoot;

	float shootTimer;
	float missleRetTimer;

	bool inRangeMiniX;
	bool inRangeCannonX;
	bool inRangeMiniY;
	bool inRangeCannonY;
	bool ableToShoot;
	bool ableToShootM;
	bool flagDown;

	int miniIdle = Animator.StringToHash("miniGunIdle");
	int miniFire = Animator.StringToHash("miniGunFire");

	int missleIdle = Animator.StringToHash("missleIdle");
	int missleFire = Animator.StringToHash("missleFire");

	int cannonIdle = Animator.StringToHash("cannonIdle");
	int cannonFire = Animator.StringToHash("cannonFire");

	int minionIdle = Animator.StringToHash("minionIdle");
	int minionPlace = Animator.StringToHash("minionSet");



	// Use this for initialization
	void Start () {
		flagCarried.SetActive(false);
		rocketAimSpeed = 15 * Time.deltaTime;
		miniGunMode = true;
		carrying = false;
	}
	
	// Update is called once per frame
	void Update () {
		//update aimer pos.
		updateAimerPos();
		//show missile landing zone
		if(missleRetTimer > 0){
			missleReticle.SetActive(true);
			missleRetTimer -= Time.deltaTime;

			if(missleRetTimer <= 0){
				missleReticle.SetActive(false);
			}

		}

		float lTrig = 0;
		float rTrig = 0;

		if (SixenseInput.Controllers[left] != null && !forceKeyboard){
			//All hydra butons and uses
			lTrig = SixenseInput.Controllers[left].Trigger;
			rTrig = SixenseInput.Controllers[right].Trigger;

			//hydra mode handling
			if(SixenseInput.Controllers[right].GetButtonDown(SixenseButtons.ONE)){
				resetModes();
				rocketMode = true;
			}
			if(SixenseInput.Controllers[left].GetButtonDown(SixenseButtons.ONE)){
				resetModes();
				minionMode = true;
			}
			if(SixenseInput.Controllers[right].GetButtonUp(SixenseButtons.ONE)){
				resetModes();
				miniGunMode = true;
			}
			if(SixenseInput.Controllers[left].GetButtonUp(SixenseButtons.ONE)){
				resetModes();
				miniGunMode = true;
			}
		}
		else {

		//MODE HANDLING for keyboard************************

			if (Input.GetKeyDown ("1")) {
				resetModes();
				rocketMode = true;
			}
			if (Input.GetKeyDown ("2")) {
				resetModes();
				minionMode = true;
			}
			if (Input.GetKeyUp ("1")) {
				resetModes();
				miniGunMode = true;
			}
			if (Input.GetKeyUp ("2")) {
				resetModes();
				miniGunMode = true;
			}

			if (Input.GetKey(KeyCode.Mouse0)){
				lTrig = 1;
			}
			if (Input.GetKey(KeyCode.Mouse1)){
				rTrig = 1;
			}

		//END OF KEYBOARD CONTROLS
		}
		if(allowedToShoot == true){
		//cooldowns
		cooldownRemainingRocket -= Time.deltaTime;
		if (miniGunMode == true) {

			pilotAnimator.SetBool(minionIdle,false);
			pilotAnimator.SetBool(missleFire,false);
			pilotAnimator.SetBool(missleIdle,false);

			//aiming the minigun and placing the reticle in the right place
			if(inRangeMiniX == true && inRangeMiniY == true){
				ableToShootM = true;
				miniGunReticle.SetActive(true);
			}
			else if(inRangeMiniX == false || inRangeCannonY){
				ableToShootM = false;
				miniGunReticle.SetActive(false);
			}

			if(inRangeCannonX == true && inRangeCannonY == true){
				ableToShoot = true;
				cannonRet.SetActive(true);
				cannonEffect.SetActive(true);
				cannonEffect2.SetActive(true);
			}
			else if(inRangeCannonX == false || inRangeCannonY == false){
				ableToShoot = false;
				cannonRet.SetActive(false);
				cannonEffect.SetActive(false);
				cannonEffect2.SetActive(false);
			}


			//spin the reticle
			cannonEffectParent.transform.Rotate(cannonEffect.transform.right * Time.deltaTime * 10,Space.World);
			cannonEffect2Parent.transform.Rotate(cannonEffect2.transform.right * Time.deltaTime * 10,Space.World);
			
			//********needs adjusting after model import*****************************************************
			//aim the position of where the minigun is going to fire from
			/*
			if(keyboard ==true){
				//is disabled on start due to use of the hydra due to hydra input overding the aiming....
				if (miniGunAimer.transform.localEulerAngles.x <= 30||miniGunAimer.transform.localEulerAngles.x >= 335) {
					if (Input.GetKey ("u")) {
						miniGunAimer.transform.Rotate(-miniGunAimer.transform.right * rotSpeed, Space.World);
					}
				}
				if (miniGunAimer.transform.localEulerAngles.x >= 330||miniGunAimer.transform.localEulerAngles.x <= 21) {
					if (Input.GetKey ("j")) {
						miniGunAimer.transform.Rotate(miniGunAimer.transform.right * rotSpeed, Space.World);
					}
				}
				if (miniGunAimer.transform.localEulerAngles.y >= 280||miniGunAimer.transform.localEulerAngles.y <= 50) {
					if (Input.GetKey ("k")) {
						miniGunAimer.transform.Rotate(Vector3.up*rotSpeed,Space.World);
					}
				}
				if (miniGunAimer.transform.localEulerAngles.y >= 300||miniGunAimer.transform.localEulerAngles.y <= 60) {
					if (Input.GetKey ("h")) {

						miniGunAimer.transform.Rotate(-Vector3.up*rotSpeed,Space.World);	
					}
				}
			}
			*/
			//MINIGUN AIMING
			//set the reticle based on a raycast
			Vector3 adjustedRotM = miniGunAimer.transform.localEulerAngles + new Vector3(-90,0,0);
			Vector3 adjustedRotV = cannonAimer.transform.localEulerAngles + new Vector3(-90,0,0);

			miniGunArm.transform.localEulerAngles = adjustedRotM;
			cannonArm.transform.localEulerAngles = adjustedRotV;

	
			
			if(lTrig > 0.8f && ableToShoot == true){
				shootTimer = 1f;
				miniGunFirer.cannonShoot = true;
				pilotAnimator.SetBool(cannonIdle,false);
				pilotAnimator.SetBool(cannonFire,true);
			}
			if(rTrig > 0.8f && ableToShootM == true){
				pilotAnimator.SetBool(miniIdle,false);
				pilotAnimator.SetBool(miniFire,true);

				miniGunFirer.fire = true;
			}
			if(lTrig < 0.8f){
				if(shootTimer >=0){
					shootTimer-= Time.deltaTime;
				}
				if(shootTimer <=0){
					pilotAnimator.SetBool(cannonFire,false);
					pilotAnimator.SetBool(cannonIdle,true);
				}
				miniGunFirer.cannonShoot = false;
			}
			if(rTrig < 0.8f){
				pilotAnimator.SetBool(miniFire,false);
				pilotAnimator.SetBool(miniIdle,true);
				miniGunFirer.fire = false;
			}

			//set the reticle based on a raycast


			/*
				if(Input.GetKeyDown("space")){
					miniGunFirer.fire = true;
				}
				*/
				/*if(Input.GetKeyDown("g")){
					shootTimer = 1f;
					miniGunFirer.cannonShoot = true;
					pilotAnimator.SetBool(cannonIdle,false);
					pilotAnimator.SetBool(cannonFire,true);
					//play cannon fire audio
					cannonSoundEmitter.PlayScheduled(AudioSettings.dspTime);
				}*/

			/*
				if(Input.GetKeyUp(KeyCode.LeftControl)){
					miniGunFirer.cannonShoot = false;
				}
				if(Input.GetKeyUp("space")){
					miniGunFirer.fire = false;
				}
			 */
			}//*******end of minigun aiming and fire***************


		//the rocket mode is on
		if (rocketMode == true) {
			//set animations	
			pilotAnimator.SetBool(miniFire,false);

			//turn on the aiming device
			missleReticle.SetActive(true);
			rangeIndicator.SetActive(true);
			//********needs adjusting after model import*****************************************************
			/*
			if(keyboard == true){
				if (rocketAimer.transform.eulerAngles.x <= 30||rocketAimer.transform.eulerAngles.x >= 335) {
					if (Input.GetKey ("u")) {
						rocketAimer.transform.Rotate(-rocketAimer.transform.right * rotSpeed, Space.World);
					}
				}
				if (rocketAimer.transform.eulerAngles.x >= 330||rocketAimer.transform.eulerAngles.x <= 21) {
					if (Input.GetKey ("j")) {
						rocketAimer.transform.Rotate(rocketAimer.transform.right * rotSpeed, Space.World);
					}
				}
				if (rocketAimer.transform.eulerAngles.y >= 280||rocketAimer.transform.eulerAngles.y <= 50) {
					print(rocketAimer.transform.eulerAngles.y);
					if (Input.GetKey ("k")) {
						rocketAimer.transform.Rotate(Vector3.up*rotSpeed,Space.World);
					}
				}
				if (rocketAimer.transform.eulerAngles.y >= 300||rocketAimer.transform.eulerAngles.y <= 60) {
					if (Input.GetKey ("h")) {
						
						rocketAimer.transform.Rotate(-Vector3.up*rotSpeed,Space.World);	
					}
				}
			}
		 	*/

			//******* change it so that is there is no target says no target *******

			//updates arm pos
			Vector3 adjustedRotM = miniGunAimer.transform.localEulerAngles + new Vector3(-90,0,0);
			miniGunArm.transform.localEulerAngles = adjustedRotM;

			//makes the ray
			if(cooldownRemainingRocket <=0){	
				Ray rayRockMode = new Ray(miniGunAimer.transform.position,miniGunAimer.transform.forward);
				RaycastHit rockModeRayHit;
				//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
				if(Physics.Raycast (rayRockMode, out rockModeRayHit,100,maskRocket)){
					if(rockModeRayHit.collider.tag == "Terrain"){

						outOfRange.SetActive(false);

						Vector3 placeHitRock = rockModeRayHit.point;
						missleTargetArea.transform.position = placeHitRock + new Vector3(0,0.5f,0);
						missleTargetArea.transform.LookAt(rockModeRayHit.normal + -placeHitRock);
						
							if(rTrig > 0.8f && cooldownRemainingRocket <= 0){
								pilotAnimator.SetBool(missleIdle,false);
								pilotAnimator.SetBool(missleFire,true);
								missleRetTimer = 5.5f;
								cooldownRemainingRocket = coolDownRocket;
								rocketScript.rocketDelayTimer = RocketFirer.RocketDelay;
							}
							if(rTrig < 0.1f){
								pilotAnimator.SetBool(missleIdle,true);
								pilotAnimator.SetBool(missleFire,false);
							}
						}

				}else {
					outOfRange.SetActive(true);
					}
			}
			


			//fire the rocket function in rocket arm script
/*
			if (Input.GetKeyDown("space") && cooldownRemainingRocket <= 0) {
					cooldownRemainingRocket = coolDownRocket;
					rocketScript.firing = true;
*/
			}

		//minion mode has been entered now time to aim
		if (minionMode == true) {
			//update arm pos
			updateAimerPos();
			pilotAnimator.SetBool(cannonIdle,false);
			pilotAnimator.SetBool(minionIdle,true);

			Vector3 adjustedRotV = cannonAimer.transform.localEulerAngles + new Vector3(-90,0,0);
			cannonArm.transform.localEulerAngles = adjustedRotV;

			//makes the ray
			Ray minMode = new Ray(cannonShotStart.transform.position,cannonShotStart.transform.forward);

			Debug.DrawRay(cannonShotStart.transform.position,cannonShotStart.transform.forward * 1000,Color.red);

			RaycastHit minHit;


			//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
			if(Physics.Raycast (minMode, out minHit,75,mask)){
				if(minHit.collider.tag == "Terrain"){
					lightBeam.SetActive(true);
					notLightBeam.SetActive(false);
					Vector3 placeHitRock = minHit.point;
					lightBeam.transform.position = placeHitRock;

					if(lTrig > 0.9f){
						pilotAnimator.SetBool(minionPlace,true);
						pilotAnimator.SetBool(minionIdle,false);
						
						flagDown = true;
					}

					if(lTrig < 0.2f && flagDown == true){
						
						minionFlag.transform.position = placeHitRock;
						minionFlag.SetActive(true);
						
						pilotAnimator.SetBool(minionPlace,false);
						pilotAnimator.SetBool(minionIdle,true);

						flagDown = false;
						networkManager.photonView.RPC("PlaceMinionWaypoint",PhotonTargets.All,minionFlag.transform.position);

					}



					//fire the rocket function in rocket arm script
					//if (Input.GetKeyDown ("space")) {
						//do something with minions
					//}

					//plays the ai direction sound
					aiDirectorEmitter.PlayScheduled(AudioSettings.dspTime);
				}
			}

			if(Physics.Raycast (minMode, out minHit,range,mask)){
				if(minHit.collider.tag != "Terrain"){
					lightBeam.SetActive(false);
					notLightBeam.SetActive(true);
					Vector3 placeHitRock = minHit.point;
					notLightBeam.transform.position = placeHitRock;
				}
			}

		}
	}

		if(carrying == true){
			//turns off all other modes
			resetModes();
			//turns off world flag and replaces it with carried version
			flag.SetActive(false);
			flagCarried.SetActive(true);
			
			leftEmitter.SetActive(true);
			rightEmitter.SetActive(true);
			//play animation for mech carrying flag thingy; 
			
			//drops the flag
			if(SixenseInput.Controllers[left].GetButtonDown(SixenseButtons.BUMPER) ){
				carrying = false;
				releaseFlag();
				miniGunMode = true;
				
				leftEmitter.SetActive(false);
				rightEmitter.SetActive(false);
			}
		}
	}//this is end of update
	//function to reset the modes
	void resetModes(){
		miniGunMode = false;
		rocketMode = false;
		minionMode = false;
		//turn off the aimers when not in the mode
		cannonRet.SetActive(false);
		rangeIndicator.SetActive(false);
		outOfRange.SetActive(false);
		missleReticle.SetActive(false);
		miniGunReticle.SetActive (false);
		lightBeam.SetActive(false);
		notLightBeam.SetActive (false);
		cannonEffect.SetActive(false);
		cannonEffect2.SetActive(false);
	}

	public void releaseFlag(){
		//moves the realflag to the fake flags position and adds a force to it so it flies away
		flag.transform.position = flagCarried.transform.position + new Vector3 (0,-5,0);
		networkManager.MechDroppedFlag(flag.transform.position);
		flag.SetActive(true);
		flagCarried.SetActive(false);
	}

	public void updateAimerPos(){
		//add limitations to aimers and unparent it
		if(hydraRight.transform.localEulerAngles.y > 320 || hydraRight.transform.localEulerAngles.y < 40){
			inRangeMiniY = true;
			if(hydraRight.transform.localEulerAngles.x > 355 || hydraRight.transform.localEulerAngles.x < 45){
				inRangeMiniX = true;
				miniGunAimer.transform.localEulerAngles = new Vector3 (hydraRight.transform.localEulerAngles.x,hydraRight.transform.localEulerAngles.y,0);
				miniGunAimer.transform.position = hydraRight.transform.position;
			}else inRangeMiniY = false;
		}else inRangeMiniX = false;


		if(hydraRight.transform.localEulerAngles.y > 320 || hydraRight.transform.localEulerAngles.y < 50){
			if(hydraRight.transform.localEulerAngles.x > 350 || hydraRight.transform.localEulerAngles.x < 50){
				rocketAimer.transform.localEulerAngles = new Vector3 (hydraRight.transform.localEulerAngles.x,hydraRight.transform.localEulerAngles.y,0);
				rocketAimer.transform.position = hydraRight.transform.position;
			}
		}

		if(hydraLeft.transform.localEulerAngles.y > 320 || hydraLeft.transform.localEulerAngles.y < 40){
			inRangeCannonY = true;
			if(hydraLeft.transform.localEulerAngles.x > 355 || hydraLeft.transform.localEulerAngles.x < 45){
				inRangeCannonX = true;
				cannonAimer.transform.localEulerAngles = new Vector3 (hydraLeft.transform.localEulerAngles.x,hydraLeft.transform.localEulerAngles.y,0);
				cannonAimer.transform.position = hydraLeft.transform.position;
			}else inRangeCannonY = false;
		}else inRangeCannonX = false;

		//the change in the pilot arm to match aiming

		Vector3 adjustedRotP = new Vector3(miniGunAimer.transform.localEulerAngles.x, -miniGunAimer.transform.localEulerAngles.y, -miniGunAimer.transform.localEulerAngles.z) + new Vector3(0,50,20);
		rightArm.transform.localEulerAngles = adjustedRotP;

		Vector3 adjustedRotPL = cannonAimer.transform.localEulerAngles + new Vector3(0,50,20);
		leftArm.transform.localEulerAngles = adjustedRotPL;

		//print("pilot" + rightArm.transform.localEulerAngles);
		//print("miniGun" + miniGunArm.transform.localEulerAngles);
	}//end of update aimer pos
}// end of class
