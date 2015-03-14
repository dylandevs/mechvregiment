using UnityEngine;
using System.Collections;

public class MinigunFirer : MonoBehaviour {
	public GameObject miniGunAimer;
	public GameObject miniGunArm; // for making bullets shoot in the right direction
	public GameObject [] cannonShot;
	public GameObject sparkPrefab;
	public GameObject cannonShotStart;
	public GameObject cannonAimer;
	public GameObject tracerStart;
	public GameObject cameraPlace;
	public GameObject miniGunReticle;
	public GameObject cannonReticle;

	public PoolManager sparks;
	public PoolManager tracerPool;
	public PoolManager flashPool;
	public PoolManager plasmaExplosion;
	public PoolManager plasmaShot;

	public LayerMask mask;
	public LayerMask maskForRet;

	public bool cannonShoot;
	public bool fire;
	public bool overHeated;

	public float range = 10000000.0f;
	public float overHeat;
	public float cannonCDR = 0;

	float coolDown;
	float coolDownWarmUp;
	float warmUpTimer;
	int cannonCounter;
	float cannonCD = 8f;
	float cooldownRemaining = 0;

	bool warmedUp;


	//shooting audio stuff
	public AudioSource cannonSoundEmitter;
	public AudioSource cannonRechargeEmitter;
	public AudioSource minigunWhirEmitter;
	public AudioSource minigunBulletEmitter;

	// Use this for initialization
	void Start () {
		overHeat = 0;
		warmedUp = false;
		//timer until minigun is warmed up
		warmUpTimer = 2f;
		cannonShoot = false;
		fire = false;
		cannonCounter = 1;
		//during warm up
		coolDownWarmUp = 0.5f;
		//after warmed up
		coolDown = 0.2f;
	}
	
