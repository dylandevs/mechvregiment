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

	public UIEffectHandling effects;
	public PoolManager tracerPool;

	public LayerMask mask;
	public LayerMask maskForRet;

	public bool cannonShoot;
	public bool fire;

	public float range = 10000000.0f;
	public float overHeat;

	float coolDown;
	float coolDownWarmUp;
	float warmUpTimer;
	int cannonCounter;
	float cannonCD = 8f;
	float cooldownRemaining = 0;
	float cannonCDR = 0;
	bool warmedUp;
	bool overHeated;

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
		coolDownWarmUp = 1f;
		//after warmed up
		coolDown = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
		//adjust the reticle 

		//get primary direction
		Ray ray = new Ray(tracerStart.transform.position, tracerStart.transform.forward);
		RaycastHit rayHit;

		Debug.DrawRay(tracerStart.transform.position,tracerStart.transform.forward * 1000, Color.red);

		if(Physics.Raycast (ray, out rayHit,range,mask)){
			Vector3 hitPoint = rayHit.point;
			//fire a ray back at the end of the first ray
			Ray ray2 = new Ray(hitPoint,cameraPlace.transform.position-hitPoint);
			RaycastHit ray2Hit;

			print ("i hit something");

			Debug.DrawRay(hitPoint,cameraPlace.transform.position-hitPoint, Color.green);

			if(Physics.Raycast (ray2,out ray2Hit,range, maskForRet)){
				//if it hits the aimerwall mvoe the reticle there
				if(ray2Hit.collider.tag == "aimerWall"){
					Vector3 placeHit = ray2Hit.point;
					miniGunReticle.transform.position = placeHit;
					miniGunReticle.transform.forward = cameraPlace.transform.forward;
				}
			}
		}
		
		else if(Physics.Raycast (ray, out rayHit ,20)){
			if(rayHit.collider.tag != "Terrain"){
				Vector3 placeHitRock = rayHit.point;
				Vector3 retPos = placeHitRock.normalized * -3;
				miniGunReticle.transform.position = placeHitRock + retPos;
				miniGunReticle.transform.forward = rayHit.normal;
			}
		}
	
		//this is for when it doesnt hit an object it still displays
		else{
			Vector3 vecEnd = tracerStart.transform.forward * range;
			Vector3 miniArmPos = tracerStart.transform.position; 
			Vector3 sendBack =  miniArmPos += vecEnd;
			// limit these angles properly

			print ("i hit nothing");

			Ray ray2No = new Ray(sendBack,cameraPlace.transform.position-sendBack);
			Debug.DrawRay(sendBack,cameraPlace.transform.position-sendBack, Color.yellow);
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

		//counts down untill switches cooldown rates
		warmUpTimer -= Time.deltaTime * 0.75f;
		
		//turning the warmUpTimer Update warmUpTimer ona nd off
		if(warmUpTimer <= 0){
			warmedUp = true;
		}
		if(warmUpTimer >= 0){
			warmedUp = false;
		}
		//when hitting overheated can't fire
		if(overHeat > 50){
			overHeated = true;
		}
		if(overHeat <= 0){
			overHeated = false;
		}
		//over heat recovery
		if(overHeated == true){
			overHeat -= Time.deltaTime * 8; 
		}

		else if(overHeat >=0 && fire == false){
			overHeat -= Time.deltaTime * 4; 
		}

		//fires the minigun based on  if there's ammo firing allowed and no cooldown left
		if (fire == true && cooldownRemaining <= 0 && overHeated == false) {

			overHeat ++;
			//gets the starting aimer angle
			Vector3 tempStart = miniGunAimer.transform.forward;
			//ads a randoma mount of spread to the angle
			Vector3 endShot =  tempStart + new Vector3 (Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F));
			Ray rayFire = new Ray (tracerStart.transform.position, endShot);;

			//handles effects
			GameObject tracer = tracerPool.Retrieve(tracerStart.transform.position);
			tracer.transform.forward = endShot;

			RaycastHit hitInfoFire;
			//fires the adjusted ray and maskes the retwall
			if (Physics.Raycast (rayFire, out hitInfoFire, range, mask)) {
				// if it hits a person do some damage  *********************
				Vector3 hitPointFire = hitInfoFire.point;
				//if graphic is there apply a bullet decal
				if (sparkPrefab != null && hitInfoFire.collider.tag != "Player") {
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfoFire.normal);
					Instantiate(sparkPrefab, hitPointFire + hitInfoFire.normal * 0.01f, hitRotation);
					}

				if(hitInfoFire.collider.tag == "Player"){
						doDamageMini();
						//add a hit graphic.
					}
		
				}
				//lower bullets whenever a shot is taken
				//still needs to be warmed up
				if(warmedUp == true){
					cooldownRemaining = coolDown;
					
				}
				//been warmed up
				else if(warmedUp == false){
					cooldownRemaining = coolDownWarmUp;
					
				}

			
		}

		//fires the cannon shot based on the cool down
		if(cannonShoot == true){
			if(cannonCDR <=0){
				GameObject currentCannonShot = cannonShot [cannonCounter];
				currentCannonShot.transform.position = cannonShotStart.transform.position;
				currentCannonShot.transform.forward = cannonAimer.transform.forward;
				currentCannonShot.SetActive(true);
				cannonCounter += 1;
				if(cannonCounter >= 4){
					cannonCounter = 1;
				}
				cannonCDR = cannonCD;
			}
		}
		
	//reload funtion trigger
	/*if (Input.GetKeyDown ("r")) {
		gunReload ();
	}
	*/


	}// end of update//

	void doDamageMini(){
		
		/*objectHealth h = go.GetComponent<objectHealth>();

					if(h != null)
					{
					h.ReciveDamage(damage);
					}*/
		
		// applies bullet spark to location fo impact
	}

	//reloading the minigun
	
	//ammo counter GUI
	void OnGUI() {
		GUI.Label (new Rect (15, 40, 200, 20), overHeat.ToString());
	}
}
