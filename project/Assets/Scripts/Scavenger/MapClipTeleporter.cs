using UnityEngine;
using System.Collections;

public class MapClipTeleporter : MonoBehaviour {

	public LayerMask terrainMask;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	void OnCollisionEnter(Collision collision){
		if (collision.gameObject.tag == "Player" || collision.gameObject.tag == "Mine"){
			RaycastHit rayHit;
			if (Physics.Raycast(collision.contacts[0].point + Vector3.up * 100, Vector3.down, out rayHit, 100, terrainMask)){
				collision.gameObject.transform.position = rayHit.point + Vector3.up;
			}
		}
	}
}
