using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class PlayerManager : MonoBehaviour {

	const int NumControllers = 4;

	public GameObject basePlayer;

	GameObject[] players = new GameObject[NumControllers];
	public Player[] playerScripts = new Player[NumControllers];
	private bool[] controllersUsed = {false, false, false, false};

	// Use this for initialization
	void Start () {
		assignControllers(countConnectedControllers());
		//Player[0].Initialize(1,
	}
	
	// Update is called once per frame
	void Update () {
		//listenForControllers();
	}

	int countConnectedControllers(){
		int controllers = 0;
		for (int i = 0; i < NumControllers; i++) {
			if (GamePad.GetState((PlayerIndex)i).IsConnected){
				controllers++;
			}
		}

		return controllers;
	}

	void assignControllers(int connectedControllers){
		if (connectedControllers > 0){
			for (int i = 0; i < connectedControllers; i++){
				if (playerScripts[i]){
					for (int j = 0; j < NumControllers; j++){
						if (GamePad.GetState((PlayerIndex)j).IsConnected && !controllersUsed[j]){
							print ("Assigned " + j);
							controllersUsed[j] = true;
							playerScripts[i].Initialize(j + 1, getWindowCoords(i + 1, connectedControllers));
							playerScripts[i].gameObject.SetActive(true);
							break;
						}
					}
				}
			}
		}
		else{
			playerScripts[0].Initialize(1, getWindowCoords(1, 1));
			playerScripts[0].SetToKeyboard();
			playerScripts[0].gameObject.SetActive(true);
		}
	}

	void listenForControllers(){
		for (int i = 0; i < NumControllers; i++){
			if (Input.GetButtonDown("A_" + (i + 1)) && players[i] == null){
				players[i] = Instantiate(basePlayer, Vector3.zero, Quaternion.identity) as GameObject;
				playerScripts[i] = players[i].GetComponent<Player>();
				//playerScripts[i].Initialize(i + 1, getWindowCoords(i + 1, NumControllers));
			}
		}
	}


	// Returns appropriate window coordinates
	float[] getWindowCoords(int playerIndex, int totalControllers){
		if (totalControllers == 2){
			switch(playerIndex){
			case 1:
				return new float[]{0, 1, 0.5f, 1};
			case 2:
				return new float[]{0, 1, 0, 0.5f};
			}
		}
		else if (totalControllers > 2){
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
