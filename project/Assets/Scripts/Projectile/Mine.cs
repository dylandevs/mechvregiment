﻿using UnityEngine;
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

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
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
		else if (isFixed){
			CheckProximity();
		}
		else{
			RaycastHit rayHit;
			if (Physics.Raycast(transform.position, Vector3.down, out rayHit, Mathf.Max(rigidbody.velocity.magnitude * Time.deltaTime, 0.16f))){
				if (rayHit.collider.tag == "Terrain"){
					AffixToTerrain(rayHit.point);
				}
			}
		}
	}

	public void Detonate(){

		isDetonated = true;

		explosionPool.Retrieve (transform.position);
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
			else if (hitObject.tag == "Goliath"){

			}
			else if (hitObject.tag == "Mine"){
				Mine mine = hitObject.GetComponent<Mine>();
				if (!mine.isDetonated){
					mine.Detonate();
				}
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
		//rigidbody.velocity = Vector3.zero;
		isFixed = true;
	}

	void OnEnable(){
		rigidbody.isKinematic = false;
		isFixed = false;
		countdownTimer = 0;
		isDetonated = false;
	}
}
