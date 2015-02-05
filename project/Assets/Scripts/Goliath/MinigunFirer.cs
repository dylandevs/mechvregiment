using UnityEngine;
using System.Collections;

public class MinigunFirer : MonoBehaviour {
	public GameObject miniGunAimer;
	public GameObject miniGunArm; // for making bullets shoot in the right direction
	public GameObject [] cannonShot;
	public GameObject sparkPrefab;
	public GameObject cannonShotStart;
	public GameObject cannonAimer;

	public bool cannonShoot;
	public bool fire;

	public float range = 100.0f;

	float overHeat;
	float coolDown;
	float coolDownWarmUp;
	float warmUpTimer;
	int cannonCounter;
	float cannonCD = 8f;
	float cooldownRemaining = 0;
	float cannonCDR = 0;
	int layerMask = 1 << 16;
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
		layerMask = ~layerMask;
		cannonCounter = 1;
		//during warm up
		coolDownWarmUp = 1f;
		//after warmed up
		coolDown = 0.5f;
	}
	
	// Update is called once per frame
	void Update () {
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
			//whenever a bullet is fired increase the overHeatMeter
			overHeat ++;
			
			//gets the starting aimer angle
			Vector3 tempStart = miniGunAimer.transform.forward;
			//ads a randoma mount of spread to the angle
			Vector3 startShot =  tempStart += new Vector3 (Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F));
			Ray ray = new Ray (miniGunAimer.transform.position, startShot);

			RaycastHit hitInfo;
			//fires the adjusted ray
			if (Physics.Raycast (ray, out hitInfo, range,layerMask)) {
				// if it hits a person do some damage  *********************
				Vector3 hitPoint = hitInfo.point;
				//if graphic is there apply a bullet decal
				if (sparkPrefab != null) {
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfo.normal);
					Instantiate(sparkPrefab, hitPoint + hitInfo.normal * 0.01f, hitRotation);
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