	// Update is called once per frame
	void Update () {

		//adjust the reticle 
		//get primary direction
		Ray ray = new Ray(tracerStart.transform.position, tracerStart.transform.forward);
		RaycastHit rayHit;

		Debug.DrawRay(tracerStart.transform.position, tracerStart.transform.forward * 1000,Color.red );

		if(Physics.Raycast (ray, out rayHit,100,mask)){
			Vector3 hitPoint = rayHit.point;
			//fire a ray back at the end of the first ray
			Ray ray2 = new Ray(hitPoint,cameraPlace.transform.position-hitPoint);
			RaycastHit ray2Hit;

			Debug.DrawRay(hitPoint,cameraPlace.transform.position-hitPoint,Color.yellow);

			if(Physics.Raycast (ray2,out ray2Hit,range, maskForRet)){
				//if it hits the aimerwall mvoe the reticle there
				if(ray2Hit.collider.tag == "aimerWall"){
					Vector3 placeHit = ray2Hit.point;
					miniGunReticle.transform.position = placeHit;
					miniGunReticle.transform.forward = cameraPlace.transform.forward;
				}
			}
		}
		
		else if(Physics.Raycast (ray, out rayHit ,10)){
			if(rayHit.collider.tag != "Terrain"){
				Vector3 placeHitRock = rayHit.point;
				Vector3 retPos = placeHitRock.normalized * -3;
				miniGunReticle.transform.position = placeHitRock + retPos;
				miniGunReticle.transform.forward = rayHit.normal;
			}
		}
	

		//this is for when it doesnt hit an object it still displays
		else{
			Vector3 vecEnd = tracerStart.transform.forward * 100;
			Vector3 miniArmPos = tracerStart.transform.position; 
			Vector3 sendBack =  miniArmPos += vecEnd;
			Ray ray2No = new Ray(sendBack,cameraPlace.transform.position-sendBack);

			Debug.DrawRay(sendBack,cameraPlace.transform.position-sendBack,Color.grey);

			RaycastHit ray2HitNo;
			if(Physics.Raycast (ray2No,out ray2HitNo,range, maskForRet)){
				//if it hits the aimerwall mvoe the reticle there
				if(ray2HitNo.collider.tag == "aimerWall"){
					Vector3 placeHit2 = ray2HitNo.point;
					miniGunReticle.transform.position = placeHit2;
					miniGunReticle.transform.forward = cameraPlace.transform.forward;
				}
			}
		}

		//**cooldown stuff**
		//counting down until next shot is fired
		cooldownRemaining -= Time.deltaTime * 5;
		cannonCDR -= Time.deltaTime *5;

		//when not firing reset the warm up timer
		if(fire == false){
			warmUpTimer = 1.5f;
		}
		minigunWhirEmitter.Play();
		//counts down untill switches cooldown rates
		
		//turning the warmUpTimer Update warmUpTimer ona nd off
		if(warmUpTimer <= 0){
			warmedUp = true;
			minigunWhirEmitter.pitch = 2f;
			minigunWhirEmitter.volume = 1f;
		}
		else if(warmUpTimer > 0){
			warmedUp = false;
			minigunWhirEmitter.pitch = 1f + (warmUpTimer/1.5f);
			minigunWhirEmitter.volume = 1f - (warmUpTimer/1.5f);
			warmUpTimer -= Time.deltaTime * 0.75f;
		}
		//when hitting overheated can't fire
		if(overHeat > 150){
			overHeated = true;
		}
		if(overHeat <= 0){
			overHeated = false;
		}
		//over heat recovery
		if(overHeated == true){
			overHeat -= Time.deltaTime * 15; 
		}

		else if(overHeat >=0 && fire == false){
			overHeat -= Time.deltaTime * 4; 
		}

		//fires the minigun based on  if there's ammo firing allowed and no cooldown left
		if (fire == true && cooldownRemaining <= 0 && overHeated == false) {

			overHeat ++;
			//gets the starting aimer angle
			Vector3 tempStart = tracerStart.transform.forward;
			//for spread if we wish to turn it back on
			//ads a randoma mount of spread to the angle
			//Vector3 endShot =  tempStart + new Vector3 (Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F));
			//Ray rayFire = new Ray (tracerStart.transform.position, tempStart);;

			//handles effects of bullet

			GameObject tracer = tracerPool.Retrieve(tracerStart.transform.position);
			tracer.transform.up = tempStart;

			TracerRoundScript tracerRound = tracer.GetComponent<TracerRoundScript>();
			tracerRound.sparkPool = sparks;
			//flash
			GameObject flash = flashPool.Retrieve(tracerStart.transform.position);
			flash.transform.up = tempStart;
				//still needs to be warmed up
				if(warmedUp == true){
					cooldownRemaining = coolDown;
				}
				//been warmed up
				else if(warmedUp == false){
					cooldownRemaining = coolDownWarmUp;
					
				}

			//play bullet noise
			minigunBulletEmitter.pitch = 1f - (Random.value * 0.1);
			minigunBulletEmitter.PlayScheduled(AudioSettings.dspTime);
		}

		Ray rayCannon = new Ray(cannonShotStart.transform.position, cannonShotStart.transform.forward);
		RaycastHit rayHitCannon;
		
		if(Physics.Raycast (rayCannon, out rayHitCannon,100,mask)){
			Vector3 hitPoint = rayHitCannon.point;
			//fire a ray back at the end of the first ray
			Ray ray2Cannon = new Ray(hitPoint,cameraPlace.transform.position-hitPoint);
			RaycastHit ray2HitCannon;
			
			if(Physics.Raycast (ray2Cannon,out ray2HitCannon,range, maskForRet)){
				//if it hits the aimerwall mvoe the reticle there
				if(ray2HitCannon.collider.tag == "aimerWall"){
					Vector3 placeHit = ray2HitCannon.point;
					cannonReticle.transform.position = placeHit;
					cannonReticle.transform.forward = cameraPlace.transform.forward;
				}
			}
		}
		
		else if(Physics.Raycast (rayCannon, out rayHitCannon ,10)){
			if(rayHitCannon.collider.tag != "Terrain"){
				Vector3 placeHitRock = rayHitCannon.point;
				Vector3 retPos = placeHitRock.normalized * -3;
				cannonReticle.transform.position = placeHitRock + retPos;
				cannonReticle.transform.forward = rayHit.normal;
			}
		}
		
		//this is for when it doesnt hit an object it still displays
		else{
			Vector3 vecEnd = cannonShotStart.transform.forward * 100;
			Vector3 miniArmPos = cannonShotStart.transform.position; 
			Vector3 sendBack =  miniArmPos += vecEnd;
			Ray ray2NoCannon = new Ray(sendBack,cameraPlace.transform.position-sendBack);
			
			RaycastHit ray2HitNoCannon;
			if(Physics.Raycast (ray2NoCannon,out ray2HitNoCannon,range, maskForRet)){
				//if it hits the aimerwall mvoe the reticle there
				if(ray2HitNoCannon.collider.tag == "aimerWall"){
					Vector3 placeHit2 = ray2HitNoCannon.point;
					cannonReticle.transform.position = placeHit2;
					cannonReticle.transform.forward = cameraPlace.transform.forward;
				}
			}
		}

		//fires the cannon shot based on the cool down
		if(cannonShoot == true){
			if(cannonCDR <=0){

				Quaternion shotDir = cannonShotStart.transform.rotation;
				GameObject plasmaShotCurr = plasmaShot.Retrieve(cannonShotStart.transform.position,shotDir);

				cannonShot plasmaShotscript = plasmaShotCurr.GetComponent<cannonShot>();
				plasmaShotscript.plasmaExplodePool = plasmaExplosion;


				cannonCDR = cannonCD;

				//play cannon fire audio
				cannonSoundEmitter.PlayScheduled(AudioSettings.dspTime);
				cannonRechargeEmitter.PlayScheduled(AudioSettings.dspTime+0.5f);
			}
		}
	}// end of update//
	
	//overHeat counter GUI
	void OnGUI() {
		GUI.Label (new Rect (15, 40, 200, 20), overHeat.ToString());
	}
}
