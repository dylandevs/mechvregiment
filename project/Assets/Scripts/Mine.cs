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

	// Use this for initialization
	void Start () {

		invExplosionRadius = 1 / ExplosionRadius;
	}
	
	// Update is called once per frame
	void Update () {
		if (countdownTimer > 0){
			countdownTimer -= Time.deltaTime;

			if (countdownTimer <= 0){
				Detonate();
			}
		}
	}

	public void Detonate(){
		Collider[] colliders = Physics.OverlapSphere(transform.position, ExplosionRadius, damageableMask);

		foreach(Collider hitObject in colliders){
			float hitDistance = Vector3.Distance(hitObject.transform.position, transform.position);

			if (hitObject.tag == "Enemy"){
				BotAI enemyhit = hitObject.GetComponent<BotAI>();
				enemyhit.Damage(Damage * hitDistance * invExplosionRadius);
			}
			else if (hitObject.tag == "Player"){
				Vector3 direction = transform.position - hitObject.transform.position;

				PlayerDamager playerHit = hitObject.GetComponent<PlayerDamager>();
				playerHit.DamagePlayer(Damage * hitDistance * invExplosionRadius, direction);
				print (playerHit.gameObject.name);
			}
			else if (hitObject.tag == "Goliath"){

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
}
