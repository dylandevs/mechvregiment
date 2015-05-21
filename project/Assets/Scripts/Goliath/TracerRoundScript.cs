using UnityEngine;
using System.Collections;

public class TracerRoundScript : MonoBehaviour {

	PoolManager pool;
	float life = 5f;
	float speed = 300;
	float damage = 2;

	public LayerMask mask;

	public PoolManager sparkPool;

	public GameObject spark;
	public bool isAvatar = false;

	public GameObject miniGunHit;

	Vector3 previousPos;

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {

		if(previousPos != null){
			Ray rayPos = new Ray(previousPos,transform.up);
			RaycastHit hitInfoFirePos;

			Vector3 dist = previousPos - transform.position;
			float trueDist = dist.magnitude;

			if (Physics.Raycast (rayPos,out hitInfoFirePos,trueDist,mask)) 
			{

				if (hitInfoFirePos.collider.tag != "Player") {
					GameObject spark = sparkPool.Retrieve(hitInfoFirePos.point);
					spark.transform.up = hitInfoFirePos.transform.up;
				}

				if (hitInfoFirePos.collider.tag == "Dummy") {
					GameObject hitPlayer = hitInfoFirePos.collider.gameObject;
					dummyDamager hitDummy = hitPlayer.GetComponent<dummyDamager>();
					hitDummy.damageDummy(damage);
				}
				
				if(hitInfoFirePos.collider.tag == "Enemy" && !isAvatar){
					if(miniGunHit.GetActive() == false){
						miniGunHit.SetActive(true);
					}
					GameObject hitMinion = hitInfoFirePos.collider.gameObject;
					MinionAvatar minionScript = hitMinion.GetComponent<MinionAvatar>();
					minionScript.Damage(damage);

				}
				
				if(hitInfoFirePos.collider.tag == "Player" && !isAvatar){
					GameObject hitPlayer = hitInfoFirePos.collider.gameObject;
					PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
					hitPlayerScript.DamagePlayer(damage,gameObject.transform.up);
					//add a hit graphic.
					if(miniGunHit.GetActive() == false){
						miniGunHit.SetActive(true);
					}
				}

				pool.Deactivate(gameObject);

			}
		}

		previousPos = transform.position;

		gameObject.transform.Translate (Vector3.up * speed * Time.deltaTime);

		life -= Time.deltaTime;
		if (life <= 0) {
			pool.Deactivate(gameObject);
		}

		float rayAmnt = 300*Time.deltaTime;
		//fire a raycast ahead to ensure you wont miss and go through a collider
		Ray ray = new Ray(transform.position,transform.up);
		RaycastHit hitInfoFire;

		//turn into a sphere cast

		if (Physics.SphereCast (transform.position,1f,transform.forward, out hitInfoFire, 1f, mask)) 
		{
			Vector3 hitInfoFirePoint = hitInfoFire.point;

			if (hitInfoFire.collider.tag != "Player") {
				//Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, hitInfoFire.normal);
				//and add a remaining bullet hole
				GameObject spark = sparkPool.Retrieve(hitInfoFirePoint);
				spark.transform.up = hitInfoFire.transform.up;
			}

			if(hitInfoFire.collider.tag == "Enemy" && !isAvatar){
				if(miniGunHit.GetActive() == false){
					miniGunHit.SetActive(true);
				}
				GameObject hitMinion = hitInfoFire.collider.gameObject;
				MinionAvatar minionScript = hitMinion.GetComponent<MinionAvatar>();
				minionScript.Damage(damage);
			}

			if(hitInfoFire.collider.tag == "Player" && !isAvatar){
				GameObject hitPlayer = hitInfoFire.collider.gameObject;
				PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
				hitPlayerScript.DamagePlayer(damage,gameObject.transform.up);
				//add a hit graphic.
				if(miniGunHit.GetActive() == false){
					miniGunHit.SetActive(true);
				}
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
		previousPos = transform.position - transform.forward;
		life = 5f;
	}	
}
