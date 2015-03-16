using UnityEngine;
using System.Collections;

public class RocketFirer : MonoBehaviour {
	public GameObject rocketStart;
	public GameObject target;

	const int RocketNumber = 35;

	public const float RocketDelay = 0.5f;
	public float rocketDelayTimer = 0;

	public PoolManager rocketPool;
	public PoolManager explosionPool;
	public GoliathNetworking networkManager;
	

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
				firingMyRockets();
	}

	public void firingMyRockets(){
		//spawn each rocket close to the rocket starting point at an intervaled time
		for (int i = 0; i < RocketNumber; i++){
				
			//not starting properly but should set the proper inital firing position
			Quaternion startRot = Quaternion.Euler (90, 0, 0);

			//spawn them a little off center
			Vector3 rocketLaunch = rocketStart.transform.position + new Vector3 (Random.Range (-20F, 20F), Random.Range (50F, 1000F), Random.Range (-20F, 20F));
			GameObject currentRocket = rocketPool.Retrieve(rocketLaunch, startRot);

			RocketScript rocketScript = currentRocket.GetComponent<RocketScript>();
			rocketScript.SetTarget(target.transform.position);
			rocketScript.explosionPool = explosionPool;

			networkManager.photonView.RPC ("CreateGoliathMeteor", PhotonTargets.All, rocketLaunch, target.transform.position);
		}
	}
}
