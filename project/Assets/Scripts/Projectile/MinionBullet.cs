using UnityEngine;
using System.Collections;

public class MinionBullet : MonoBehaviour {

	public float damage = 5.5f;
	public float speed = 50;

	Vector3 lastPos = Vector3.zero;
	private float life = 3.0f;
	private float LifeSpan = 3;
	
	public LayerMask shootableLayer;
	
	PoolManager pool;
	public PoolManager bulletMarkPool;
	
	private bool toDeactivate = false;
	
	// For networked object behaviour
	public bool isAvatar = false;
	
	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {
		if (!toDeactivate){
			// Decrease life
			life -= Time.deltaTime;
			if (life <= 0){
				GetComponent<Rigidbody>().velocity = Vector3.zero;
				toDeactivate = true;
				GetComponent<ParticleEmitter>().emit = false;
			}

			// Move forward (remember position)
			lastPos = transform.position;
			transform.position += transform.forward * speed * Time.deltaTime;

			// Check for collisions
			bool collisionFound = checkForwardCollision();
			
			if (collisionFound) {
				GetComponent<Rigidbody>().velocity = Vector3.zero;
				toDeactivate = true;
				GetComponent<ParticleEmitter>().emit = false;
			}
		}
		else{
			// Allow trail to fade out
			if (GetComponent<ParticleEmitter>().particleCount == 0){
				pool.Deactivate(gameObject);
			}
		}
	}
	
	// Checks for collision since last update cycle
	bool checkForwardCollision(){
		RaycastHit rayHit;
		float travelDist = Vector3.Distance (lastPos, transform.position);
		if (travelDist > 0){
			
			if (Physics.Raycast(lastPos, transform.forward, out rayHit, travelDist, shootableLayer)){
				//print (travelDist);
				
				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.AngleAxis(Random.Range(0, 360), rayHit.normal) * Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					bulletMarkPool.Retrieve(rayHit.point + rayHit.normal * 0.01f, hitRotation);
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					if (!isAvatar){
						PlayerDamager playerHit = rayHit.collider.GetComponent<PlayerDamager>();
						playerHit.DamagePlayer(damage, transform.forward);
					}
				}
				else if (rayHit.collider.gameObject.tag == "Enemy"){

				}
				else if (rayHit.collider.gameObject.tag == "Goliath"){

				}
				return true;
			}
			return false;
		}
		return false;
	}
	
	void OnEnable(){
		life = LifeSpan;
		lastPos = transform.position;
		toDeactivate = false;
		GetComponent<ParticleEmitter>().emit = true;
	}
}
