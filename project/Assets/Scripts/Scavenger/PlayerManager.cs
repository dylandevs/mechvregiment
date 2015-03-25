using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class PlayerManager : MonoBehaviour {

	public bool force4Split = false;

	const int NumControllers = 4;

	public GameObject playerWrapper;
	private Player[] playerScripts;
	private bool[] controllersUsed = {false, false, false, false};
	public GameObject skyCameraWrapper;
	private GameObject[] playerCams;
	
	public RuntimeAnimatorController fpsAnim;

	// Use this for initialization
	void Start () {
		playerScripts = new Player[playerWrapper.transform.childCount];
		for (int i = 0; i < playerWrapper.transform.childCount; i++){
			playerScripts[i] = playerWrapper.transform.GetChild(i).GetComponent<Player>();
			playerScripts[i].fpsAnim.runtimeAnimatorController = fpsAnim;
			playerScripts[i].initializer.Layer = i + 1;
		}

		playerCams = new GameObject[skyCameraWrapper.transform.childCount];
		for (int i = 0; i < skyCameraWrapper.transform.childCount; i++){
			playerCams[i] = skyCameraWrapper.transform.GetChild(i).gameObject;
			playerCams[i].GetComponent<SkyCamera>().playerController = playerScripts[i].playerController;
			playerScripts[i].display.skyCam = playerCams[i].camera;
		}

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
							playerCams[i].SetActive(true);
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
