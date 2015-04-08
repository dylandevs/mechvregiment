using UnityEngine;
using System.Collections;

public class Mine : MonoBehaviour {

	public float Damage = 50;
	public float DetonationWait = 1.5f;
	public float ExplosionRadius = 5;
	private float invExplosionRadius = 1;
	public float DetectionRadius = 3;
	public LayerMask triggerableMask;
	public LayerMask damageableMask;

	PoolManager pool;
	public PoolManager explosionPool;

	private float countdownTimer = 0;
	private bool isFixed = false;

	[HideInInspector]
	public bool isDetonated = false;

	public Player playerSource;
	public bool isAvatar = false;
	public GoliathNetworking goliathNetworker;

	[HideInInspector]
	public Pooled pooled;

	[HideInInspector]
	public int remoteId = -1;
	[HideInInspector]
	private bool transmitPosition = false;

	//Audio for explosion and landing sound
	public AudioSource landingSound;
	public AudioSource explosionSound;

	// Use this for initializations
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
		invExplosionRadius = 1 / ExplosionRadius;
		pool.splitListener.StoreAudioSource(landingSound);
		pool.splitListener.StoreAudioSource(explosionSound);
	}
	
	// Update is called once per frame
	void Update () {
		if (transmitPosition && remoteId != -1 && !isAvatar){
			playerSource.networkManager.photonView.RPC("AffixMine", PhotonTargets.All, remoteId, transform.position);
			transmitPosition = false;
		}

		if (countdownTimer > 0){
			countdownTimer -= Time.deltaTime;

			if (countdownTimer <= 0){
				Detonate();
			}
		}
		else if (isFixed){
			if (!isAvatar){
				CheckProximity();
			}
		}
		else{
			RaycastHit rayHit;
			if (Physics.Raycast(transform.position, rigidbody.velocity.normalized, out rayHit, Mathf.Max(rigidbody.velocity.magnitude * Time.deltaTime, 0.16f))){
				if (rayHit.collider.tag == "Terrain"){
					AffixToTerrain(rayHit.point);
				}
			}
		}
	}

	public void Detonate(){

		isDetonated = true;

		explosionPool.Retrieve (transform.position);

		if (pool.splitListener){
			pool.splitListener.PlayAudioSource(explosionSound, transform.position);
		}
		else{
			explosionSound.Play();
		}

		if (!isAvatar){
			Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, damageableMask);

			foreach(Collider hitObject in colliders){
				float hitDistance = Vector3.Distance(hitObject.transform.position, transform.position);

				if (hitObject.tag == "Enemy"){
					BotAI enemyhit = hitObject.GetComponent<BotAI>();
					enemyhit.Damage(Damage * hitDistance * invExplosionRadius);
				}
				// Only hit player once
				else if (hitObject.tag == "Player" && hitObject.name == "jnt_Hip"){
					Vector3 direction = hitObject.transform.position - transform.position;

					PlayerDamager playerHit = hitObject.GetComponent<PlayerDamager>();
					playerHit.DamagePlayer(Damage * hitDistance * invExplosionRadius, direction);
				}
				else if (hitObject.tag == "Goliath" &&
				         (hitObject.name == "jnt_Root_Bottom" || hitObject.name == "jnt_L_Ball" || hitObject.name == "jnt_R_Ball")){
					Vector3 direction = hitObject.transform.position - transform.position;
					
					GoliathDamager damager = hitObject.GetComponent<GoliathDamager>();
					damager.DamageGoliath(Damage * hitDistance * invExplosionRadius, direction);
				}
				else if (hitObject.tag == "Mine"){
					Mine mine = hitObject.GetComponent<Mine>();
					if (!mine.isDetonated){
						mine.Detonate();
					}
				}
			}

			if (remoteId != -1){
				playerSource.networkManager.photonView.RPC("DetonateMine", PhotonTargets.All, remoteId);
			}
		}

		pool.Deactivate(gameObject);
	}

	void CheckProximity(){
		if (Physics.CheckSphere(transform.position, DetectionRadius, triggerableMask)){
			StartCountdown();
		}
	}

	void StartCountdown(){
		countdownTimer = DetonationWait;
	}

	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "Terrain" && !isFixed){
			AffixToTerrain(transform.position);
		}
	}

	void AffixToTerrain(Vector3 position){
		transform.position = position;
		rigidbody.isKinematic = true;
		isFixed = true;
		transmitPosition = true;
		if (pool.splitListener){
			pool.splitListener.PlayAudioSource(landingSound, transform.position);
		}
		else{
			landingSound.Play();
		}
	}

	void OnEnable(){
		rigidbody.isKinematic = false;
		isFixed = false;
		countdownTimer = 0;
		isDetonated = false;

		remoteId = -1;
		transmitPosition = false;
		if (!pooled){
			pooled = GetComponent<Pooled>();
		}
	}

	public void SetAffixedPosition(Vector3 position){
		transform.position = position;
		rigidbody.isKinematic = true;
		isFixed = true;
	}
}
