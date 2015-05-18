using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	public enum State{AllClear, Traveling, Searching, Approaching, VeryClose};
	enum TargetComponent{Head, Torso, Feet, None};

	// Distance thresholds
	const int ThreshFire = 24;
	const int ThreshApproach = 30;
	const int ThreshClose = 12;
	const int ThreshDangerClose = 8;
	const int ThreshSearch = 38;
	const int ThreshHearing = 44;

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

	const float WaypointRad = 7;

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
	public const float WalkSpeed = 2.5f;
	public const float MoveSpeed = 10f;
	public const float RunSpeed = 12f;
	public const float FireRate = 1.2f;
	public const float TurnStep = 0.02f;
	public const float FireAngle = 60 / 2;
	public const float MaxHealth = 100;
	public const float ResightRate = 0.25f;
	public LayerMask shootableLayer;
	public bool controllable = true;

	// Storage variables
	public GameObject allyGroup;
	private BotAI[] allies;
	public GameObject playerGroup;
	private Player[] players;
	NavMeshAgent navMeshAgent;
	[HideInInspector]
	public Player currentTarget = null;

	// Bot stats
	[HideInInspector]
	public State state = State.AllClear;
	private State prevState = State.AllClear;
	TargetComponent targetVisible = TargetComponent.None;
	float health = MaxHealth;
	[HideInInspector]
	public float reloadProg = FireRate;
	bool isDead = false;
	float resightProg = ResightRate;
	
	public Vector3 lastSighted;

	// Inputs
	public PoolManager projectilePool;
	public PoolManager impactPool;
	public PoolManager wreckagePool;
	private PoolManager pool;
	public GameObject waypoint;
	public GoliathAvatar goliath;
	public ParticleEmitter flash;
	public Transform bulletSpawn;

	[HideInInspector]
	public int remoteId = -1;
	[HideInInspector]
	public Pooled pooled;

	private bool navigating = false;
	private bool waypointQueued = false;

	//Audio sources
	public AudioSource yes1Sound;
	public AudioSource yes2Sound;
	public AudioSource fireSound;
	public AudioSource deathSound;

	private Vector3 referencePosition;

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

		if (!pooled){
			pooled = GetComponent<Pooled>();
		}

		yes1Sound.pitch = 1 + Random.Range(-0.2f, 0.2f);
		yes2Sound.pitch = 1 + Random.Range(-0.2f, 0.2f);
		fireSound.pitch = 1 + Random.Range(-0.2f, 0.2f);
		if (pool.splitListener){
			pool.splitListener.StoreAudioSource(yes1Sound);
			pool.splitListener.StoreAudioSource(yes2Sound);
			pool.splitListener.StoreAudioSource(fireSound);
			pool.splitListener.StoreAudioSource(deathSound);
		}

		referencePosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		// Living behaviour
		if (!isDead){
			if (navigating){

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
					float angle = Vector3.Angle(transform.forward, diffVec);

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
					Idle();
				}
				else if (state == State.Traveling){
					if (HasAgentArrivedAtDest()){
						state = State.AllClear;
					}
				}
				else if (state == State.Searching){
					// If is newly searching
					if (prevState != state){
						currentTarget = null;
						navMeshAgent.speed = MoveSpeed;
					}

					Search();
				}
				else if (state == State.Approaching){
					// If newly approaching
					if (prevState != state){
						//alertTime = AlertDuration;
						navMeshAgent.speed = MoveSpeed;
					}

					navMeshAgent.Resume();
					navMeshAgent.destination = lastSighted;
				}
				else if (state == State.VeryClose){
					alertTime = AlertDuration;

					FaceTarget(currentTarget.gameObject);
					navMeshAgent.velocity = Vector3.zero;
				}

				if (state > State.Searching){
					//if (IsSightUnobstructed(currentTarget.gameObject)){
						navMeshAgent.stoppingDistance = 10;
					/*}
					else{
						navMeshAgent.stoppingDistance = 0;
					}*/
				}
				else{
					navMeshAgent.stoppingDistance = 0;
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
		}
		else{
			wreckagePool.Retrieve(transform.position, transform.rotation);if (pool.splitListener){
				pool.splitListener.PlayAudioSource(deathSound, transform.position);
			}
			else{
				deathSound.Play();
			}
			pool.Deactivate(gameObject);

			pooled.scavNetworker.photonView.RPC("DestroyMinion", PhotonTargets.All, remoteId);
			remoteId = -1;
		}
	}

	void OnEnable(){
		GetComponent<Rigidbody>().isKinematic = false;
		GetComponent<Rigidbody>().velocity = Vector3.zero;

		health = MaxHealth;

		alertTime = 0;
		searchTime = 0;
		searchDelay = 0;

		state = State.AllClear;
		isDead = false;
		reloadProg = FireRate;
		resightProg = ResightRate;
	}

	void OnCollisionEnter(Collision collision){
		if (!navigating && collision.gameObject.tag == "Terrain"){
			navigating = true;
			referencePosition = transform.position;
			if (navMeshAgent){
				navMeshAgent.enabled = true;
			}
			GetComponent<Rigidbody>().isKinematic = true;

			if (waypointQueued){
				SetNewWaypoint();
			}
		}
	}

	public void SetNewWaypoint(){
		if (controllable){
			if (navMeshAgent.enabled){
				state = State.Traveling;
				navMeshAgent.speed = RunSpeed;
				navMeshAgent.Resume();
				navMeshAgent.destination = GetRandPos(WaypointRad, waypoint.transform.position);
				if(Random.Range(0f, 1f) < 0.5){
					if (pool.splitListener){
						pool.splitListener.PlayAudioSource(yes1Sound, transform.position);
					}
					else{
						yes1Sound.Play();
					}
				}
				else {
					if (pool.splitListener){
						pool.splitListener.PlayAudioSource(yes2Sound, transform.position);
					}
					else{
						yes2Sound.Play();
					}
				}
			}
			else{
				waypointQueued = true;
			}
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
				else if (distance < ThreshSearch && (state == State.Searching || state == State.Traveling)){
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
		Vector3 headVec = diffVec + new Vector3 (0, subject.transform.GetComponent<Collider>().bounds.extents.y - 0.1f, 0);
		Vector3 feetVec = diffVec - new Vector3 (0, subject.transform.GetComponent<Collider>().bounds.extents.y - 0.1f, 0);

		// Cast ray to determine obstructions in sight
		RaycastHit rayHit;
		
		// Check head
		if (Physics.Raycast(transform.position, headVec, out rayHit)){
			// If hit target, no obstruction
			if (rayHit.collider.tag == subject.tag){	
				return true;
			}
		}
		else if (Physics.Raycast(transform.position, diffVec, out rayHit)){
			// If hit target, no obstruction
			if (rayHit.collider.tag == subject.tag){
				return true;
			}
		}
		// Check feet
		else if (Physics.Raycast(transform.position, feetVec, out rayHit)){	
			// If hit target, no obstruction
			if (rayHit.collider.tag == subject.tag){
				return true;
			}
		}
		
		return false;
	}

	// Check whether subject is in sight
	bool IsInSight(GameObject subject){
		Vector3 diffVec = subject.transform.position - transform.position;
		Vector3 headVec = diffVec + new Vector3 (0, subject.transform.GetComponent<Collider>().bounds.extents.y - 0.1f, 0);
		Vector3 feetVec = diffVec - new Vector3 (0, subject.transform.GetComponent<Collider>().bounds.extents.y - 0.1f, 0);
		float angle = Vector3.Angle(transform.forward, diffVec);

		// Check if within FoV
		if (angle <= ViewAngle){
			
			// Cast ray to determine obstructions in sight
			RaycastHit rayHit;
			
			// Check head
			if (Physics.Raycast(transform.position, headVec, out rayHit)){
				// If hit target, no obstruction
				if (rayHit.collider.tag == subject.tag && rayHit.distance < ThreshApproach){	
					return true;
				}
			}
			else if (Physics.Raycast(transform.position, diffVec, out rayHit)){
				// If hit target, no obstruction
				if (rayHit.collider.tag == subject.tag && rayHit.distance < ThreshApproach){
					return true;
				}
			}
			// Check feet
			else if (Physics.Raycast(transform.position, feetVec, out rayHit)){	
				// If hit target, no obstruction
				if (rayHit.collider.tag == subject.tag && rayHit.distance < ThreshApproach){
					return true;
				}
			}
		}

		return false;
	}

	// Fires bullet predictively at target based on velocity
	void FireAtPredictively(GameObject target){
		Vector3 bulletGenPos = bulletSpawn.position;
		flash.Emit();
		GameObject bullet = projectilePool.Retrieve(bulletGenPos);
		MinionBullet bulletScript = bullet.GetComponent<MinionBullet>();
		float timeDelay = 0;

		float distanceSquare = Vector3.SqrMagnitude(target.transform.position - bulletGenPos);
		float targetVelSquare = target.GetComponent<Rigidbody>().velocity.sqrMagnitude;
		float bulletVelSquare = bulletScript.speed * bulletScript.speed;

		timeDelay = Mathf.Sqrt(distanceSquare / (Mathf.Abs(bulletVelSquare - targetVelSquare)));
		Vector3 direction = new Vector3(target.transform.position.x + target.GetComponent<Rigidbody>().velocity.x * timeDelay, target.transform.position.y + target.GetComponent<Collider>().bounds.extents.y + target.GetComponent<Rigidbody>().velocity.y * timeDelay, target.transform.position.z + target.GetComponent<Rigidbody>().velocity.z * timeDelay) - bulletGenPos;
	
		bullet.transform.forward = direction;
		bulletScript.shootableLayer = shootableLayer;
		bulletScript.bulletMarkPool = impactPool;

		pooled.scavNetworker.photonView.RPC ("CreateMinionBullet", PhotonTargets.All, bulletGenPos, direction, remoteId);
		if (pool.splitListener){
			pool.splitListener.PlayAudioSource(fireSound, transform.position);
		}
		else{
			fireSound.Play();
		}
	}

	// Fires bullet in direction provided
	void fireInDirection(Transform target){
		Vector3 bulletGenPos = bulletSpawn.position;
		Vector3 direction = new Vector3(target.position.x, target.position.y + target.GetComponent<Collider>().bounds.extents.y, target.position.z) - bulletGenPos;

		GameObject bullet = projectilePool.Retrieve(bulletGenPos);
		bullet.transform.forward = direction;
		MinionBullet bulletScript = bullet.GetComponent<MinionBullet>();
		bulletScript.shootableLayer = shootableLayer;
		bulletScript.bulletMarkPool = impactPool;
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
			navMeshAgent.speed = WalkSpeed;
			idleState = IdleMoving;
			actionTime = 0;
			navMeshAgent.Resume();
			navMeshAgent.destination = GetRandPos(IdleWalkRad, referencePosition);
			if (navMeshAgent.remainingDistance > ThreshClose){
				navMeshAgent.speed = RunSpeed;
			}
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
			navMeshAgent.Resume();
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
		return (Vector3.Distance(transform.position, navMeshAgent.destination) < 1f && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete);
		//float remainder = navMeshAgent.remainingDistance;
		//return (remainder != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0);
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
		if (!isDead){
			health -= damage;

			// Schedule bot for death
			if (health <= 0){
				isDead = true;

				if (controllable){
					// Schedule for respawn
					controllable = false;
					goliath.AddRespawningMinion();
				}

				// Resetting some attributes
				navigating = false;
				navMeshAgent.enabled = false;
				navMeshAgent.speed = WalkSpeed;
			}

			if (playerSource){
				currentTarget = playerSource;
				lastSighted = currentTarget.transform.position;
				alertTime = AlertDuration;
			}
		}
	}
}
