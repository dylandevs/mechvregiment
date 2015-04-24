using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	private float damage = 0;
	private Vector3 velocity = Vector3.zero;
	private Vector3 lastPos = Vector3.zero;
	private float life = 3.0f;
	private float LifeSpan = 3;

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
		GetComponent<Rigidbody>().velocity = direction.normalized * speed;
		bulletMarkPool = markPool;
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

			// Check for collisions
			bool collisionFound = checkForwardCollision();

			// Move forward (remember position)
			lastPos = transform.position;

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

			if (Physics.Raycast(lastPos, velocity, out rayHit, travelDist, shootableLayer)){
				//print (travelDist);

				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.AngleAxis(Random.Range(0, 360), rayHit.normal) * Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					bulletMarkPool.Retrieve(rayHit.point + rayHit.normal * 0.01f, hitRotation);
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
					if (!isAvatar){
						BotAI botHit = rayHit.collider.GetComponent<BotAI>();

						if (playerSource){
							botHit.Damage(damage, playerSource);
							playerSource.TriggerHitMarker();
							playerSource = null;
						}
						else{
							botHit.Damage(damage);
						}
					}
				}
				else if (rayHit.collider.gameObject.tag == "Goliath"){
					if (!isAvatar){
						GoliathDamager damager = rayHit.transform.GetComponent<GoliathDamager>();
						damager.DamageGoliath(damage, velocity);

						if (playerSource){
							playerSource.TriggerHitMarker();
							playerSource = null;
						}
					}
				}
				else if (rayHit.collider.gameObject.tag == "Shield"){
					Shield shield = rayHit.transform.GetComponent<Shield>();
					shield.DamageGoliath(damage, velocity, rayHit.point);

					if (playerSource){
						playerSource.TriggerHitMarker();
						playerSource = null;
					}
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
