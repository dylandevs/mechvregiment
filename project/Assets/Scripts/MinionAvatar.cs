using UnityEngine;
using System.Collections;

public class MinionAvatar : MonoBehaviour {

	public GoliathNetworking networkManager;
	public float index = 0;

	private float syncProg = 0;
	private float syncDelay = 0;
	private float invSyncDelay = 1;
	private float lastSync = 0;
	
	private Vector3 lastSyncPos;
	private Vector3 nextSyncPos;
	
	private Quaternion lastSyncRot;
	private Quaternion nextSyncRot;
	private Vector3 facing;
	
	// Use this for initialization
	void Start () {
		lastSyncPos = transform.position;
		nextSyncPos = transform.position;
		lastSyncRot = transform.rotation;
		nextSyncRot = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		InterpolateTransform ();
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

	public void Damage(float damage){
		networkManager.photonView.RPC ("ApplyMinionDamage", PhotonTargets.All, index, damage);
	}
}
