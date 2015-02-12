using UnityEngine;
using System.Collections;

public class MinimapIcon : MonoBehaviour {

	public GameObject associatedObject;
	public enum MMIconType {Objective, Scavenger, Minion, Goliath};
	public MMIconType type;
	public BotAI enemyScript = null;
	// public Goliath goliathScript = null;

	// Use this for initialization
	void Start () {
		// Attempt to grab all scripts
		enemyScript = associatedObject.GetComponent<BotAI> ();
		// goliathScript = associatedObject.GetComponent<Goliath> ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
