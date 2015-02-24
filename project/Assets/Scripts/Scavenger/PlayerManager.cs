using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class PlayerManager : MonoBehaviour {

	public bool force4Split = false;

	const int NumControllers = 4;
	
	public Player[] playerScripts = new Player[NumControllers];
	private bool[] controllersUsed = {false, false, false, false};

	// Use this for initialization
	void Start () {
		assignControllers(countConnectedControllers());
	}
	
	// Update is called once per frame
	void Update () {
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
							controllersUsed[j] = true;
							playerScripts[i].Initialize(j + 1, getWindowCoords(i + 1, connectedControllers), GetUIReferenceScale(connectedControllers));
							playerScripts[i].gameObject.SetActive(true);
							break;
						}
					}
				}
			}
		}
		else{
			playerScripts[0].Initialize(1, getWindowCoords(1, 1), 1);
			playerScripts[0].SetToKeyboard();
			playerScripts[0].gameObject.SetActive(true);
		}
	}

	float GetUIReferenceScale(int connectedControllers){
		if (connectedControllers == 1 && !force4Split){
			return 1;
		}
		else{
			return 2;
		}
	}

	// Returns appropriate window coordinates
	float[] getWindowCoords(int playerIndex, int totalControllers){
		if (totalControllers > 2 || force4Split){
			switch(playerIndex){
			case 1:
				return new float[]{0, 0.5f, 0.5f, 0.5f};
			case 2:
				return new float[]{0.5f, 0.5f, 0.5f, 0.5f};
			case 3:
				return new float[]{0.5f, 0, 0.5f, 0.5f};
			case 4:
				return new float[]{0, 0, 0.5f, 0.5f};
			}
		}
		else if (totalControllers == 2){
			switch(playerIndex){
			case 1:
				return new float[]{0, 0.5f, 1, 0.5f};
			case 2:
				return new float[]{0, 0, 1, 0.5f};
			}
		}

		// Default to full screen
		return new float[]{0, 0, 1, 1};

	}
}
