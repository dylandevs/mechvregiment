using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// State values
	const byte ALL_CLEAR = 0;
	const byte SIGHTED = 1;
	const byte FIRING = 2;

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

	Transform allyGroup = null;
	public GameObject opponent;
	public GameObject ammunition;
	float reloadProg = FIRE_RATE;

	NavMeshAgent navMeshAgent;

	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);

	byte state = 0;

	// Use this for initialization
	void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
		navMeshAgent.stoppingDistance = THRESH_CLOSE;
		navMeshAgent.speed = MOVE_SPEED;
		allyGroup = transform.parent;
	}
	
	// Update is called once per frame
	void Update () {
		facing = transform.rotation * baseFacing;
		//Vector3 newVel = Vector3.zero;


		// Calculating useful values
		Vector3 diffVec = opponent.transform.position - transform.position;
		float angle = Vector3.Angle(facing, diffVec);
		state = checkInSight(opponent.transform);

		if (allyGroup != null && state == ALL_CLEAR){
			if (alliesAlarmed()){
				if (diffVec.magnitude > THRESH_CLOSE){
					state = SIGHTED;
				}
				else {
					state = FIRING;
				}
			}
		}

		// Act based on state
		if (state == ALL_CLEAR){
			setSurfaceColour(safeCol);
			//navMeshAgent.velocity = Vector3.zero;
		}
		else if (state == SIGHTED){
			//faceTarget(opponent);
			//newVel += MOVE_SPEED * facing;
			navMeshAgent.destination = opponent.transform.position;
			setSurfaceColour(warnCol);
		}
		else if (state == FIRING){
			if (reloadProg >= FIRE_RATE && angle < FIRE_ANGLE){
				fireInDirection(opponent.transform);
				reloadProg = 0;
			}
			faceTarget(opponent.transform);
			setSurfaceColour(dngrCol);
			navMeshAgent.velocity = Vector3.zero;
		}

		// Keep reloading regardless of state
		reloadProg += Time.deltaTime;

		// Apply velocity
		//rigidbody.velocity = new Vector3(newVel.x, rigidbody.velocity.y, newVel.z);
	}

	// Determines whether given vector difference is near, mid, or long range and within FoV
	byte checkInSight(Transform target){//float angle, Vector3 diffVec){
		Vector3 diffVec = target.position - transform.position;
		float angle = Vector3.Angle(facing, diffVec);

		// Check if within FoV
		if (angle <= VIEW_ANGLE){

			// Cast ray to determine obstructions in sight
			RaycastHit rayHit;
			if (Physics.Raycast(transform.position, diffVec, out rayHit)){

				// If hit target, no obstruction
				if (rayHit.collider.transform == target){

					float distance = rayHit.distance;

					if (distance < THRESH_CLOSE){
						return FIRING;
					}
					else if (distance < THRESH_MED){
						return SIGHTED;
					}
				}
			}
		}

		return ALL_CLEAR;
	}

	// Fires bullet in direction provided
	void fireInDirection(Transform target){
		Vector3 direction = target.position - transform.position;

		Vector3 bulletGenPos = transform.position + facing;
		GameObject bullet = Instantiate(ammunition, bulletGenPos, Quaternion.identity) as GameObject;
		TempProjectile bulletScript = bullet.GetComponent<TempProjectile>();
		bulletScript.setDirection(direction);
	}

	// Gradually turns to face given target
	// NOTE: This is time step-based, meaning variable turning speeds. Consider using a fixed/max turn speed?
	void faceTarget(Transform target){

		// Shifting y coordinates into bot transform space
		Vector3 targetVec = target.position;
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

	public byte getState(){
		return state;
	}

	public Transform getTransform(){
		return transform;
	}

	// Determines whether any allies in view are in alarmed state
	bool alliesAlarmed(){
		BotAI[] allies = allyGroup.GetComponentsInChildren<BotAI>();
		foreach(BotAI ally in allies){
			if (ally.getState() > ALL_CLEAR){

				// Check if ally is in sight of current bot
				byte allyVisible = checkInSight(ally.getTransform());

				if (allyVisible >= SIGHTED){
					return true;
				}
			}
		}

		return false;
	}
}
