using UnityEngine;
using System.Collections;

public class PlayerAvatarDamager : MonoBehaviour {

	public PlayerAvatar player;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DamagePlayer(float damage, Vector3 direction){
		player.Damage(damage, direction);
	}
}
