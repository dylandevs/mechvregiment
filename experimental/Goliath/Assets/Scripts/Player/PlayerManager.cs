using UnityEngine;
using System.Collections;

public class PlayerManager : MonoBehaviour {

	const int NUM_CONTROLLERS = 4;

	public GameObject basePlayer;

	GameObject[] players = new GameObject[NUM_CONTROLLERS];
	Player[] playerScripts = new Player[NUM_CONTROLLERS];

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		listenForControllers();
	}

	void listenForControllers(){
		for (int i = 0; i < NUM_CONTROLLERS; i++){
			if (Input.GetButtonDown("A_" + (i + 1)) && players[i] == null){
				players[i] = Instantiate(basePlayer, Vector3.zero, Quaternion.identity) as GameObject;
				playerScripts[i] = players[i].GetComponent<Player>();
				playerScripts[i].Initialize("Player", i + 1, getWindowCoords(i + 1, NUM_CONTROLLERS));
			}
		}
	}


	// Returns appropriate window coordinates
	float[] getWindowCoords(int playerIndex, int maxPlayers){
		if (maxPlayers == 2){
			switch(playerIndex){
			case 1:
				return new float[]{0, 1, 0.5f, 1};
			case 2:
				return new float[]{0, 1, 0, 0.5f};
			}
		}
		else if (maxPlayers == 4){
			switch(playerIndex){
			case 1:
				return new float[]{0, 0.5f, 0.5f, 1};
			case 2:
				return new float[]{0.5f, 1, 0.5f, 1};
			case 3:
				return new float[]{0, 0.5f, 0, 0.5f};
			case 4:
				return new float[]{0.5f, 1, 0, 0.5f};
			}
		}

		// Default to full screen
		return new float[]{0, 1, 0, 1};

	}
}
