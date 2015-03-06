using UnityEngine;
using System.Collections;

public class TracerRoundScript : MonoBehaviour {

	PoolManager pool;
	float life = 5f;
	float speed = 200f;
	float damage = 50f;

	public LayerMask mask;

	public PoolManager sparkPool;

	public GameObject spark;

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {

		gameObject.transform.Translate (Vector3.up * speed * Time.deltaTime);

		life -= Time.deltaTime;
		if (life <= 0) {
			pool.Deactivate(gameObject);
		}
	}

	void OnEnable(){
		life = 5f;
	}

	void FixedUpdate () {
		//fire a raycast ahead to ensure you wont miss and go through a collider
		Ray ray = new Ray(transform.position,transform.up);

		RaycastHit hitInfoFire;

		if (Physics.Raycast (ray,out hitInfoFire, 8,mask)) 
		{
			Vector3 hitInfoFirePoint = hitInfoFire.point;
			//if graphic is there apply a bullet decal

			if (hitInfoFire.collider.tag != "Player") {

				//Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfoFire.normal);
				//and add a remaining bullet hole
				GameObject spark = sparkPool.Retrieve(hitInfoFirePoint);
				spark.transform.up = hitInfoFire.transform.up;
			}
			
			if(hitInfoFire.collider.tag == "Player"){
				doDamageMini();
				print ("hit player");


				//add a hit graphic.


			}
			//turn off the bullet prefab
			pool.Deactivate(gameObject);
		}
	}

	void doDamageMini(){
		pool.Deactivate(gameObject);
		/*objectHealth h = go.GetComponent<objectHealth>();

					if(h != null)
					{
					h.ReciveDamage(damage);
					}*/
		
		// applies bullet spark to location fo impact
	}
}
