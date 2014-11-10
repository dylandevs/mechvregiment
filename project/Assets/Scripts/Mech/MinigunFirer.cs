using UnityEngine;
using System.Collections;

public class MinigunFirer : MonoBehaviour {

	public GameObject sparkPrefab;
	public bool fire;
	public float range = 100.0f;
	public float coolDown = 0.1f;
	float cooldownRemaining = 0;

	//ammo variables for minigun
	public float gunClipAmmo = 40f;
	public float currentClipAmmo = 40f;

	int layerMask = 1 << 16;

	// Use this for initialization
	void Start () {
		fire = false;
		layerMask = ~layerMask;
	}
	
	// Update is called once per frame
	void Update () {
		cooldownRemaining -= Time.deltaTime * 5;

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

					/*objectHealth h = go.GetComponent<objectHealth>();

					if(h != null)
					{
					h.ReciveDamage(damage);
					}*/
					
					// applies bullet spark to location fo impact
					if (sparkPrefab != null) {
						Instantiate (sparkPrefab, hitPoint, Quaternion.identity);
						
					}
					//lower bullets whenever a shot is taken
					currentClipAmmo -=1;
					cooldownRemaining = coolDown;
			}
		}
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
