using UnityEngine;
using System.Collections;

public class MinigunFirer : MonoBehaviour {

	public GameObject sparkPrefab;
	public bool fire;
	public float range = 100.0f;
	public float coolDown = 0.5f;
	float cooldownRemaining = 0;

	//ammo variables for minigun
	public float gunClipAmmo = 40f;
	public float currentClipAmmo = 40f;

	// Use this for initialization
	void Start () {
		fire = false;
	}
	
	// Update is called once per frame
	void Update () {
		cooldownRemaining -= Time.deltaTime * 5;

		//reload funtion trigger
		if (Input.GetKeyDown ("r")) {
			gunReload ();
		}
		if (currentClipAmmo >= 1 && fire == true && cooldownRemaining <= 0) {

			//add some form of recoil or bullet spread when firing ****************************

			Ray ray = new Ray (gameObject.transform.position, gameObject.transform.forward);
			RaycastHit hitInfo;
			if (Physics.Raycast (ray, out hitInfo, range)) {
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
