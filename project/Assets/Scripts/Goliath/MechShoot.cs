using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//minigun things
	public float range = 100.0f;
	public float damage = 50f;

	//variables for rocketFire
	public float coolDownRocket = 4f;
	float cooldownRemainingRocket = 0;	

	//modes
	public bool rocketMode;
	public bool minionMode;
	public bool miniGunMode;
	public bool keyboard;

	//aiming stuff
	public GameObject miniGunAimer;
	public GameObject missleReticle;
	public GameObject missleTargetArea;
	public GameObject rocketAimer;
	public float rocketAimSpeed;
	public GameObject miniGunReticle;
	public GameObject cameraPlace;
	public GameObject retWall;
	public GameObject lightBeam;
	public GameObject notLightBeam;
	public GameObject miniGunArm;
	public GameObject cannonAimer;
	public GameObject cannonArm;
	public GameObject cannonRet;
	int layerMask = 1 << 22; //for avoiding the ret wall

	//firing objcts
	public MinigunFirer miniGunFirer;
	public GameObject rocketFirer;
	public RocketFirer rocketScript;
	
	//hydra variables
	const int left = 0;
	const int right = 1;

	// Use this for initialization
	void Start () {

		rocketAimSpeed = 15 * Time.deltaTime;
		miniGunMode = true;
		layerMask = ~layerMask;
		keyboard = false;

	}
	
	// Update is called once per frame
	void Update () {
		//All hydra butons and uses
		float lTrig = SixenseInput.Controllers[left].Trigger;
		float rTrig = SixenseInput.Controllers[right].Trigger;

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



		//MODE HANDLING for keyboard************************
		/*
		if(keyboard == true){
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
		}
		*/
		//END OF KEYBOARD CONTROLS

		//cooldowns
		cooldownRemainingRocket -= Time.deltaTime;

		if (miniGunMode == true) {
			//aiming the minigun and placing the reticle in the right place
			miniGunReticle.SetActive(true);
			cannonRet.SetActive(true);
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
			Ray ray = new Ray(miniGunAimer.transform.position,miniGunAimer.transform.forward);
			RaycastHit hitInfoAimer;
			//check if it hit something then send a ray back to display the reticle
			if(Physics.Raycast (ray, out hitInfoAimer,range,layerMask)){

				Vector3 hitPoint = hitInfoAimer.point;
				//create a hit variable for the second raycast
				Ray ray2 = new Ray(hitPoint,cameraPlace.transform.position-hitPoint);
				RaycastHit ray2Hit;
				//make it look like the minigun arm is facing where its shooting
				Vector3 vecEnd = miniGunAimer.transform.forward * 100;
				// limit the angles properly
				miniGunArm.transform.right = -vecEnd;
				if(Physics.Raycast (ray2,out ray2Hit,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(ray2Hit.collider.tag == "aimerWall"){
						Vector3 placeHit = ray2Hit.point;
						miniGunReticle.transform.position = placeHit;
						miniGunReticle.transform.forward = cameraPlace.transform.forward;
					}
				}
			}

			if(Physics.Raycast (ray, out hitInfoAimer ,20,layerMask)){
				if(hitInfoAimer.collider.tag != "Terrain"){
					Vector3 placeHitRock = hitInfoAimer.point;
					Vector3 retPos = placeHitRock.normalized * -3;
					miniGunReticle.transform.position = placeHitRock + retPos;
					miniGunReticle.transform.forward = hitInfoAimer.normal;
				}
			}

			//this is for when it doesnt hit an object it still displays
			else{
				Vector3 vecEnd = miniGunAimer.transform.forward * 100;
				Vector3 miniArmPos = miniGunAimer.transform.position; 
				Vector3 sendBack =  miniArmPos += vecEnd;
			// limit these angles properly
				miniGunArm.transform.right = -vecEnd;

				Ray ray2No = new Ray(sendBack,cameraPlace.transform.position-sendBack);
				RaycastHit ray2HitNo;
				if(Physics.Raycast (ray2No,out ray2HitNo,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(ray2HitNo.collider.tag == "aimerWall"){
						Vector3 placeHit2 = ray2HitNo.point;
						miniGunReticle.transform.position = placeHit2;
						miniGunReticle.transform.forward = cameraPlace.transform.forward;
					}
				}
			}



			//CANNON AIMING
			Ray rayCannon = new Ray(cannonAimer.transform.position,cannonAimer.transform.forward);
			RaycastHit hitInfoAimerCannon;
			//check if it hit something then send a ray back to display the reticle
			if(Physics.Raycast (rayCannon, out hitInfoAimerCannon,range,layerMask)){
				
				Vector3 hitPointCannon = hitInfoAimerCannon.point;
				//create a hit variable for the second raycast
				Ray ray2Cannon = new Ray(hitPointCannon,cameraPlace.transform.position-hitPointCannon);
				RaycastHit ray2HitCannon;
				//make it look like the minigun arm is facing where its shooting
				Vector3 vecEndCannon = cannonAimer.transform.forward * 100;
				cannonArm.transform.right = -vecEndCannon;
				if(Physics.Raycast (ray2Cannon,out ray2HitCannon,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(ray2HitCannon.collider.tag == "aimerWall"){
						Vector3 placeHitCannon = ray2HitCannon.point;
						cannonRet.transform.position = placeHitCannon;
						cannonRet.transform.forward = cameraPlace.transform.forward;
					}
				}
			}

			if(Physics.Raycast (rayCannon, out hitInfoAimerCannon ,20,layerMask)){
				if(hitInfoAimerCannon.collider.tag != "Terrain"){
					Vector3 placeHitRockC = hitInfoAimerCannon.point;
					Vector3 retPosC = placeHitRockC.normalized * -3;
					cannonRet.transform.position = placeHitRockC + retPosC;
					cannonRet.transform.forward = hitInfoAimerCannon.normal;
				}
			}
				
			//this is for when it doesnt hit an object it still displays
			else{
				Vector3 endOfVector = cannonAimer.transform.forward * 100;
				Vector3 CannonArmPosition = cannonAimer.transform.position; 
				Vector3 secondPart =  CannonArmPosition += endOfVector;
				// limit these angles properly
				cannonArm.transform.right = -secondPart;
				
				
				Ray cannonsSecondRay = new Ray(secondPart,cameraPlace.transform.position-secondPart);
				RaycastHit connonsSecondRayHit;

				if(Physics.Raycast (cannonsSecondRay,out connonsSecondRayHit,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(connonsSecondRayHit.collider.tag == "aimerWall"){
						Vector3 hittyThingy = connonsSecondRayHit.point;
						cannonRet.transform.position = hittyThingy;
						cannonRet.transform.forward = cameraPlace.transform.forward;
					}
				}
			}
			
			if(lTrig > 0.8f){
				miniGunFirer.cannonShoot = true;
			}
			if(rTrig > 0.8f){
				miniGunFirer.fire = true;
			}
			if(lTrig < 0.7f){
				miniGunFirer.cannonShoot = false;
			}
			if(rTrig < 0.7f){
				miniGunFirer.fire = false;
			}

			//set the reticle based on a raycast


			/*
				if(Input.GetKeyDown("space")){
					miniGunFirer.fire = true;
				}
				
				if(Input.GetKeyDown(KeyCode.LeftControl)){
					miniGunFirer.cannonShoot = true;
				}
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
				
			//turn on the aiming device
			missleReticle.SetActive(true);
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
			//makes the ray
			if(cooldownRemainingRocket <=0){
				Ray rayRockMode = new Ray(rocketAimer.transform.position,rocketAimer.transform.forward);
				RaycastHit rockModeRayHit;
				//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
				if(Physics.Raycast (rayRockMode, out rockModeRayHit,range,layerMask)){
					if(rockModeRayHit.collider.tag == "Terrain"){
						Vector3 placeHitRock = rockModeRayHit.point;
						missleTargetArea.transform.position = placeHitRock;
						missleTargetArea.transform.LookAt(rockModeRayHit.normal + -placeHitRock);
					}
					else{
							//have to fix the missle decal not turning over!!
						Vector3 placeHitRock = rockModeRayHit.point;
						missleTargetArea.transform.position = placeHitRock;
						missleTargetArea.transform.LookAt(-rockModeRayHit.normal + -placeHitRock);
					}
				}
			}
			if(rTrig > 0.8f && cooldownRemainingRocket <= 0){
				cooldownRemainingRocket = coolDownRocket;
				rocketScript.firing = true;
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
			//makes the ray
			Ray rayRockMode = new Ray(cannonAimer.transform.position,cannonAimer.transform.forward);
			RaycastHit rockModeRayHit;
			//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
			if(Physics.Raycast (rayRockMode, out rockModeRayHit,range,layerMask)){
				if(rockModeRayHit.collider.tag == "Terrain"){
					lightBeam.SetActive(true);
					notLightBeam.SetActive(false);
					Vector3 placeHitRock = rockModeRayHit.point;
					lightBeam.transform.position = placeHitRock;

					//fire the rocket function in rocket arm script
					if (Input.GetKeyDown ("space")) {
						//do something with minions
					}
				}
			}

			 if(Physics.Raycast (rayRockMode, out rockModeRayHit,range,layerMask)){
				 if(rockModeRayHit.collider.tag != "Terrain"){
						lightBeam.SetActive(false);
						notLightBeam.SetActive(true);
					Vector3 placeHitRock = rockModeRayHit.point;
					notLightBeam.transform.position = placeHitRock;
				}
			}
	}//this is end of update
}
	//function to reset the modes
	void resetModes(){
		miniGunMode = false;
		rocketMode = false;
		minionMode = false;
		//turn off the aimers when not in the mode
		cannonRet.SetActive(false);
		missleReticle.SetActive(false);
		miniGunReticle.SetActive (false);
		lightBeam.SetActive(false);
		notLightBeam.SetActive (false);
	}
}
