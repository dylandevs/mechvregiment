using UnityEngine;
using System.Collections;

public class MinionBullet : MonoBehaviour {

	float damage = 0;
	public Vector3 velocity = Vector3.zero;
	Vector3 lastPos = Vector3.zero;
	float life = 3.0f;
	public float LifeSpan = 3;
	
	public Player playerSource = null;
	public LayerMask shootableLayer;
	
	PoolManager pool;
	PoolManager bulletMarkPool;
	
	private bool toDeactivate = false;
	
	// For networked object behaviour
	public bool isAvatar = false;
	
	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	public void setProperties(float baseDamage, Vector3 direction, float speed, PoolManager markPool){
		damage = baseDamage;
		velocity = direction.normalized * speed;
		rigidbody.velocity = direction.normalized * speed;
		bulletMarkPool = markPool;
	}
	
	// Update is called once per frame
	void Update () {
		if (!toDeactivate){
			// Decrease life
			life -= Time.deltaTime;
			if (life <= 0){
				rigidbody.velocity = Vector3.zero;
				toDeactivate = true;
				particleEmitter.emit = false;
			}
			
			// Check for collisions
			bool collisionFound = checkForwardCollision();
			
			// Move forward (remember position)
			lastPos = transform.position;
			
			if (collisionFound) {
				rigidbody.velocity = Vector3.zero;
				toDeactivate = true;
				particleEmitter.emit = false;
			}
		}
		else{
			// Allow trail to fade out
			if (particleEmitter.particleCount == 0){
				pool.Deactivate(gameObject);
			}
		}
	}
	
	// Checks for collision since last update cycle
	bool checkForwardCollision(){
		RaycastHit rayHit;
		float travelDist = Vector3.Distance (lastPos, transform.position);
		if (travelDist > 0){
			
			if (Physics.Raycast(lastPos, velocity, out rayHit, travelDist, shootableLayer)){
				//print (travelDist);
				
				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					GameObject mark = bulletMarkPool.Retrieve(rayHit.point + rayHit.normal * 0.01f, hitRotation);
					mark.GetComponent<BulletHoleBehaviour>().Initialize();
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					if (!isAvatar){
						PlayerDamager playerHit = rayHit.collider.GetComponent<PlayerDamager>();
						playerHit.DamagePlayer(damage, velocity);
						if (playerSource){
							playerSource.TriggerHitMarker();
							playerSource = null;
						}
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
		particleEmitter.emit = true;
	}
}
