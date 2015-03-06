using UnityEngine;
using System.Collections;

public class RocketFirer : MonoBehaviour {
	public GameObject rocketStart;
	public GameObject target;

	const int RocketNumber = 6;

	public const float RocketDelay = 0.5f;
	public float rocketDelayTimer = 0;

	public PoolManager rocketPool;
	public PoolManager explosionPool;
	

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (rocketDelayTimer > 0){
			rocketDelayTimer -= Time.deltaTime;

			if (rocketDelayTimer <= 0){
				firingMyRockets();
			}
		}

	}

	public void firingMyRockets(){
		//spawn each rocket close to the rocket starting point at an intervaled time
		for (int i = 0; i < RocketNumber; i++){
				
			//not starting properly but should set the proper inital firing position
			Quaternion startRot = Quaternion.Euler (90, 0, 0);

			//spawn them a little off center
			Vector3 rocketLaunch = rocketStart.transform.position += new Vector3 (Random.Range (-10F, 10F), Random.Range (1F, 50F), Random.Range (-10F, 10F));
			GameObject currentRocket = rocketPool.Retrieve(rocketLaunch, startRot);

			RocketScript rocketScript = currentRocket.GetComponent<RocketScript>();
			rocketScript.SetTarget(target.transform.position);
			rocketScript.explosionPool = explosionPool;

		}

	}
}
