using UnityEngine;
using System.Collections;

public class cannonShot : MonoBehaviour {

	public Vector3 constantSpeed;
	public float explosionRadius = 10f;

	public PoolManager plasmaExplodePool;
	public PoolManager pool;

	public ParticleEmitter explode1;
	public ParticleEmitter explode2;

	public Vector3 explosionLocation;
	public Vector3 remainsLocation;

	public bool isAvatar = false;

	public GameObject cannonHit;
	float timer;
	float speed = 30;
	float waitOutTimer;
	float damage = 40;
	float damageDirect = 75;
	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {
		// turn off object after a certain amount of time
		if(timer >=0){
			timer -= Time.deltaTime;
		}
		if (timer <= 0f) {
			timer = 50;
			explode1.emit = false;
			explode2.emit = false;
			waitOutTimer = 4;
		}

		transform.Translate(Vector3.forward * speed * Time.deltaTime);

		Ray ray = new Ray(transform.position,transform.forward);
		RaycastHit hit;

		explosionLocation = transform.position;
		//hitRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
		//remainsLocation = hit.point + hit.normal;

		//not decreasing when things are not hit
		if(waitOutTimer > 0){
			waitOutTimer -= Time.deltaTime;
			if(waitOutTimer <= 0){
				pool.Deactivate(gameObject);
			}
		}

		if (Physics.Raycast (ray,out hit, 35 * Time.deltaTime)) 
		{
			if(hit.collider.tag == "Player"){
				GameObject hitPlayer = hit.collider.gameObject;
				//cannonHit.SetActive(true);
				if (hitPlayer){
					PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();
					if (hitPlayerScript){
						hitPlayerScript.DamagePlayer(damageDirect,gameObject.transform.forward);
					}
					else{
					}
				}
				else{
				}
			}
			else{
				GameObject plasmaExplosion = plasmaExplodePool.Retrieve(hit.point);

				explode1.emit = false;
				explode2.emit = false;
				waitOutTimer = 4;

				if (!isAvatar){
					//hurts whats near the boom depending on a overlap sphere function
					Collider[] colliders = Physics.OverlapSphere (transform.position, explosionRadius);
					foreach (Collider c in colliders) 
					{
						if(c.gameObject.collider.tag == "Player"){
							//cannonHit.SetActive(true);
							float dist = Vector3.Distance(transform.position, c.transform.position);
							float damageRatio = 1f - (dist / explosionRadius);
							float damageAmnt = damage * damageRatio;
							// a bit iffy on this direction calculation
							Vector3 direction = transform.position - c.transform.position;

							GameObject hitPlayer = c.collider.gameObject;
							if (hitPlayer){
								PlayerAvatarDamager hitPlayerScript = hitPlayer.GetComponent<PlayerAvatarDamager>();

								if (hitPlayerScript){
									hitPlayerScript.DamagePlayer(damageAmnt,gameObject.transform.forward);
								}
								else{}
							}
							else{}
						}
						if(c.gameObject.collider.tag == "Enemy"){
							float dist = Vector3.Distance(transform.position, c.transform.position);
							float damageRatio = 1f - (dist / explosionRadius);
							float damageAmnt = damage * damageRatio;
							
							GameObject hitMinion = c.collider.gameObject;
							MinionAvatar minionScript = hitMinion.GetComponent<MinionAvatar>();
							minionScript.Damage(damageAmnt);
						}
					}
				}
			}
		}
	}

	void OnEnable(){
		waitOutTimer = 0;
		timer = 5;
		explode1.emit = true;
		explode2.emit = true;
	}

	void doDamageCannon(){
		//do damgy thingys
	}
}
