using UnityEngine;
using System.Collections;

public class SetChildTag : MonoBehaviour {

	public string tagName = "Untagged";

	// Use this for initialization
	void Start () {
		SetTagRecursively (gameObject, tagName);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetTagRecursively(GameObject baseObj, string tag){
		baseObj.tag = tag;
		
		foreach (Transform child in baseObj.transform){
			SetTagRecursively(child.gameObject, tag);
		}
	}
}
