using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour {

	public PlayerNetSend networkManager;
	public PoolManager hitPool;
	public bool goliathShield = true;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void DamageGoliath(float damage, Vector3 direction, Vector3 position){
		if (goliathShield) {
			networkManager.photonView.RPC ("DamageGoliathShielded", PhotonTargets.All, damage, direction, position);
		}
		GameObject shieldHit = hitPool.Retrieve(position);
		shieldHit.transform.forward = -direction;
	}
}
