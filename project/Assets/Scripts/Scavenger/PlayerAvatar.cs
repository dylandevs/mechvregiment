using UnityEngine;
using System.Collections;

public class PlayerAvatar : MonoBehaviour {

	public GameObject NetworkManager;

	private float syncProg = 0;
	private float syncDelay = 0;
	private float invSyncDelay = 1;
	private float lastSync = 0;

	private Vector3 lastSyncPos;
	private Vector3 nextSyncPos;

	private Quaternion lastSyncRot;
	private Quaternion nextSyncRot;

	Vector3 facing;

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
			rigidbody.MovePosition (Vector3.Lerp (lastSyncPos, nextSyncPos, syncProg * invSyncDelay));
			rigidbody.MoveRotation (Quaternion.Lerp (lastSyncRot, nextSyncRot, syncProg * invSyncDelay));
		}
	}

	public void SetNextTargetTransform(Vector3 nextPos, Quaternion nextRot, Vector3 currVelocity){
		syncProg = 0;

		syncDelay = Time.time - lastSync;
		lastSync = Time.time;

		lastSyncPos = rigidbody.position;
		nextSyncPos = nextPos + currVelocity * syncDelay;

		lastSyncRot = nextSyncRot;
		nextSyncRot = nextRot;
	}

	public void SetAim(bool ADS){
		if(ADS){
			print("aiming");
		}
		else {
			print("not aiming");
		}
	}
}
