using UnityEngine;
using System.Collections;

public class TracerRoundScript : MonoBehaviour {

	PoolManager pool;
	float life = 5f;
	float speed = 300;
	float damage = 10;

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

		float rayAmnt = 300*Time.deltaTime;
		//fire a raycast ahead to ensure you wont miss and go through a collider
		Ray ray = new Ray(transform.position,transform.up);
		RaycastHit hitInfoFire;
		Debug.DrawRay(transform.position,transform.up * rayAmnt , Color.red);

		//turn into a sphere cast

		if (Physics.SphereCast (transform.position,1.5f,transform.forward, out hitInfoFire, 1.5f, mask)) 
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
				GameObject hitPlayer = hitInfoFire.collider.gameObject;
				PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
				hitPlayerScript.DamagePlayer(damage,gameObject.transform.up);
				//add a hit graphic.
			}
			//turn off the bullet prefab
			pool.Deactivate(gameObject);
		}
		
		/*
		Ray rayLeft = new Ray(transform.position,transform.forward * -0.5f);
		RaycastHit hitInfoLeft;
		Debug.DrawRay(transform.position,transform.forward * -0.5f, Color.red);
		if (Physics.Raycast (rayLeft,out hitInfoLeft, 0.5f,mask)) 
		{
			if (hitInfoLeft.collider.tag == "Player") {
				GameObject hitPlayer = hitInfoFire.collider.gameObject;
				PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();

				hitPlayerScript.DamagePlayer(damage,gameObject.transform.forward);
				pool.Deactivate(gameObject);
			}
		}
		
		Ray rayRight = new Ray(transform.position,transform.forward * 0.5f);
		RaycastHit hitInfoRight;
		Debug.DrawRay(transform.position,transform.forward * 0.5f, Color.red);
		if (Physics.Raycast (rayRight,out hitInfoRight, 0.5f,mask)) 
		{
			if (hitInfoRight.collider.tag == "Player") {
				GameObject hitPlayer = hitInfoFire.collider.gameObject;
				PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
				hitPlayerScript.DamagePlayer(damage,gameObject.transform.forward);
				pool.Deactivate(gameObject);
			}
		}
		*/
	}

	void OnEnable(){
		life = 5f;
	}	
}
