using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// Distance thresholds
	const int THRESH_CLOSE = 12;
	const int THRESH_MED = 20;

	// Bot attributes
	const float VIEW_ANGLE = 120 / 2;
	const float MOVE_SPEED = 7f;
	const float FIRE_RATE = 1.2f;
	const float TURN_STEP = 0.02f;
	const float FIRE_ANGLE = 60 / 2;

	// Predefined state colours
	Color safeCol = new Color32(0, 255, 0, 1);
	Color warnCol = new Color32(255, 255, 0, 1);
	Color dngrCol = new Color32(255, 0, 0, 1);

	public GameObject opponent;
	public GameObject ammunition;
	float reloadProg = FIRE_RATE;

	NavMeshAgent navMeshAgent;

	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);

	// Use this for initialization
	void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
		navMeshAgent.stoppingDistance = THRESH_CLOSE;
		navMeshAgent.speed = MOVE_SPEED;
	}
	
	// Update is called once per frame
	void Update () {
		facing = transform.rotation * baseFacing;
		//Vector3 newVel = Vector3.zero;


		// Calculating useful values
		Vector3 diffVec = opponent.transform.position - transform.position;
		float angle = Vector3.Angle(facing, diffVec);

		switch (checkInSight(angle, diffVec)){

		// Out of view
		case 0:
			setSurfaceColour(safeCol);
			//navMeshAgent.velocity = Vector3.zero;
			break;

		// In view, out of range
		case 1:
			//faceTarget(opponent);
			//newVel += MOVE_SPEED * facing;
			navMeshAgent.destination = opponent.transform.position;
			setSurfaceColour(warnCol);
			break;

		// In range
		case 2:
			if (reloadProg >= FIRE_RATE && angle < FIRE_ANGLE){
				fireInDirection(diffVec);
				reloadProg = 0;
			}
			faceTarget(opponent);
			setSurfaceColour(dngrCol);
			navMeshAgent.velocity = Vector3.zero;
			break;

		default:
			break;
		}

		// Keep reloading regardless of state
		reloadProg += Time.deltaTime;

		// Apply velocity
		//rigidbody.velocity = new Vector3(newVel.x, rigidbody.velocity.y, newVel.z);
	}

	// Determines whether given vector difference is near, mid, or long range
	byte checkInSight(float angle, Vector3 diffVec){
		float distance = diffVec.magnitude;

		if (distance < THRESH_CLOSE && angle <= VIEW_ANGLE){
			return 2;
		}
		else if (distance < THRESH_MED && angle <= VIEW_ANGLE){
			return 1;
		}
		else{
			return 0;
		}
	}

	// Fires bullet in direction provided
	void fireInDirection(Vector3 direction){
		Vector3 bulletGenPos = transform.position + facing;
		GameObject bullet = Instantiate(ammunition, bulletGenPos, Quaternion.identity) as GameObject;
		TempProjectile bulletScript = bullet.GetComponent<TempProjectile>();
		bulletScript.setDirection(direction);
	}

	// Gradually turns to face given target
	// NOTE: This is time step-based, meaning variable turning speeds. Consider using a fixed turn speed?
	void faceTarget(GameObject target){

		// Shifting y coordinates into bot transform space
		Vector3 targetVec = target.transform.position;
		targetVec.y = transform.position.y;
		Vector3 newDiffVec = targetVec - transform.position;

		// Calculating and applying rotation
		Quaternion targRot = Quaternion.LookRotation(newDiffVec);
		transform.rotation = Quaternion.Lerp(transform.rotation, targRot, Time.time * TURN_STEP);
	}

	// TESTING: sets surface colour of model
	void setSurfaceColour(Color newCol){
		renderer.material.color = newCol;
	}
}
