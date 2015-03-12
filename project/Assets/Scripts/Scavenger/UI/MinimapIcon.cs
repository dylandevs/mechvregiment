using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MinimapIcon : MonoBehaviour {

	public GameObject associatedObject;
	public enum MMIconType {Objective, Scavenger, Minion, Goliath};
	public MMIconType type;
	public BotAI minionScript = null;
	public Image img;
	// public Goliath goliathScript = null;

	float InvBaseEnemyTime = 1;

	// Use this for initialization
	void Start () {
		// Attempt to grab all scripts
		minionScript = associatedObject.GetComponent<BotAI> ();
		InvBaseEnemyTime = 1 / BotAI.FireRate;
		// goliathScript = associatedObject.GetComponent<Goliath> ();
	}
	
	// Update is called once per frame
	void Update () {

	}

	public float GetEnemyOpacity(){
		if (associatedObject.GetActive() && minionScript){
			return (BotAI.FireRate - minionScript.reloadProg) * InvBaseEnemyTime;
		}

		return 0;
	}
}
