using UnityEngine;
using System.Collections;

public class InitMenu : MonoBehaviour {

	public string scavSceneName = "ScavengerMenu";
	public string goliathSceneName = "GoliathScene";

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void LoadScavScene(){
		Application.LoadLevel(scavSceneName);
	}

	public void LoadGoliathScene(){
		Application.LoadLevel(goliathSceneName);
	}
}
