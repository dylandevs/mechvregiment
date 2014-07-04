using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	float damage = 0;
	string originator = "Default";
	Vector3 velocity = Vector3.zero;
	Vector3 lastPos = Vector3.zero;
	float life = 3.0f;

	// Use this for initialization
	void Start () {
		
	}

	public void setProperties(float baseDamage, string firer, Vector3 direction, float speed){
		damage = baseDamage;
		originator = firer;
		velocity = direction.normalized * speed;
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
		transform.position += velocity * Time.deltaTime;

	}

	// Checks for collision since last update cycle
	bool checkForwardCollision(){
		RaycastHit rayHit;
		if (Physics.Raycast(lastPos, velocity, out rayHit, velocity.magnitude * Time.deltaTime)){

			if (rayHit.collider.GetType() != typeof(TerrainCollider)){
				//print("hit");
				Player playerHit;
				BotAI botHit;
				if (playerHit = rayHit.collider.GetComponent<Player>()){
					if (originator != playerHit.faction){
						playerHit.Damage(damage);
					}
				}
				else if (botHit = rayHit.collider.GetComponent<BotAI>()){
					if (originator != botHit.faction){
						botHit.Damage(damage);
					}
				}
			}
			return true;
		}
		return false;
	}
}
