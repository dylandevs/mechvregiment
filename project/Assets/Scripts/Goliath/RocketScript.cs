using UnityEngine;
using System.Collections;

public class RocketScript : MonoBehaviour {
	//misse target area
	public Vector3 target;

	//stuff fr detonation
	public float damage = 50f;  //damage at center
	public float explosionRadius = 10f;

	//public GameObject missleRemains;
	public ParticleEmitter smokeParticles;

	//the smoke and the misslemesh
	public GameObject missleMesh;

	//speeds for rockets
	private float speed = 100f;
	private Vector3 mustHit;

	//audio for explosion
	public AudioSource explosionEmitter;

	public LayerMask mask;

	PoolManager pool;
	public PoolManager explosionPool;

	public float destroyDelay;
	float timer = 0;
	float turnTimer;
	bool hitGround;

	public bool isAvatar = false;

	void Start(){
		pool = transform.parent.GetComponent<PoolManager>();
	}

	// Update is called once per frame
	void Update(){
		timer += Time.deltaTime * 5;
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
				hitGround = true;
				Vector3 explosionPlace = explosionLocation.point;
				//spawns the Boom
				GameObject explosion = explosionPool.Retrieve(explosionPlace);
				Detonate(explosionPlace);
			}
		}

	}// end of fixed update

	void OnEnable(){
		smokeParticles.emit = true;
		turnTimer = 1;
		hitGround = false;
		missleMesh.SetActive(true);
	}

	public void SetTarget(Vector3 newTarget){
		mustHit = newTarget + new Vector3 (Random.Range (-35F, 35F), 0, Random.Range (-35F, 35F));
	}

	void Detonate(Vector3 pos)
	{
		//stop the audio woosh and play the explosion
		this.GetComponent<AudioSource>().Stop();
		explosionEmitter.Play();

		if (!isAvatar){

			//hurts whats near the boom depending on a overlap sphere function
			Collider[] colliders = Physics.OverlapSphere (pos, explosionRadius,mask);
			foreach (Collider c in colliders) 
			{
				if(c.gameObject.collider.tag == "Player"){

					float dist = Vector3.Distance(transform.position, c.transform.position);
					float damageRatio = 1f - (dist / explosionRadius);
					float damageAmnt = damage * damageRatio;
					// a bit iffy on this direction calculation
					Vector3 direction = transform.position - c.transform.position;
					
					GameObject hitPlayer = c.collider.gameObject;
					PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
					hitPlayerScript.DamagePlayer(damageAmnt,gameObject.transform.up);
				}
			}
		}
	}
}
