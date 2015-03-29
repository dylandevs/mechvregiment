using UnityEngine;
using System.Collections;

public class GoliathAvatar : MonoBehaviour {

	public PlayerNetSend networkManager;
	public Animator anim;

	public Transform topJoint;
	public Transform botJoint;
	public Transform spineJoint;
	public Transform shoulderRJoint;
	public Transform shoulderLJoint;

	public float DashDamage = 80;
	public float DashForce = 1000;
	public float MinionSpawnRate = 5;
	private float minionSpawnProg = 0;
	public Transform minionSpawn;
	public PoolManager minionManager;
	private int minionsToSpawn = 0;

	private float syncProg = 0;
	private float syncDelay = 0;
	private float invSyncDelay = 1;
	private float lastSync = 0;
	
	private Vector3 lastSyncPosTop;
	private Vector3 nextSyncPosTop;
	private Quaternion lastSyncRotTop;
	private Quaternion nextSyncRotTop;
	
	private Vector3 lastSyncPosBot;
	private Vector3 nextSyncPosBot;
	private Quaternion lastSyncRotBot;
	private Quaternion nextSyncRotBot;

	private Quaternion lastSyncRotSpine;
	private Quaternion nextSyncRotSpine;

	private Quaternion lastSyncRotShouldR;
	private Quaternion nextSyncRotShouldR;

	private Quaternion lastSyncRotShouldL;
	private Quaternion nextSyncRotShouldL;

	private Vector3 cachedVelocity;

	public bool isDashing = false;

	private int fwdHash = Animator.StringToHash("FwdSpeed");
	private int rgtHash = Animator.StringToHash("RgtSpeed");

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		InterpolateTransform();

		// Minion respawning
		if (minionSpawnProg > 0){
			minionSpawnProg -= Time.deltaTime;

			if (minionSpawnProg <= 0){
				GameObject newMinion = minionManager.Retrieve(minionSpawn.transform.position);
				BotAI minionScript = newMinion.GetComponent<BotAI>();
				minionScript.controllable = true;

				minionScript.SetNewWaypoint();
				minionsToSpawn--;

				if (minionsToSpawn > 0){
					minionSpawnProg = MinionSpawnRate;
				}

				networkManager.photonView.RPC ("SpawnNetworkedMinion", PhotonTargets.All, minionScript.pooled.index, minionSpawn.transform.position);
			}
		}

		// Calculate directional speeds
		Quaternion revFacingRot = Quaternion.FromToRotation(botJoint.forward, Vector3.forward);
		Vector3 rotatedVelocity = revFacingRot * cachedVelocity;
		
		float forwardSpeed = rotatedVelocity.z;
		float rightSpeed = rotatedVelocity.x;

		anim.SetFloat (fwdHash, forwardSpeed);
		anim.SetFloat (rgtHash, rightSpeed);
	}

	private void InterpolateTransform(){
		if (syncProg < syncDelay){
			syncProg += Time.deltaTime;

			topJoint.position = Vector3.Lerp (lastSyncPosTop, nextSyncPosTop, syncProg * invSyncDelay);
			topJoint.rotation = Quaternion.Lerp (lastSyncRotTop, nextSyncRotTop, syncProg * invSyncDelay);

			botJoint.position = Vector3.Lerp (lastSyncPosBot, nextSyncPosBot, syncProg * invSyncDelay);
			botJoint.rotation = Quaternion.Lerp (lastSyncRotBot, nextSyncRotBot, syncProg * invSyncDelay);

			spineJoint.rotation = Quaternion.Lerp (lastSyncRotSpine, nextSyncRotSpine, syncProg * invSyncDelay);
			shoulderRJoint.rotation = Quaternion.Lerp (lastSyncRotShouldR, nextSyncRotShouldR, syncProg * invSyncDelay);
			shoulderLJoint.rotation = Quaternion.Lerp (lastSyncRotShouldL, nextSyncRotShouldL, syncProg * invSyncDelay);
		}
	}
	
	public void SetNextTargetTransform(Vector3 nextTopPos, Quaternion nextTopRot, Vector3 nextBotPos, Quaternion nextBotRot, Vector3 currBotVelocity, Quaternion nextSpineRot, Quaternion nextShouldRRot, Quaternion nextShouldLRot){
		syncProg = 0;
		
		syncDelay = Time.time - lastSync;
		invSyncDelay = 1 / syncDelay;
		lastSync = Time.time;

		lastSyncPosBot = botJoint.position;
		lastSyncPosTop = topJoint.position;
		nextSyncPosBot = nextBotPos + currBotVelocity * syncDelay;
		nextSyncPosTop = nextTopPos + currBotVelocity * syncDelay;
		
		lastSyncRotBot = botJoint.rotation;
		lastSyncRotTop = topJoint.rotation;
		lastSyncRotSpine = spineJoint.rotation;
		lastSyncRotShouldR = shoulderRJoint.rotation;
		lastSyncRotShouldL = shoulderLJoint.rotation;

		nextSyncRotBot = nextBotRot;
		nextSyncRotTop = nextTopRot;
		nextSyncRotSpine = nextSpineRot;
		nextSyncRotShouldR = nextShouldRRot;
		nextSyncRotShouldL = nextShouldLRot;

		cachedVelocity = currBotVelocity;
	}

	public void Damage(float damage, Vector3 direction){
		networkManager.photonView.RPC ("DamageGoliath", PhotonTargets.All, damage, direction);
	}

	public void AddRespawningMinion(){
		if (minionSpawnProg <= 0){
			minionSpawnProg = MinionSpawnRate;
		}
		minionsToSpawn++;
	}
}
