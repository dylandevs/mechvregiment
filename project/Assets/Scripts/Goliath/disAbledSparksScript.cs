using UnityEngine;
using System.Collections;

public class disAbledSparksScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (particleEmitter.particleCount == 0) {
			gameObject.SetActive(false);
		}
	}
	void OnEnable(){
		particleEmitter.Emit();
	}

}
