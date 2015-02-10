using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	float damage = 0;
	string originator = "Default";
	public Vector3 velocity = Vector3.zero;
	Vector3 lastPos = Vector3.zero;
	float life = 3.0f;

	PoolManager pool;
	PoolManager bulletMarkPool;

	// Use this for initialization
	void Start () {
		lastPos = transform.position;
	}

	public void setProperties(float baseDamage, string firer, Vector3 direction, float speed){
		damage = baseDamage;
		originator = firer;
		velocity = direction.normalized * speed;
		rigidbody.velocity = direction.normalized * speed;
		life = 3;
	}
	
	// Update is called once per frame
	void Update () {

		// Decrease life
		life -= Time.deltaTime;
		if (life <= 0){
			pool.Deactivate(gameObject);
		}

		// Check for collisions
		bool collisionFound = checkForwardCollision();

		// Move forward (remember position)
		lastPos = transform.position;

		if (collisionFound) {
			pool.Deactivate(gameObject);
		}
	}

	// Checks for collision since last update cycle
	bool checkForwardCollision(){
		RaycastHit rayHit;
		float travelDist = Vector3.Distance (lastPos, transform.position);
		if (travelDist > 0){
			if (Physics.Raycast(lastPos, velocity, out rayHit, travelDist)){
				//print (travelDist);

				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the terrain, make mark
					Quaternion hitRotation = Quaternion.FromToRotation(Vector3.up, rayHit.normal);
					GameObject mark = bulletMarkPool.Retrieve(rayHit.point + rayHit.normal * 0.01f, hitRotation);
					mark.GetComponent<BulletHoleBehaviour>().SetPool(bulletMarkPool);
					mark.GetComponent<BulletHoleBehaviour>().Initialize();
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					Player playerHit = rayHit.collider.GetComponent<Player>();
					playerHit.Damage(damage);
				}
				else if (rayHit.collider.gameObject.tag == "Enemy"){
					BotAI botHit = rayHit.collider.GetComponent<BotAI>();
					botHit.Damage(damage);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public void SetPool(PoolManager pooler){
		pool = pooler;
	}

	public void SetMarkPool(PoolManager pooler){
		bulletMarkPool = pooler;
	}
}
