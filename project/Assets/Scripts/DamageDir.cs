using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DamageDir : MonoBehaviour {

	const float baseDuration = 1;
	const float invBaseDuration = 1 / baseDuration;

	float duration = 0;
	Quaternion baseRot;

	PoolManager pool;
	Player player;

	// Inputs
	public Image dirGraphic;

	// Use this for initialization
	void Start () {
		pool = transform.parent.GetComponent<PoolManager>();
	}
	
	// Update is called once per frame
	void Update () {
		Quaternion origRot = player.transform.rotation * baseRot;
		transform.localRotation = Quaternion.Euler(0, 0, 180-origRot.eulerAngles.y);

		duration -= Time.deltaTime;

		if (duration <= 0){
			pool.Deactivate(gameObject);
		}
		else{
			// Update graphic opacity
			dirGraphic.color = new Color(1, 1, 1, duration * invBaseDuration);
		}
	}

	public void Initialize(Player playerRef, Quaternion rot){
		baseRot = rot;
		player = playerRef;
		duration = baseDuration;

		Quaternion origRot = player.transform.rotation * baseRot;
		transform.localRotation = Quaternion.Euler(0, 0, 180-origRot.eulerAngles.y);
	}
}
