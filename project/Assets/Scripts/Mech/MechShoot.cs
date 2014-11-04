using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//minigun things
	public float range = 100.0f;
	public float damage = 50f;
	float rotSpeed;

	//variables for rocketFire
	public float coolDownRocket = 2f;
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
				miniGunArm.transform.Rotate(miniGunArm.transform.right * rotSpeed, Space.World);
			}
			if (Input.GetKey ("j")) {
				miniGunArm.transform.Rotate(-miniGunArm.transform.right * rotSpeed, Space.World);
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

			if(Physics.Raycast (ray, out hitInfoAimer,range)){
				Vector3 hitPoint = hitInfoAimer.point;
				//create a hit variable for the second raycast
				RaycastHit ray2Hit;
				//fire second raycast
				Ray ray2 = new Ray(hitPoint,retWall.transform.position);
				if(Physics.Raycast (ray2,out ray2Hit,range)){
					//if it hits the aimerwall mvoe the reticle there
					if(ray2Hit.collider.tag == "aimerWall"){
						Vector3 placeHit = ray2Hit.point;
						miniGunReticle.transform.position = placeHit;
					}
				}
				//Vector3 whereToDraw = cameraPlace.transform.position - hitPoint;
				//Vector3 timesOne = whereToDraw;
				//whereToDraw /= 2;
				//whereToDraw += hitPoint;

				//***********must fix reticle not showing up ************
					//miniGunReticle.transform.position = whereToDraw;
					//miniGunReticle.transform.rotation = Quaternion.LookRotation(timesOne);



				if(Input.GetKeyDown("space")){
					miniGunFirer.fire = true;
				}

				if(Input.GetKeyUp("space")){
					miniGunFirer.fire = false;
				}
			}
		}//*******end of minigun aiming and fire***************

		//the rocket mode is on
		if (rocketMode == true) {
			//turn on the aiming device
			missleReticle.SetActive(true);

			//aim the rockets position on the map
			if (Input.GetKey ("u")) {
				missleTargetArea.transform.Translate(missleTargetArea.transform.forward * rocketAimSpeed);
			}
			if (Input.GetKey ("j")) {
				missleTargetArea.transform.Translate(missleTargetArea.transform.forward * rocketAimSpeed * -1);
			}
			if (Input.GetKey ("k")) {
				missleTargetArea.transform.Translate(missleTargetArea.transform.right * rocketAimSpeed);
			}
			if (Input.GetKey ("h")) {
				missleTargetArea.transform.Translate(missleTargetArea.transform.right * rocketAimSpeed * -1);
			}

			//fire the rocket function in rocket arm script
			if (Input.GetKeyDown ("space") && cooldownRemainingRocket <= 0) {
					cooldownRemainingRocket = coolDownRocket;
					rocketScript.firing = true;
			}
		}

		//minion mode has been entered now time to aim
		if (minionMode == true) {

			//turn on the aiming device
			missleReticle.SetActive(true);
			
			//aim the rockets position on the map
			if (Input.GetKey ("u")) {
				missleTargetArea.transform.Translate(transform.forward * rocketAimSpeed);
			}
			if (Input.GetKey ("j")) {
				missleTargetArea.transform.Translate(transform.forward * rocketAimSpeed * -1);
			}
			if (Input.GetKey ("k")) {
				missleTargetArea.transform.Translate(transform.right * rocketAimSpeed);
			}
			if (Input.GetKey ("h")) {
				missleTargetArea.transform.Translate(transform.right * rocketAimSpeed * -1);
			}
			
			//fire the rocket function in rocket arm script
			if (Input.GetKeyDown ("space")) {
				//do something with minions
			}
		}

	}//this is end of update
	
	//function to reset the modes
	void resetModes(){
		miniGunMode = false;
		rocketMode = false;
		minionMode = false;
		//turn off the aimers when not in the mode
		missleReticle.SetActive(false);
		miniGunReticle.SetActive (false);
	}

}
