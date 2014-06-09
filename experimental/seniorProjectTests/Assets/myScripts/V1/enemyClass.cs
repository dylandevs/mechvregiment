using UnityEngine;
using System.Collections;

public class enemyClass : MonoBehaviour {

	public float hp = 5;
	public Rigidbody[] gibs;
	public float explodeDistance = 1.0f;
	public float explodePower = 100.0f;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}

	void hasBeenShot()
	{
		hp--;
		if (hp <= 0) 
		{
			Die();
		}
	}

	void Die()
	{
		foreach(Rigidbody gib in gibs)
		{
			Rigidbody instance = Instantiate(gib,transform.position+Random.insideUnitSphere*explodeDistance *0.5f,Random.rotation) as Rigidbody;
			instance.AddExplosionForce(explodePower,transform.position,explodeDistance);
		}
		Destroy(gameObject);
	}
}
