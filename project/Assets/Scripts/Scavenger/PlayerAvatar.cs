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
	//private Vector3 syncVelocity = Vector3.zero;

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
			transform.position = Vector3.Lerp (lastSyncPos, nextSyncPos, syncProg * invSyncDelay);
			transform.rotation = Quaternion.Lerp (lastSyncRot, nextSyncRot, syncProg * invSyncDelay);
			//rigidbody.MovePosition (Vector3.Lerp (lastSyncPos, nextSyncPos, syncProg * invSyncDelay));
			//rigidbody.MovePosition (Vector3.SmoothDamp (lastSyncPos, nextSyncPos, ref syncVelocity, syncDelay));
			//rigidbody.MoveRotation (Quaternion.Lerp (lastSyncRot, nextSyncRot, syncProg * invSyncDelay));;
			//print (rigidbody.position + " " + nextSyncPos);
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

		//rigidbody.MovePosition(nextPos);

		//syncVelocity = currVelocity;

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
