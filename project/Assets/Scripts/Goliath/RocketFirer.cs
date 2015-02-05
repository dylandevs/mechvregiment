using UnityEngine;
using System.Collections;

public class RocketFirer : MonoBehaviour {
	public GameObject rocketStart;
	public GameObject[] rockets;
	public bool firing;

	float rockTimer = 0.2f;
	int rockCounter = 0;

	// Use this for initialization
	void Start () {
		firing = false;
	}
	
	// Update is called once per frame
	void Update () {
		rockTimer -= Time.deltaTime;

		if (firing == true) {
			firingMyRockets();
		}

	}

		public void firingMyRockets(){

			//spawn each rocket close to the rocket starting point at an intervaled time
			if (rockTimer <= 0) {
				GameObject currentRocket = rockets [rockCounter];
				
				//not starting properly but should set the proper inital firing position
				Vector3 startRot = new Vector3 (270, 0, 0);
				currentRocket.transform.rotation = Quaternion.identity;
				currentRocket.transform.Rotate (startRot);
				//spawn them a little off center
				Vector3 rocketLaunch = rocketStart.transform.position += new Vector3 (Random.Range (-0.25F, 0.25F), Random.Range (-0.25F, 0.25F), Random.Range (-0.25F, 0.25F));
				currentRocket.transform.position = rocketLaunch;
				currentRocket.SetActive (true);
				
				rockTimer = 0.1f;
				rockCounter += 1;
				
				if (rockCounter >= 4) {
					rockCounter = 0;
					firing = false;
				}

			}
		}
	}
