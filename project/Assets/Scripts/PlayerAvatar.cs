﻿using UnityEngine;
using System.Collections;

public class PlayerAvatar : MonoBehaviour {

	public GoliathNetworking networkManager;
	public int PlayerNum = 1;

	private float syncProg = 0;
	private float syncDelay = 0;
	private float invSyncDelay = 1;
	private float lastSync = 0;

	private Vector3 lastSyncPos;
	private Vector3 nextSyncPos;

	private Quaternion lastSyncRot;
	private Quaternion nextSyncRot;

	// Input
	public Animator anim;
	public GameObject shotCollider;

	// Cached values
	private int fwdSpeedHash = Animator.StringToHash("FwdSpeed");
	private int rgtSpeedHash = Animator.StringToHash("RgtSpeed");
	private int speedHash = Animator.StringToHash("Speed");
	private int fireHash = Animator.StringToHash("Firing");
	private int sprintHash = Animator.StringToHash("Sprinting");
	private int adsHash = Animator.StringToHash("Aiming");
	private int jumpHash = Animator.StringToHash("Jump");
	private int crouchHash = Animator.StringToHash ("Crouching");
	private int weaponHash = Animator.StringToHash ("WeaponNum");
	private int changeWeapHash = Animator.StringToHash ("ChangeWeapon");
	private int resetHash = Animator.StringToHash("Reset");
	private int fwdDeadHash = Animator.StringToHash("DieFwd");
	private int bckDeadHash = Animator.StringToHash("DieBck");
	private int reloadHash = Animator.StringToHash("Reload");
	
	// Use this for initialization
	void Start () {
		lastSyncPos = transform.position;
		nextSyncPos = transform.position;
		lastSyncRot = transform.rotation;
		nextSyncRot = transform.rotation;

		// Set all children to Player tag
		SetTagRecursively(gameObject, "Player");
		SetLayerRecursively(shotCollider, LayerMask.NameToLayer("PlayerCollider1"));
	}
	
	// Update is called once per frame
	void Update () {
		InterpolateTransform ();
	}

	void SetTagRecursively(GameObject baseObj, string tag){
		baseObj.tag = tag;
		
		foreach (Transform child in baseObj.transform){
			SetTagRecursively(child.gameObject, tag);
		}
	}

	void SetLayerRecursively(GameObject baseObj, int layer){
		baseObj.layer = layer;
		
		foreach (Transform child in baseObj.transform){
			SetLayerRecursively(child.gameObject, layer);
		}
	}

	private void InterpolateTransform(){
		if (syncProg < syncDelay){
			syncProg += Time.deltaTime;
			transform.position = Vector3.Lerp (lastSyncPos, nextSyncPos, syncProg * invSyncDelay);
			transform.rotation = Quaternion.Lerp (lastSyncRot, nextSyncRot, syncProg * invSyncDelay);
		}
	}

	public void SetNextTargetTransform(Vector3 nextPos, Quaternion nextRot, Vector3 currVelocity){
		syncProg = 0;

		syncDelay = Time.time - lastSync;
		invSyncDelay = 1 / syncDelay;
		lastSync = Time.time;

		lastSyncPos = transform.position;
		nextSyncPos = nextPos + currVelocity * syncDelay;

		lastSyncRot = transform.rotation;
		nextSyncRot = nextRot;
	}

	public void UpdateAnimValues(float fwdSpeed, float rgtSpeed, float speed, bool crouching, bool sprinting, bool ads, bool firing){
		anim.SetFloat(fwdSpeedHash, fwdSpeed);
		anim.SetFloat(rgtSpeedHash, rgtSpeed);
		anim.SetFloat(speedHash, speed);
		anim.SetBool(crouchHash, crouching);
		anim.SetBool(sprintHash, sprinting);
		anim.SetBool(adsHash, ads);
		anim.SetBool(fireHash, firing);
	}

	public void CycleNewWeapon(int newWeapon){
		anim.SetTrigger(changeWeapHash);
		anim.SetInteger(weaponHash, newWeapon);
	}

	public void Damage(float damage, Vector3 direction){
		networkManager.photonView.RPC ("ApplyPlayerDamage", PhotonTargets.All, PlayerNum, damage, direction);
	}

	public void TriggerJump(){
		anim.SetTrigger(jumpHash);
	}

	public void DeathAnim(bool forward){
		// Disable firing layer
		anim.SetLayerWeight(1, 0);
		
		if (forward){
			anim.SetTrigger(fwdDeadHash);
		}
		else{
			anim.SetTrigger(bckDeadHash);
		}
	}

	public void TriggerRespawn(){
		// Enable firing layer
		anim.SetLayerWeight(1, 1);
		anim.SetTrigger(resetHash);
	}

	public void TriggerReload(){
		anim.SetTrigger(reloadHash);
	}

	public void TriggerFire(){
		anim.SetBool(fireHash, true);
	}
}