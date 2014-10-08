using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//GameObjects needed
	public GameObject sparkPrefab;
	public GameObject[] rockets;
	//minigun things
	public float range = 100.0f;
	public float coolDown = 0.005f;
	public float damage = 50f;
	float cooldownRemaining = 0;
	
	//variables for rocketFire
	public float coolDownRocket = 2f;
	public GameObject rocketStart;
	public bool firingRockets = false;
	float cooldownRemainingRocket = 0;	
	float rockTimer = 0.1f;
	int rockCounter = 0;

	//public GameObject rocketPrefab;


	//regular gun ammo vars
	public float gunClipAmmo = 20f;
	public float currentClipAmmo = 20f;
	public float shotsTaken= 0;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

		//cooldowns
		cooldownRemaining -= Time.deltaTime;
		cooldownRemainingRocket -= Time.deltaTime;

		//Machine gun fire absed on cool down
		if(Input.GetMouseButton(0) && cooldownRemaining <=0){
			if(currentClipAmmo > 0){
				cooldownRemaining = coolDown;

				Ray ray = new Ray(gameObject.transform.position,gameObject.transform.forward);
				RaycastHit hitInfo;

				currentClipAmmo -=1;
				if(Physics.Raycast (ray, out hitInfo,range)){
					Vector3 hitPoint = hitInfo.point;
					GameObject go = hitInfo.collider.gameObject;
					/*objectHealth h = go.GetComponent<objectHealth>();

					if(h != null)
					{
						h.ReciveDamage(damage);
					}*/

					// applies bullet spark to location fo impact
						if(sparkPrefab !=null){
							Instantiate (sparkPrefab,hitPoint,Quaternion.identity);
					
					}
				}
			}
		}

		//reload funtion trigger
		if (Input.GetKeyDown ("r")) {
			gunReload();
		}

		//rocket fire based on coolDown
		if(Input.GetMouseButton(1) && cooldownRemainingRocket <=0)
		{
			cooldownRemainingRocket = coolDownRocket;
			firingRockets = true;
			//Instantiate(rocketPrefab,Camera.main.transform.position,Camera.main.transform.rotation);
			
		}

		if (firingRockets == true) {
			//spawn each rocket close to the rocket starting point at an intervaled time
			rockTimer -= Time.deltaTime;

			if(rockTimer <=0){
				GameObject currentRocket = rockets[rockCounter];
				Vector3 rocketLaunch = rocketStart.transform.position += new Vector3(Random.Range(-0.25F, 0.25F), Random.Range(-0.25F, 0.25F), Random.Range(-0.25F, 0.25F));
				currentRocket.transform.position = rocketLaunch;
				currentRocket.SetActive(true);
				rockTimer = 0.1f;
				rockCounter += 1;
				if(rockCounter >=4){
					firingRockets = false;
					rockCounter = 0;
				}
			}
		}

	}

	void gunReload()
	{
		currentClipAmmo = 20f;
	}

	void OnGUI() {
		GUI.Label (new Rect (10, 10, 100, 20), currentClipAmmo.ToString());
	}

}
