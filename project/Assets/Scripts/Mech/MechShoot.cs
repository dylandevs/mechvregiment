using UnityEngine;
using System.Collections;

public class MechShoot : MonoBehaviour {
	//variables for machine gun
	public float range = 100.0f;
	public float coolDown = 0.2f;
	float cooldownRemaining = 0;
	public float damage = 50f;
	public GameObject sparkPrefab;

	//variables for rocketFire
	public float coolDownRocket = 2f;
	float cooldownRemainingRocket = 0;	
	public GameObject rocketPrefab;

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

				Ray ray = new Ray(Camera.main.transform.position,Camera.main.transform.forward);
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
			Instantiate(rocketPrefab,Camera.main.transform.position,Camera.main.transform.rotation);
			
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
