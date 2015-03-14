using UnityEngine;
using System.Collections;

public class GoliathDamager : MonoBehaviour {

	public GoliathAvatar goliath;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DamageGoliath(float damage, Vector3 direction){
		goliath.Damage(damage, direction);
	}
}
