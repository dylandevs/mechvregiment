using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	float damage = 0;
	string originator = "Default";
	public Vector3 velocity = Vector3.zero;
	Vector3 lastPos = Vector3.zero;
	float life = 3.0f;

	// Use this for initialization
	void Start () {
		lastPos = transform.position;
	}

	public void setProperties(float baseDamage, string firer, Vector3 direction, float speed){
		damage = baseDamage;
		originator = firer;
		velocity = direction.normalized * speed;
		rigidbody.velocity = direction.normalized * speed;
	}
	
	// Update is called once per frame
	void Update () {

		// Decrease life
		life -= Time.deltaTime;
		if (life <= 0){
			Destroy(gameObject);
		}

		// Check for collisions
		bool collisionFound = checkForwardCollision();

		// Move forward (remember position)
		lastPos = transform.position;
		//transform.position += velocity * Time.deltaTime;

		if (collisionFound) {
			Destroy(gameObject);
		}
	}

	// Checks for collision since last update cycle
	bool checkForwardCollision(){
		RaycastHit rayHit;
		float travelDist = Vector3.Distance (lastPos, transform.position);
		if (travelDist > 0){
			if (Physics.Raycast(lastPos, velocity, out rayHit, travelDist)){
				print (rayHit.collider.gameObject.tag);
				//print (travelDist);

				if (rayHit.collider.gameObject.tag == "Terrain"){
					// Hit the ground, do nothing
				}
				else if (rayHit.collider.gameObject.tag == "Player"){
					Player playerHit = rayHit.collider.GetComponent<Player>();
					playerHit.Damage(damage);
					//print("hit player");
				}
				else if (rayHit.collider.gameObject.tag == "Enemy"){
					BotAI botHit = rayHit.collider.GetComponent<BotAI>();
					botHit.Damage(damage);
					//print("hit enemy");
				}
				return true;
			}
			return false;
		}
		return false;
	}
}
