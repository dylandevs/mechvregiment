using UnityEngine;
using System.Collections;

public class ParticleTargeting : MonoBehaviour {

	public GameObject target;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 targetDir = target.transform.position - gameObject.transform.position;
		gameObject.transform.forward = targetDir;
	}
}
