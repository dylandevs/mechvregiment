using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	//misse target area
	public Vector3 target;

	//stuff fr detonation
	public float damage = 75f;  //damage at center
	public float explosionRadius = 15f;

	//public GameObject missleRemains;
	public ParticleEmitter smokeParticles;

	//the smoke and the misslemesh
	public GameObject missleMesh;

	//speeds for rockets
	private float speed = 100f;
	private Vector3 mustHit;

	//audio for explosion
	public AudioSource explosionEmitter;
	private AudioSource meteorEmitter;
	private float initialPitch = 0.9f;

	public LayerMask mask;

	PoolManager pool;
	public PoolManager explosionPool;

	public float destroyDelay;
	float timer = 0;
	float turnTimer;
	bool hitGround;
	bool hitAPlayer;
	public bool isAvatar = false;

	void Start(){
		meteorEmitter = transform.GetComponent<AudioSource>();
		meteorEmitter.pitch = initialPitch;
		meteorEmitter.volume = 0f;
		pool = transform.parent.GetComponent<PoolManager>();
		if (pool.splitListener){
			pool.splitListener.StoreAudioSource(this.GetComponent<AudioSource>());
		}
	}

	// Update is called once per frame
	void Update(){
		timer += Time.deltaTime * 5;
		if (turnTimer <= 0 && meteorEmitter.isPlaying){
			if(initialPitch > 0){
				//initialPitch -= Time.deltaTime/10;
				meteorEmitter.pitch = initialPitch;
			}
			if(meteorEmitter.volume < 1f){
				meteorEmitter.volume += Time.deltaTime/10;
			}
			else {
				meteorEmitter.volume = 1f;
			}
			if (pool.splitListener){
				pool.splitListener.StoreAudioSource(meteorEmitter);
			}
		}
	}

	void FixedUpdate () {

		if(turnTimer > 0){
			turnTimer -= Time.deltaTime;
			Vector3 targetDir = mustHit - transform.position;
			float step = speed * Time.deltaTime;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
			transform.rotation = Quaternion.LookRotation(newDir);
		}

		if(hitGround == false){
			transform.Translate(Vector3.forward * speed * Time.deltaTime);
		}

		if(hitGround == false){
			//fire a raycast ahead to ensure you wont miss and go through a collider
			Ray ray = new Ray(transform.position,transform.forward);
			RaycastHit explosionLocation;

			if (Physics.Raycast (ray,out explosionLocation, 2f)) 
			{
				missleMesh.SetActive(false);
				smokeParticles.emit = false;
				hitGround = true;
				Vector3 explosionPlace = explosionLocation.point;
				//spawns the Boom
				GameObject explosion = explosionPool.Retrieve(explosionPlace);
				Detonate(explosionPlace);
			}
		}
		else if (smokeParticles.GetComponent<ParticleEmitter>().particleCount == 0) {
			pool.Deactivate(gameObject);
		}

	}// end of fixed update

	void OnEnable(){
		smokeParticles.emit = true;
		turnTimer = 1;
		hitGround = false;
		missleMesh.SetActive(true);
		hitAPlayer = false;
		//meteorEmitter.enabled = true;
	}

	public void SetTarget(Vector3 newTarget){
		mustHit = newTarget + new Vector3 (Random.Range (-35F, 35F), 0, Random.Range (-35F, 35F));
	}

	void Detonate(Vector3 pos)
	{
		//stop the audio woosh and play the explosion
		meteorEmitter.pitch = 0.9f;
		meteorEmitter.volume = 0;
		initialPitch = 0.9f;
		meteorEmitter.Stop();
		meteorEmitter.enabled = false;
		explosionEmitter.Play();
		if (pool.splitListener){
			pool.splitListener.StoreAudioSource(explosionEmitter);
		}

		if (!isAvatar){

			//hurts whats near the boom depending on a overlap sphere function
			Collider[] colliders = Physics.OverlapSphere (pos, explosionRadius,mask);
			foreach (Collider c in colliders) 
			{
				if(c.gameObject.GetComponent<Collider>().tag == "Player" && hitAPlayer == false){
					hitAPlayer = true;
					float dist = Vector3.Distance(transform.position, c.transform.position);
					float damageRatio = 1f - (dist / explosionRadius);
					float damageAmnt = damage * damageRatio;
					// a bit iffy on this direction calculation
					Vector3 direction = transform.position - c.transform.position;
					
					GameObject hitPlayer = c.GetComponent<Collider>().gameObject;
					PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
					hitPlayerScript.DamagePlayer(damageAmnt,gameObject.transform.up);
				}

				if(c.gameObject.GetComponent<Collider>().tag == "Enemy"){
					float dist = Vector3.Distance(transform.position, c.transform.position);
					float damageRatio = 1f - (dist / explosionRadius);
					float damageAmnt = damage * damageRatio;

					GameObject hitMinion = c.GetComponent<Collider>().gameObject;
					MinionAvatar minionScript = hitMinion.GetComponent<MinionAvatar>();
					minionScript.Damage(damageAmnt);
				}
			}
		}
	}

}
