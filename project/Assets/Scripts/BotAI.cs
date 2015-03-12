using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// State values
	/*const byte AllClear = 0;
	const byte Searching = 1;
	const byte Sighted = 2;
	const byte Firing = 3;*/

	public enum State{AllClear, Searching, Approaching, VeryClose};
	enum TargetComponent{Head, Torso, Feet, None};

	// Distance thresholds
	const int ThreshFire = 18;
	const int ThreshApproach = 20;
	const int ThreshClose = 12;
	const int ThreshDangerClose = 6;
	const int ThreshSearch = 32;
	const int ThreshHearing = 42;

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
	float searchTime = 0;
	float searchDelay = 0;
	const float SearchDelayLow = 3;
	const float SearchDelayHigh = 5;
	const float AlertDuration = 8;
	const float SearchDuration = 12;
	const float SearchRad = 15;
	const float SearchTurnRate = 0.5f;

	// Bot attributes
	public const float ViewAngle = 120 / 2;
	public const float WalkSpeed = 1.5f;
	public const float MoveSpeed = 7f;
	public const float FireRate = 1.2f;
	public const float TurnStep = 0.02f;
	public const float FireAngle = 60 / 2;
	public const float MaxHealth = 100;
	public const float ResightRate = 0.25f;
	public const float BulletSpeed = 50f;
	public const float BulletDamage = 5.5f;
	public LayerMask shootableLayer;

	// Storage variables
	public GameObject allyGroup;
	private BotAI[] allies;
	public GameObject playerGroup;
	private Player[] players;
	NavMeshAgent navMeshAgent;
	public Player currentTarget = null;

	// Bot stats
	public State state = State.AllClear;
	private State prevState = State.AllClear;
	TargetComponent targetVisible = TargetComponent.None;
	//byte state = 0;
	float health = MaxHealth;
	public float reloadProg = FireRate;
	bool isDead = false;
	float resightProg = ResightRate;
	
	public Vector3 lastSighted;
	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);

	// Inputs
	public PoolManager projectilePool;
	public PoolManager impactPool;
	private PoolManager pool;

	// Use this for initialization
	void Start () {
		navMeshAgent = GetComponent<NavMeshAgent>();
		pool = transform.parent.GetComponent<PoolManager>();

		allies = new BotAI[allyGroup.transform.childCount];
		for (int i = 0; i < allyGroup.transform.childCount; i++){
			allies[i] = allyGroup.transform.GetChild(i).GetComponent<BotAI>();
		}

		players = new Player[playerGroup.transform.childCount];
		for (int i = 0; i < playerGroup.transform.childCount; i++){
			players[i] = playerGroup.transform.GetChild(i).GetComponent<Player>();
		}
	}
	
	// Update is called once per frame
	void Update () {
		facing = transform.rotation * baseFacing;

		// Living behaviour
		if (!isDead){

			// Check first to see if target is alive
			if (currentTarget){
				if (currentTarget.isDead){
					currentTarget = null;
					state = State.Searching;
					searchTime = SearchDuration;
				}
			}

			if (!currentTarget) {
				AttemptAcquireTarget();
			}

			if (currentTarget){
				// Calculating useful values
				Vector3 diffVec = currentTarget.transform.position - transform.position;
				float angle = Vector3.Angle(facing, diffVec);

				CalculateCurrentState();

				// Determine if firing is appropriate
				if (diffVec.magnitude < ThreshFire && currentTarget && state > State.Searching){
					if (reloadProg >= FireRate && angle < ViewAngle){
						//fireInDirection(currentTarget.transform);
						FireAtPredictively(currentTarget.gameObject);
						reloadProg = 0;
					}
				}
			}

			// If all else fails, see where ally is looking
			else {
				AttemptAcquireAllyTarget();
			}

			// If still unable to visually spot target, listen
			if (state < State.Approaching){
				ListenForGunshots();
			}

			// Act based on state
			if (state == State.AllClear){
				if (prevState != state){
					currentTarget = null;
				}
				setSurfaceColour(safeCol);
				Idle();
			}
			else if (state == State.Searching){
				// If is newly searching
				if (prevState != state){
					currentTarget = null;
					navMeshAgent.speed = MoveSpeed;
				}

				setSurfaceColour(srchCol);
				Search();
			}
			else if (state == State.Approaching){
				// If newly approaching
				if (prevState != state){
					//alertTime = AlertDuration;
					navMeshAgent.speed = MoveSpeed;
				}

				navMeshAgent.destination = lastSighted;
				setSurfaceColour(warnCol);
			}
			else if (state == State.VeryClose){
				alertTime = AlertDuration;

				FaceTarget(currentTarget.gameObject);
				setSurfaceColour(dngrCol);
				navMeshAgent.velocity = Vector3.zero;
			}

			if (reloadProg < FireRate){
				reloadProg += Time.deltaTime;
			}
			if (alertTime > 0){
				alertTime -= Time.deltaTime;
			}
			if (searchTime > 0){
				searchTime -= Time.deltaTime;
			}
			prevState = state;

		}
		else{
			pool.Deactivate(gameObject);
		}
	}

	// Tries to find viable target from existing list
	void AttemptAcquireTarget(){
		float closestDistance = 0;

		foreach (Player player in players){
			if (player.gameObject.GetActive() && !player.isDead){
				float distance = Vector3.Distance(player.transform.position, transform.position);

				// Add players that are in sight, or extremely close
				if (IsInSight(player.gameObject) || distance < ThreshDangerClose){
					if (distance > closestDistance){
						currentTarget = player;
						closestDistance = distance;
					}
				}

				// When searching, more sensitive to players
				else if (distance < ThreshSearch && state == State.Searching){
					currentTarget = player;
					closestDistance = distance;
					alertTime = AlertDuration;
					break;
				}
			}
		}
	}

	void AttemptAcquireAllyTarget(){
		BotAI ally = null;
		if ((ally = AreAlliesAlarmed()) && ally.currentTarget){
			if (!ally.currentTarget.isDead){
				currentTarget = ally.currentTarget;
				lastSighted = ally.lastSighted;
			}

			//state = ally.state;
			
			//alertTime = ally.alertTime;
			//print (alertTime);
		}
	}

	void ListenForGunshots(){
		foreach (Player player in players){
			if (player.gameObject.GetActive()){
				if (player.GetCurrentWeapon().IsFiringAudibly()){
					if (Vector3.Distance (player.transform.position, transform.position) < ThreshHearing){
						state = State.Searching;
						lastSighted = player.transform.position;
						searchTime = SearchDuration;

						break;
					}
				}
			}
		}
	}
	
	void CalculateCurrentState(){
		State newState = state;
		resightProg -= Time.deltaTime;

		if (resightProg <= 0){

			float distanceToTarget = Vector3.Distance(currentTarget.transform.position, transform.position);

			// At extremely close distances, enemy has full awareness of nearby player
			if (distanceToTarget < ThreshDangerClose){
				alertTime = AlertDuration;
			}

			// If no targets nearby, check status of allies
			else if (state < State.Approaching){
				BotAI ally = null;
				if ((ally = AreAlliesAlarmed()) && ally.currentTarget){
					if (!ally.currentTarget.isDead){
						currentTarget = ally.currentTarget;
						lastSighted = ally.lastSighted;

						alertTime = ally.alertTime;
					}
				}
			}
			
			// If fully alert, can detect nearby enemies outside of FoV
			if (alertTime > 0){
				if (distanceToTarget < ThreshClose && IsSightUnobstructed(currentTarget.gameObject)){
					newState = State.VeryClose;
				}
				else {
					newState = State.Approaching;
				}

				lastSighted = currentTarget.transform.position;
			}
			else{
				// Calculate state based on visibility of current target
				if (IsInSight(currentTarget.gameObject)){
					if (distanceToTarget < ThreshClose){
						newState = State.VeryClose;
					}
					else if (distanceToTarget < ThreshApproach){
						newState = State.Approaching;
					}
					lastSighted = currentTarget.transform.position;
				}

				// Start searching area
				else if (prevState > State.Searching) {
					searchTime = SearchDuration;
					newState = State.Searching;
				}
			}

			resightProg = ResightRate;
		}

		state = newState;
	}

	bool IsSightUnobstructed(GameObject subject){
		Vector3 diffVec = subject.transform.position - transform.position;
		Vector3 headVec = diffVec + new Vector3 (0, subject.transform.collider.bounds.extents.y - 0.1f, 0);
		Vector3 feetVec = diffVec - new Vector3 (0, subject.transform.collider.bounds.extents.y - 0.1f, 0);

		// Cast ray to determine obstructions in sight
		RaycastHit rayHit;
		
		// Check head
		if (Physics.Raycast(transform.position, headVec, out rayHit)){
			// If hit target, no obstruction
			if (rayHit.collider.transform == subject.transform){	
				return true;
			}
		}
		else if (Physics.Raycast(transform.position, diffVec, out rayHit)){
			// If hit target, no obstruction
			if (rayHit.collider.transform == subject.transform){
				return true;
			}
		}
		// Check feet
		else if (Physics.Raycast(transform.position, feetVec, out rayHit)){	
			// If hit target, no obstruction
			if (rayHit.collider.transform == subject.transform){
				return true;
			}
		}
		
		return false;
	}

	// Check whether subject is in sight
	bool IsInSight(GameObject subject){
		Vector3 diffVec = subject.transform.position - transform.position;
		Vector3 headVec = diffVec + new Vector3 (0, subject.transform.collider.bounds.extents.y - 0.1f, 0);
		Vector3 feetVec = diffVec - new Vector3 (0, subject.transform.collider.bounds.extents.y - 0.1f, 0);
		float angle = Vector3.Angle(facing, diffVec);

		// Check if within FoV
		if (angle <= ViewAngle){
			
			// Cast ray to determine obstructions in sight
			RaycastHit rayHit;
			
			// Check head
			if (Physics.Raycast(transform.position, headVec, out rayHit)){
				// If hit target, no obstruction
				if (rayHit.collider.transform == subject.transform && rayHit.distance < ThreshApproach){	
					return true;
				}
			}
			else if (Physics.Raycast(transform.position, diffVec, out rayHit)){
				// If hit target, no obstruction
				if (rayHit.collider.transform == subject.transform && rayHit.distance < ThreshApproach){
					return true;
				}
			}
			// Check feet
			else if (Physics.Raycast(transform.position, feetVec, out rayHit)){	
				// If hit target, no obstruction
				if (rayHit.collider.transform == subject.transform && rayHit.distance < ThreshApproach){
					return true;
				}
			}
		}

		return false;
	}

	// Calculates direction to fire in based on last-seen target component
	Vector3 chooseFireTarget(){
		Vector3 fireDir = transform.forward;
		if (targetVisible == TargetComponent.Head){

		}
		else if (targetVisible == TargetComponent.Head){
			
		}
		else if (targetVisible == TargetComponent.Head){
			
		}

		return fireDir;
	}

	// Fires bullet predictively at target based on velocity
	void FireAtPredictively(GameObject target){
		Vector3 bulletGenPos = transform.position + facing;
		//Vector3 direction = new Vector3(target.transform.position.x, target.transform.position.y + target.collider.bounds.extents.y, target.transform.position.z) - bulletGenPos;
		float timeDelay = 0;

		float distanceSquare = Vector3.SqrMagnitude(target.transform.position - bulletGenPos);
		float targetVelSquare = target.rigidbody.velocity.sqrMagnitude;
		float bulletVelSquare = BulletSpeed * BulletSpeed;

		timeDelay = Mathf.Sqrt(distanceSquare / (Mathf.Abs(bulletVelSquare - targetVelSquare)));
		Vector3 direction = new Vector3(target.transform.position.x + target.rigidbody.velocity.x * timeDelay, target.transform.position.y + target.collider.bounds.extents.y + target.rigidbody.velocity.y * timeDelay, target.transform.position.z + target.rigidbody.velocity.z * timeDelay) - bulletGenPos;
	
		GameObject bullet = projectilePool.Retrieve(bulletGenPos, Quaternion.identity);
		Bullet bulletScript = bullet.GetComponent<Bullet>();
		bulletScript.setProperties(BulletDamage, gameObject.tag, direction, BulletSpeed, impactPool);
		bulletScript.shootableLayer = shootableLayer;
	}

	// Fires bullet in direction provided
	void fireInDirection(Transform target){
		Vector3 bulletGenPos = transform.position + facing;
		Vector3 direction = new Vector3(target.position.x, target.position.y + target.collider.bounds.extents.y, target.position.z) - bulletGenPos;

		GameObject bullet = projectilePool.Retrieve(bulletGenPos, Quaternion.identity);
		Bullet bulletScript = bullet.GetComponent<Bullet>();
		bulletScript.setProperties(BulletDamage, gameObject.tag, direction, BulletSpeed, impactPool);
		bulletScript.shootableLayer = shootableLayer;
	}

	// Gradually turns to face given target
	// NOTE: This is time step-based, meaning variable turning speeds. Consider using a fixed/max turn speed?
	void FaceTarget(GameObject target){

		// Shifting y coordinates into bot transform space
		Vector3 targetVec = target.transform.position;
		targetVec.y = transform.position.y;
		Vector3 newDiffVec = targetVec - transform.position;

		// Calculating and applying rotation
		Quaternion targRot = Quaternion.LookRotation(newDiffVec);
		transform.rotation = Quaternion.Lerp(transform.rotation, targRot, Time.time * TurnStep);
	}

	// TESTING: sets surface colour of model
	void setSurfaceColour(Color newCol){
		//renderer.material.color = newCol;
	}

	// Determines whether any allies in view are in alarmed state
	BotAI AreAlliesAlarmed(){
		float closestAllyDist = 0;
		BotAI closestAlly = null;
		State mostAlarmed = State.AllClear;

		foreach(BotAI ally in allies){
			if (ally.state >= State.Approaching && ally.gameObject.GetActive()){

				float distance = Vector3.Distance(ally.transform.position, transform.position);
				
				if (distance > closestAllyDist && distance <= ThreshApproach){
					closestAlly = ally;
					mostAlarmed = ally.state;
				}
			}
		}

		return closestAlly;
	}

	// Idle behaviour management
	void Idle(){

		// Increment time progress, set state if necessary
		actionTime += Time.deltaTime;

		if (actionTime > idleDelay && idleState != IdleMoving){
			navMeshAgent.stoppingDistance = 0;
			navMeshAgent.speed = WalkSpeed;
			idleState = IdleMoving;
			actionTime = 0;
			navMeshAgent.destination = GetRandPos(IdleWalkRad, transform.position);
			idleDelay = Random.Range(IdleDelayLow, IdleDelayHigh);
		}


		// Perform idle behaviours
		if (idleState == IdleTurning){
			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, IdleTurnRate, 0) * Mathf.Sin(actionTime));
		}
		else if (idleState == IdleMoving){
			// Stop moving if arrived or if time exceeded
			if (HasAgentArrivedAtDest() || actionTime > idleDelay) {
				navMeshAgent.Stop();
				idleState = IdleTurning;
				actionTime = 0;
				idleDelay = Random.Range(IdleDelayLow, IdleDelayHigh);
			}
		}
	}

	// Search behaviour management
	void Search(){

		// Search movement interval
		searchDelay -= Time.deltaTime;

		// Move to new position
		if (searchDelay <= 0){
			searchDelay = Random.Range(SearchDelayLow, SearchDelayHigh);
			navMeshAgent.destination = GetRandPos(SearchRad, lastSighted);
		}
		else if (HasAgentArrivedAtDest()){
			// Rotate side to side
			transform.rotation *= Quaternion.Euler(new Vector3(0, SearchTurnRate, 0) * Mathf.Sin(alertTime));
		}

		if (searchTime <= 0){
			state = State.AllClear;
		}
	}
	
	bool HasAgentArrivedAtDest(){
		float remainder = navMeshAgent.remainingDistance;
		return (remainder != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0);
	}

	// Gets random position on navmesh within given radius
	Vector3 GetRandPos(float radius, Vector3 centre){
		Vector3 chosenDir = Random.insideUnitSphere * radius;
		chosenDir += centre;
		NavMeshHit hit;
		NavMesh.SamplePosition(chosenDir, out hit, radius, 1);
		return hit.position;
	}

	// Deals damage to bot
	public void Damage(float damage, Player playerSource = null){
		health -= damage;

		// Schedule bot for death
		if (health <= 0){
			//Destroy (gameObject);
			isDead = true;
		}

		if (playerSource){
			currentTarget = playerSource;
			lastSighted = currentTarget.transform.position;
			alertTime = AlertDuration;
		}
	}
}
