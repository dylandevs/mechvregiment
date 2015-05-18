using UnityEngine;
using System.Collections;

public class BurntMinion : MonoBehaviour {

	private float life = 10;
	private PoolManager pool;
	private Vector3[] originalPos;
	public Transform pieces;

	void Awake(){
		originalPos = new Vector3[pieces.transform.childCount];
		for (int i = 0; i < pieces.transform.childCount; i++){
			originalPos[i] = pieces.transform.GetChild(i).localPosition;
		}

	}

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {
		life -= Time.deltaTime;

		if (life <= 0){
			pool.Deactivate(gameObject);
		}
	}

	void OnEnable(){
		life = 10;

		// Reset positions
		for (int i = 0; i < pieces.transform.childCount; i++){
			Transform child = pieces.transform.GetChild(i);

			Rigidbody childRigidbody = child.GetComponent<Rigidbody>();

			childRigidbody.velocity = Vector3.zero;
			childRigidbody.angularVelocity = Vector3.zero;
			child.localPosition = originalPos[i];
			child.localRotation = Quaternion.identity;
			childRigidbody.AddForce(new Vector3(Random.Range(-50, 50), Random.Range(-50, 50), Random.Range(-50, 50)));
		}
	}
}
