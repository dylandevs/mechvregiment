using UnityEngine;
using System.Collections;

public class ARTracer : MonoBehaviour {

	private const float Speed = 80;
	private const float MaxDuration = 2;

	private PoolManager pool;
	private float duration = MaxDuration;
	private float checkDist = 1;

	public Vector3 endpoint = Vector3.zero;

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
		checkDist = Speed / 60;
	}
	
	// Update is called once per frame
	void Update () {
		duration -= Time.deltaTime;
		transform.position += transform.forward * Speed * Time.deltaTime;

		if (duration <= 0){
			pool.Deactivate(gameObject);
		}
		else if (Physics.Raycast(transform.position, transform.forward, checkDist)){
			pool.Deactivate(gameObject);
		}
	}

	void OnEnable(){
		duration = MaxDuration;
	}
}
