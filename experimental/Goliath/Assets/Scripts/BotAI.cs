using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// State values
	const byte AllClear = 0;
	const byte Searching = 1;
	const byte Sighted = 2;
	const byte Firing = 3;

	// Predefined state colours
	Color safeCol = new Color32(0, 255, 0, 1);
	Color warnCol = new Color32(255, 255, 0, 1);
	Color dngrCol = new Color32(255, 0, 0, 1);
	Color srchCol = new Color32(255, 0, 255, 1);

	// Idle behaviour attributes
	float actionTime = 0;
	int idleState = 0;
	float idleDelay = 4.5f;
	const float IdleDelayLow = 3;
	const float IdleDelayHigh = 6;
	const byte IdleTurning = 0;
	const byte IdleMoving = 1;
	const float IdleWalkRad = 5;
	const float IdleTurnRate = 0.5f;

	// Search behaviour attributes
	float alertTime = 0;
	float searchDelay = 0;
	const float SearchDelayLow = 4;
	const float SearchDelayHigh = 7;
	const float AlertDuration = 20;
	const float SearchRad = 15;
	const float SearchTurnRate = 0.5f;

	// Distance thresholds
	const int ThreshClose = 12;
	const int ThreshMed = 20;
	const int ThreshTolerance = 2;

	// Bot attributes
	const float ViewAngle = 120 / 2;
	const float WalkSpeed = 1.5f;
	const float MoveSpeed = 7f;
	const float FireRate = 1.2f;
	const float TurnStep = 0.02f;
	const float FireAngle = 60 / 2;
	const float MaxHealth = 100;
	const float ResightRate = 0.25f;

	// Storage variables
	Transform allyGroup = null;
	public GameObject opponent;
	public GameObject ammunition;
	NavMeshAgent navMeshAgent;
	
	// Bot stats
	byte state = 0;
	float health = MaxHealth;
	float reloadProg = FireRate;
	bool isDead = false;
	float resightEnemyProg = ResightRate;
	float resightAllyProg = ResightRate;
	
	public Vector3 lastSighted;
	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);

	// Use this for initialization
	void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
		allyGroup = transform.parent;
	}
	
	// Update is called once per frame
	void Update () {
		facing = transform.rotation * baseFacing;
		//Vector3 newVel = Vector3.zero;

		// Living behaviour
		if (!isDead){
			// Calculating useful values
			Vector3 diffVec = opponent.transform.position - transform.position;
			float angle = Vector3.Angle(facing, diffVec);

			//state = checkInSight(opponent.transform);

			state = getCurrentState(angle, diffVec);

			/*// Check status of nearby allies
			if (allyGroup != null && state == AllClear || state == Searching){
				if (alliesAlarmed()){
					if (diffVec.magnitude > ThreshClose){
						state = Sighted;
					}
					else {
						state = Firing;
					}
				}
			}*/

			// Act based on state
			if (state == AllClear){
				setSurfaceColour(safeCol);
				idle();
				//navMeshAgent.velocity = Vector3.zero;
			}
			else if (state == Searching){
				navMeshAgent.stoppingDistance = 0;
				navMeshAgent.speed = MoveSpeed;
				setSurfaceColour(srchCol);
				search();
			}
			else if (state == Sighted){
				//faceTarget(opponent);
				//newVel += MoveSpeed * facing;
				navMeshAgent.stoppingDistance = ThreshClose;
				navMeshAgent.speed = MoveSpeed;
				navMeshAgent.destination = lastSighted;
				setSurfaceColour(warnCol);

				alertTime = AlertDuration;
				lastSighted = opponent.transform.position;
			}
			else if (state == Firing){
				if (reloadProg >= FireRate && angle < ViewAngle){
					fireInDirection(opponent.transform);
					reloadProg = 0;
				}
				faceTarget(opponent.transform);
				setSurfaceColour(dngrCol);
				navMeshAgent.velocity = Vector3.zero;

				alertTime = AlertDuration;
				lastSighted = opponent.transform.position;
			}

			// Keep reloading regardless of state
			reloadProg += Time.deltaTime;

		}
		else{
			Destroy(gameObject);
		}
	}

	byte getCurrentState(float angle, Vector3 diffVec){
		byte newState = state;
		resightAllyProg -= Time.deltaTime;
		resightEnemyProg -= Time.deltaTime;

		// Get state based on cone of vision
		if (resightEnemyProg <= 0) {
			newState = checkInSight (opponent.transform);
			resightEnemyProg = ResightRate;
		}

		// Determine if still searching
		if (alertTime > 0 && newState < Sighted){
			newState = Searching;
		}

		// Check status of nearby allies
		if (allyGroup != null && state < Sighted && resightAllyProg <= 0){
			if (alliesAlarmed()){
				if (diffVec.magnitude > ThreshClose){
					newState = Sighted;
				}
				else {
					newState = Firing;
				}
			}
			resightAllyProg = ResightRate;
		}

		return newState;
	}

	// Determines whether given vector difference is near, mid, or long range and within FoV
	byte checkInSight(Transform target){//float angle, Vector3 diffVec){
		Vector3 diffVec = target.position - transform.position;
		Vector3 headVec = diffVec + new Vector3 (0, target.collider.bounds.extents.y - 0.1f, 0);
		Vector3 feetVec = diffVec - new Vector3 (0, target.collider.bounds.extents.y - 0.1f, 0);
		float angle = Vector3.Angle(facing, diffVec);
		byte returnState = AllClear;

		// Check if within FoV
		if (angle <= ViewAngle){

			// Cast ray to determine obstructions in sight
			RaycastHit rayHit;
			if (Physics.Raycast(transform.position, diffVec, out rayHit)){

				// If hit target, no obstruction
				if (rayHit.collider.transform == target){

					float distance = rayHit.distance;

					if (distance < ThreshClose){
						returnState = Firing;
					}
					else if (distance < ThreshMed){
						returnState = Sighted;
					}
					print ("torso");
				}
			}

			// Check head
			if (returnState == AllClear){
				if (Physics.Raycast(transform.position, headVec, out rayHit)){
					
					// If hit target, no obstruction
					if (rayHit.collider.transform == target){
						
						float distance = rayHit.distance;
						
						if (distance < ThreshClose){
							returnState = Firing;
						}
						else if (distance < ThreshMed){
							returnState = Sighted;
						}
						print ("head");
					}
				}
			}

			// Check feet
			if (returnState == AllClear){
				if (Physics.Raycast(transform.position, feetVec, out rayHit)){
					
					// If hit target, no obstruction
					if (rayHit.collider.transform == target){
						
						float distance = rayHit.distance;
						
						if (distance < ThreshClose){
							returnState = Firing;
						}
						else if (distance < ThreshMed){
							returnState = Sighted;
						}
						print ("feet");
					}
				}
			}
		}

		return returnState;
	}

	// Fires bullet in direction provided
	void fireInDirection(Transform target){
		Vector3 direction = new Vector3(target.position.x, target.position.y + target.collider.bounds.extents.y, target.position.z) - transform.position;
		Vector3 bulletGenPos = transform.position + facing;

		GameObject bullet = Instantiate(ammunition, bulletGenPos, Quaternion.identity) as GameObject;
		Bullet bulletScript = bullet.GetComponent<Bullet>();
		bulletScript.setProperties(5.5f, gameObject.tag, direction, 4);

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
		transform.rotation = Quaternion.Lerp(transform.rotation, targRot, Time.time * TurnStep);
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
		// TODO: Add StatePos object

		BotAI[] allies = allyGroup.GetComponentsInChildren<BotAI>();
		foreach(BotAI ally in allies){
			if (ally.getState() >= Searching){

				// Check if ally is in sight of current bot
				byte allyVisible = checkInSight(ally.getTransform());

				if (allyVisible >= Searching){
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
		navMeshAgent.speed = WalkSpeed;
	}

	// Idle behaviour management
	void idle(){

		// Increment time progress, set state if necessary
		actionTime += Time.deltaTime;

		if (actionTime > idleDelay && idleState != IdleMoving){
			navMeshAgent.stoppingDistance = 0;
			navMeshAgent.speed = WalkSpeed;
			idleState = IdleMoving;
			actionTime = 0;
			navMeshAgent.destination = getRandPos(IdleWalkRad, transform.position);
			idleDelay = Random.Range(IdleDelayLow, IdleDelayHigh);
		}


		// Perform idle behaviours
		if (idleState == IdleTurning){
			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, IdleTurnRate, 0) * Mathf.Sin(actionTime));
		}
		else if (idleState == IdleMoving){
			// Stop moving if arrived or if time exceeded
			if (hasAgentArrivedAtDest() || actionTime > idleDelay) {
				navMeshAgent.Stop();
				idleState = IdleTurning;
				actionTime = 0;
				idleDelay = Random.Range(IdleDelayLow, IdleDelayHigh);
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
		searchDelay -= Time.deltaTime;

		// Move to new position
		if (searchDelay <= 0){
			searchDelay = Random.Range(SearchDelayLow, SearchDelayHigh);
			navMeshAgent.destination = getRandPos(SearchRad, lastSighted);
		}
		else if (hasAgentArrivedAtDest()){
			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, SearchTurnRate, 0) * Mathf.Sin(alertTime));
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

	// Deals damage to bot
	public void Damage(float damage){
		health -= damage;

		// Schedule bot for death
		if (health <= 0){
			//Destroy (gameObject);
			isDead = true;
		}
	}
}
