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
	public GameObject dashingEffect;

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

	public GameObject carryEmitters;

	private bool isDisabled = false;
	private Vector3 armLOrigPos;
	private Vector3 armROrigPos;

	public AudioSource minigunSound;
	public AudioSource cannonSound;

	// Use this for initialization
	void Start () {
		armLOrigPos = shoulderLJoint.localPosition;
		armROrigPos = shoulderRJoint.localPosition;
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

		if (pool.splitListener){
			pool.splitListener.StoreAudioSource(minigunSound);
			pool.splitListener.StoreAudioSource(cannonSound);
		}
	}

	private void InterpolateTransform(){
		if (syncProg < syncDelay){
			syncProg += Time.deltaTime;
	
			botJoint.position = Vector3.Lerp (lastSyncPosBot, nextSyncPosBot, syncProg * invSyncDelay);
			botJoint.rotation = Quaternion.Lerp (lastSyncRotBot, nextSyncRotBot, syncProg * invSyncDelay);

			if (!isDisabled){
				topJoint.position = Vector3.Lerp (lastSyncPosTop, nextSyncPosTop, syncProg * invSyncDelay);
				topJoint.rotation = Quaternion.Lerp (lastSyncRotTop, nextSyncRotTop, syncProg * invSyncDelay);

				spineJoint.rotation = Quaternion.Lerp (lastSyncRotSpine, nextSyncRotSpine, syncProg * invSyncDelay);
				shoulderRJoint.rotation = Quaternion.Lerp (lastSyncRotShouldR, nextSyncRotShouldR, syncProg * invSyncDelay);
				shoulderLJoint.rotation = Quaternion.Lerp (lastSyncRotShouldL, nextSyncRotShouldL, syncProg * invSyncDelay);
			}
		}
	}
	
	public void SetNextTargetTransform(Vector3 nextTopPos, Quaternion nextTopRot, Vector3 nextBotPos, Quaternion nextBotRot, Vector3 currBotVelocity, Quaternion nextSpineRot, Quaternion nextShouldRRot, Quaternion nextShouldLRot){
		syncProg = 0;
		
		syncDelay = Time.time - lastSync;
		invSyncDelay = 1 / syncDelay;
		lastSync = Time.time;

		lastSyncPosBot = botJoint.position;
		nextSyncPosBot = nextBotPos + currBotVelocity * syncDelay;

		if (!isDisabled){
			lastSyncPosTop = topJoint.position;
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
		}

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

	public void StartDash(){
		isDashing = true;
		dashingEffect.SetActive (true);
	}

	public void EndDash(){
		isDashing = false;
		dashingEffect.SetActive (false);
	}

	public void SetDisabled(){
		SetLayerRecursively(topJoint.gameObject, LayerMask.NameToLayer("Non-Interactive Physics"));
		topJoint.rigidbody.isKinematic = false;
		topJoint.rigidbody.WakeUp();
		shoulderLJoint.rigidbody.isKinematic = false;
		shoulderLJoint.rigidbody.WakeUp();
		shoulderRJoint.rigidbody.isKinematic = false;
		shoulderRJoint.rigidbody.WakeUp();
		isDisabled = true;
	}

	public void SetEnabled(){
		SetLayerRecursively(topJoint.gameObject, LayerMask.NameToLayer("Goliath"));
		topJoint.rigidbody.velocity = Vector3.zero;
		topJoint.rigidbody.angularVelocity = Vector3.zero;
		topJoint.rigidbody.isKinematic = true;

		shoulderLJoint.rigidbody.velocity = Vector3.zero;
		shoulderLJoint.rigidbody.angularVelocity = Vector3.zero;
		shoulderLJoint.localPosition = armLOrigPos;
		shoulderLJoint.localRotation = Quaternion.identity;
		shoulderLJoint.rigidbody.isKinematic = true;

		shoulderRJoint.rigidbody.velocity = Vector3.zero;
		shoulderRJoint.rigidbody.angularVelocity = Vector3.zero;
		shoulderRJoint.localPosition = armROrigPos;
		shoulderRJoint.localRotation = Quaternion.identity;
		shoulderRJoint.rigidbody.isKinematic = true;

		isDisabled = false;
	}

	void SetLayerRecursively(GameObject baseObj, int layer){
		baseObj.layer = layer;
		
		foreach (Transform child in baseObj.transform){
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	public void FireMinigun(){
		if (pool.splitListener){
			pool.splitListener.PlayAudioSource(minigunSound, minigunSound.gameObject.transform.position);
		}
		else{
			minigunSound.Play();
		}
	}
	public void FireCannon(){
		if (pool.splitListener){
			pool.splitListener.PlayAudioSource(cannonSound, cannonSound.gameObject.transform.position);
		}
		else{
			cannonSound.Play();
		}
	}
}
