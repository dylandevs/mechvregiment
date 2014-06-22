using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour {

	int damage = 10;
	string originator = "Default";
	Vector3 velocity = Vector3.zero;
	Vector3 lastPos = Vector3.zero;
	float life = 3.0f;

	// Use this for initialization
	void Start () {
		
	}

	public void setProperties(int newDamage, string firer, Vector3 direction, float speed){
		damage = newDamage;
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
				print("hit");
			}
			return true;
		}
		return false;
	}
}
