using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//minigun things
	public float range = 100.0f;
	public float damage = 50f;
	float rotSpeed;

	//variables for rocketFire
	public float coolDownRocket = 4f;
	float cooldownRemainingRocket = 0;	

	//modes
	public bool rocketMode;
	public bool minionMode;
	public bool miniGunMode;

	//aiming stuff
	public GameObject miniGunArm;
	public GameObject missleReticle;
	public GameObject missleTargetArea;
	public float rocketAimSpeed;
	public GameObject miniGunReticle;
	public GameObject cameraPlace;
	public GameObject retWall;
	public GameObject lightBeam;
	public GameObject notLightBeam;
	int layerMask = 1 << 16; //for avoiding the ret wall

	//firing objcts
	public MinigunFirer miniGunFirer;
	public GameObject rocketFirer;
	private RocketFirer rocketScript;

	// Use this for initialization
	void Start () {
		rocketAimSpeed = 15 * Time.deltaTime;
		miniGunMode = true;
		rotSpeed = Time.deltaTime * 50;
		rocketScript = rocketFirer.GetComponent<RocketFirer>();
		miniGunFirer = miniGunFirer.GetComponent<MinigunFirer>();
		layerMask = ~layerMask;
	}
	
	// Update is called once per frame
	void Update () {
		//MODE HANDLING************************
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

		//cooldowns
		cooldownRemainingRocket -= Time.deltaTime;

		//the minigun mode
		if (miniGunMode == true) {
			
			//aiming the minigun and placing the reticle in the right place
			miniGunReticle.SetActive(true);

			//aim the position of where the minigun is going to fire from
			if (Input.GetKey ("u")) {
				miniGunArm.transform.Rotate(-miniGunArm.transform.right * rotSpeed, Space.World);
			}
			if (Input.GetKey ("j")) {
				miniGunArm.transform.Rotate(miniGunArm.transform.right * rotSpeed, Space.World);
			}
			if (Input.GetKey ("k")) {
				miniGunArm.transform.Rotate(Vector3.up*rotSpeed,Space.World);
			}
			if (Input.GetKey ("h")) {
				miniGunArm.transform.Rotate(-Vector3.up*rotSpeed,Space.World);	
			}
			//set the reticle based on a raycast
			Ray ray = new Ray(miniGunArm.transform.position,miniGunArm.transform.forward);
			RaycastHit hitInfoAimer;

			if(Physics.Raycast (ray, out hitInfoAimer,range,layerMask)){
					Vector3 hitPoint = hitInfoAimer.point;
					//create a hit variable for the second raycast
					Ray ray2 = new Ray(hitPoint,cameraPlace.transform.position-hitPoint);
					RaycastHit ray2Hit;

					if(Physics.Raycast (ray2,out ray2Hit,range)){
						//if it hits the aimerwall mvoe the reticle there
						if(ray2Hit.collider.tag == "aimerWall"){
							Vector3 placeHit = ray2Hit.point;
							miniGunReticle.transform.position = placeHit;
						}
					}
			}
			else{
				Vector3 vecEnd = miniGunArm.transform.forward * 100;
				Vector3 miniArmPos = miniGunArm.transform.position; 
				Vector3 sendBack =  miniArmPos += vecEnd;
				Ray ray2No = new Ray(sendBack,cameraPlace.transform.position-sendBack);
				RaycastHit ray2HitNo;
				if(Physics.Raycast (ray2No,out ray2HitNo,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(ray2HitNo.collider.tag == "aimerWall"){
						Vector3 placeHit2 = ray2HitNo.point;
						miniGunReticle.transform.position = placeHit2;
					}
				}
			}

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
					miniGunFirer.cannonShoot = false;
				}
			}//*******end of minigun aiming and fire***************

		//the rocket mode is on
		if (rocketMode == true) {

			//makes the ray
			Ray rayRockMode = new Ray(cameraPlace.transform.position,cameraPlace.transform.forward);
			RaycastHit rockModeRayHit;
			//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
			if(Physics.Raycast (rayRockMode, out rockModeRayHit,range,layerMask)){
				if(rockModeRayHit.collider.tag == "Terrain"){
					Vector3 placeHitRock = rockModeRayHit.point;
					missleTargetArea.transform.position = placeHitRock;
					missleTargetArea.transform.rotation = Quaternion.Euler(rockModeRayHit.normal);
				}
				else{
					Vector3 placeHitRock = rockModeRayHit.point;
					missleTargetArea.transform.position = placeHitRock;
					missleTargetArea.transform.rotation = Quaternion.Euler(rockModeRayHit.normal);
				}
			}

			//turn on the aiming device
			missleReticle.SetActive(true);

			//fire the rocket function in rocket arm script
			if (Input.GetKeyDown ("space") && cooldownRemainingRocket <= 0) {
					cooldownRemainingRocket = coolDownRocket;
					rocketScript.firing = true;
			}
		}

		//minion mode has been entered now time to aim
		if (minionMode == true) {
			//makes the ray
			Ray rayRockMode = new Ray(cameraPlace.transform.position,cameraPlace.transform.forward);
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
		missleReticle.SetActive(false);
		miniGunReticle.SetActive (false);
		lightBeam.SetActive(false);
		notLightBeam.SetActive (false);
	}

}
