using UnityEngine;
using System.Collections;

public class MinigunFirer : MonoBehaviour {
	public GameObject miniGunArm;
	public GameObject [] cannonShot;
	public GameObject sparkPrefab;
	public GameObject cannonShotStart;

	public bool cannonShoot;
	public bool fire;

	public float range = 100.0f;
	public float coolDown = 0.05f;
	//ammo variables for minigun
	public float gunClipAmmo = 40f;
	public float currentClipAmmo = 40f;

	int cannonCounter;
	float cannonCD = 8f;
	float cooldownRemaining = 0;
	float cannonCDR = 0;
	int layerMask = 1 << 16;

	// Use this for initialization
	void Start () {
		cannonShoot = false;
		fire = false;
		layerMask = ~layerMask;
		cannonCounter = 1;
	}
	
	// Update is called once per frame
	void Update () {
		cooldownRemaining -= Time.deltaTime * 5;
		cannonCDR -= Time.deltaTime *5;

		//reload funtion trigger
		if (Input.GetKeyDown ("r")) {
			gunReload ();
		}

		if (currentClipAmmo >= 1 && fire == true && cooldownRemaining <= 0) {

			//gets the starting aimer angle
			Vector3 tempStart = gameObject.transform.forward;
			//ads a randoma mount of spread to the angle
			Vector3 startShot =  tempStart += new Vector3 (Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F), Random.Range (-0.02F, 0.02F));
			Ray ray = new Ray (gameObject.transform.position, startShot);
			RaycastHit hitInfo;
			//fires the adjusted ray
			if (Physics.Raycast (ray, out hitInfo, range,layerMask)) {
					Vector3 hitPoint = hitInfo.point;
					GameObject go = hitInfo.collider.gameObject;
					//if graphic is there apply a bullet decal
					if (sparkPrefab != null) {
						Instantiate (sparkPrefab, hitPoint, Quaternion.identity);
						
					}
					//lower bullets whenever a shot is taken
					currentClipAmmo -=1;
					cooldownRemaining = coolDown;
			}
		}
		if(cannonShoot == true){
			if(cannonCDR <=0){
				GameObject currentCannonShot = cannonShot [cannonCounter];
				currentCannonShot.transform.position = cannonShotStart.transform.position;
				currentCannonShot.transform.forward = cannonShotStart.transform.forward;

				currentCannonShot.SetActive(true);
				cannonCounter += 1;
				if(cannonCounter >= 4){
					cannonCounter = 1;
				}
				cannonCDR = cannonCD;
			}
		}

	}// end of update//

	void doDamageMini(){
		
		/*objectHealth h = go.GetComponent<objectHealth>();

					if(h != null)
					{
					h.ReciveDamage(damage);
					}*/
		
		// applies bullet spark to location fo impact
	}
	void gunReload()
	{
		currentClipAmmo = 40f;
	}

	//reloading the minigun
	
	//ammo counter GUI
	void OnGUI() {
		GUI.Label (new Rect (10, 10, 100, 20), currentClipAmmo.ToString());
	}
}
