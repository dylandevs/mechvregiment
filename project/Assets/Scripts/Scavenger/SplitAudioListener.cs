using UnityEngine;
using System.Collections;

public class SplitAudioListener : MonoBehaviour {

	public Transform playerWrapper;
	private Transform[] playerTransforms;

	// Use this for initialization
	void Start () {
		playerTransforms = new Transform[playerWrapper.childCount];
		for (int i = 0; i < playerWrapper.childCount; i++){
			playerTransforms[i] = playerWrapper.GetChild(i);
		}
	}
	
	// Update is called once per frame
	void Update () {
		UpdatePosition ();
	}

	private void UpdatePosition(){
		int count = 0;
		Vector3 averagePosition = Vector3.zero;
		foreach(Transform player in playerTransforms){
			if (player.gameObject.GetActive()){
				averagePosition += player.position;
				count++;
			}
		}

		if (count > 0){
			transform.position = averagePosition / count;
		}
	}
}
