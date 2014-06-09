using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// State values
	const byte ALL_CLEAR = 0;
	const byte SEARCHING = 1;
	const byte SIGHTED = 2;
	const byte FIRING = 3;

	// Idle behaviour attributes
	float actionTime = 0;
	int idleState = 0;
	float idleDelay = 4.5f;
	const float IDLE_DELAY_LOW = 3;
	const float IDLE_DELAY_HIGH = 6;
	const byte IDLE_TURNING = 0;
	const byte IDLE_MOVING = 1;
	const float IDLE_WALK_RAD = 5;
	const float IDLE_TURN_RATE = 0.5f;

	// Search behaviour attributes
	float alertTime = 0;
	float searchDelay = 0;
	const float SEARCH_DELAY_LOW = 2;
	const float SEARCH_DELAY_HIGH = 4;
	const float ALERT_DURATION = 10;
	const float SEARCH_RAD = 10;
	const float SEARCH_TURN_RATE = 0.5f;

	// Distance thresholds
	const int THRESH_CLOSE = 12;
	const int THRESH_MED = 20;
	const int THRESH_TOLERANCE = 2;

	// Bot attributes
	const float VIEW_ANGLE = 120 / 2;
	const float WALK_SPEED = 1.5f;
	const float MOVE_SPEED = 7f;
	const float FIRE_RATE = 1.2f;
	const float TURN_STEP = 0.02f;
	const float FIRE_ANGLE = 60 / 2;

	// Predefined state colours
	Color safeCol = new Color32(0, 255, 0, 1);
	Color warnCol = new Color32(255, 255, 0, 1);
	Color dngrCol = new Color32(255, 0, 0, 1);
	Color srchCol = new Color32(255, 0, 255, 1);

	Transform allyGroup = null;
	public GameObject opponent;
	public GameObject ammunition;
	float reloadProg = FIRE_RATE;

	NavMeshAgent navMeshAgent;
	public Vector3 lastSighted;

	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);

	byte state = 0;

	// Use this for initialization
	void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
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

		state = getCurrentState(angle, diffVec);

		// Check status of nearby allies
		if (allyGroup != null && state == ALL_CLEAR || state == SEARCHING){
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
			idle();
			//navMeshAgent.velocity = Vector3.zero;
		}
		else if (state == SEARCHING){
			setSurfaceColour(srchCol);
			search();
		}
		else if (state == SIGHTED){
			//faceTarget(opponent);
			//newVel += MOVE_SPEED * facing;
			navMeshAgent.stoppingDistance = THRESH_CLOSE;
			navMeshAgent.speed = MOVE_SPEED;
			navMeshAgent.destination = lastSighted;
			setSurfaceColour(warnCol);

			alertTime = ALERT_DURATION;
			lastSighted = opponent.transform.position;
		}
		else if (state == FIRING){
			if (reloadProg >= FIRE_RATE && angle < FIRE_ANGLE){
				fireInDirection(opponent.transform);
				reloadProg = 0;
			}
			faceTarget(opponent.transform);
			setSurfaceColour(dngrCol);
			navMeshAgent.velocity = Vector3.zero;

			alertTime = ALERT_DURATION;
			lastSighted = opponent.transform.position;
		}

		// Keep reloading regardless of state
		reloadProg += Time.deltaTime;

		// Apply velocity
		//rigidbody.velocity = new Vector3(newVel.x, rigidbody.velocity.y, newVel.z);
	}

	byte getCurrentState(float angle, Vector3 diffVec){
		byte newState = state;

		// Get state based on cone of vision
		newState = checkInSight(opponent.transform);

		// Determine if still searching
		if (alertTime > 0 && newState < SIGHTED){
			newState = SEARCHING;
		}

		// Check status of nearby allies
		if (allyGroup != null && state < SIGHTED){
			if (alliesAlarmed()){
				if (diffVec.magnitude > THRESH_CLOSE){
					newState = SIGHTED;
				}
				else {
					newState = FIRING;
				}
			}
		}

		return newState;
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
			if (ally.getState() >= SIGHTED){

				// Check if ally is in sight of current bot
				byte allyVisible = checkInSight(ally.getTransform());

				if (allyVisible >= SIGHTED){
					lastSighted = ally.lastSighted;
					return true;
				}
			}
		}

		return false;
	}

	// Resets idle variables
	void resetIdle(){
		actionTime = 0;
		idleDelay = 4.5f;
		idleState = 0;
		navMeshAgent.stoppingDistance = 0;
		navMeshAgent.speed = WALK_SPEED;
	}

	// Idle behaviour management
	void idle(){

		// Increment time progress, set state if necessary
		actionTime += Time.deltaTime;

		if (actionTime > idleDelay && idleState != IDLE_MOVING){
			navMeshAgent.stoppingDistance = 0;
			navMeshAgent.speed = WALK_SPEED;
			idleState = IDLE_MOVING;
			actionTime = 0;
			navMeshAgent.destination = getRandPos(IDLE_WALK_RAD, transform.position);
			idleDelay = Random.Range(IDLE_DELAY_LOW, IDLE_DELAY_HIGH);
		}


		// Perform idle behaviours
		if (idleState == IDLE_TURNING){
			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, IDLE_TURN_RATE, 0) * Mathf.Sin(actionTime));
		}
		else if (idleState == IDLE_MOVING){
			// Stop moving if arrived or if time exceeded
			if (hasAgentArrivedAtDest() || actionTime > idleDelay) {
				navMeshAgent.Stop();
				idleState = IDLE_TURNING;
				actionTime = 0;
				idleDelay = Random.Range(IDLE_DELAY_LOW, IDLE_DELAY_HIGH);
			}
		}
	}

	// Resets search variables
	void resetSearchVariables(){
		searchDelay = 0;
	}

	// Search behaviour management
	void search(){

		// Decrement alert progress
		alertTime -= Time.deltaTime;

		// Move to new position
		if (searchDelay <= 0){
			searchDelay = Random.Range(SEARCH_DELAY_LOW, SEARCH_DELAY_HIGH);
			navMeshAgent.destination = getRandPos(SEARCH_RAD, lastSighted);
		}
		else if (hasAgentArrivedAtDest()){
			searchDelay -= Time.deltaTime;

			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, SEARCH_TURN_RATE, 0) * Mathf.Sin(alertTime));
		}
	}
	
	bool hasAgentArrivedAtDest(){
		float remainder = navMeshAgent.remainingDistance;
		return (remainder != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0);
	}

	// Gets random position on navmesh within given radius
	Vector3 getRandPos(float radius, Vector3 centre){
		Vector3 chosenDir = Random.insideUnitSphere * radius;
		chosenDir += centre;
		NavMeshHit hit;
		NavMesh.SamplePosition(chosenDir, out hit, radius, 1);
		return hit.position;
	}
}
