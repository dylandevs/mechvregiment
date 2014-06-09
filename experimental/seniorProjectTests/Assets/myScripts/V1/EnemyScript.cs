using UnityEngine;
using System.Collections;

public class EnemyScript : MonoBehaviour {

	public float speed = 2.0f;

	// Use this for initialization
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		//moves toward the player
		Vector3 playerPos = GameObject.FindGameObjectWithTag("Player").transform.position;
		Vector3 normalizedPlayerLook = (playerPos - transform.position).normalized;
		transform.position +=  normalizedPlayerLook * speed * Time.deltaTime;

		//looks toward character
		transform.rotation = Quaternion.LookRotation (normalizedPlayerLook);
	}

}
