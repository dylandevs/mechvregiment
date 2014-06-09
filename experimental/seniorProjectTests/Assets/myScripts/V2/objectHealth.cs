using UnityEngine;
using System.Collections;

public class objectHealth : MonoBehaviour {

	public float hp = 100f;
	public Rigidbody[] gibs;
	public float explodeDistance = 2.0f;
	public float explodePower = 200.0f;

	public void ReciveDamage(float amt)
	{
		Debug.Log ("damageTaken:" + amt);

		hp -= amt;
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
