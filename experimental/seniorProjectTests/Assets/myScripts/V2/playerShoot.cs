using UnityEngine;
using System.Collections;

public class playerShoot : MonoBehaviour {
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

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		cooldownRemaining -= Time.deltaTime;
		cooldownRemainingRocket -= Time.deltaTime;

		//Machine gun fire absed on cool down
		if(Input.GetMouseButton(0) && cooldownRemaining <=0){
			cooldownRemaining = coolDown;
			// may need to cast from further to avoid self shot
			Ray ray = new Ray(Camera.main.transform.position,Camera.main.transform.forward);
			RaycastHit hitInfo;

			if(Physics.Raycast (ray, out hitInfo,range)){
				Vector3 hitPoint = hitInfo.point;
				GameObject go = hitInfo.collider.gameObject;
				Debug.Log("Hit Something" + go.name);
				Debug.Log ("Hit Point:" + hitPoint);

				objectHealth h = go.GetComponent<objectHealth>();

				if(h != null)
				{
					h.ReciveDamage(damage);
				}

				// applies bullet spark to location fo impact
				if(sparkPrefab !=null){
					Instantiate (sparkPrefab,hitPoint,Quaternion.identity);

				}
			}
		}

		//rocket fire based on coolDown
		if(Input.GetMouseButton(1) && cooldownRemainingRocket <=0)
		{
			cooldownRemainingRocket = coolDownRocket;
			Instantiate(rocketPrefab,Camera.main.transform.position,Camera.main.transform.rotation);
			
		}


	}
}
