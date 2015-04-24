﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavGame : MonoBehaviour {

	public float StartMatchTime = 300;
	private float remainingTime = 0;
	public UnityEngine.UI.Text timerText;

	public GameObject flag;
	public GameObject playerWrapper;
	private Player[] players;
	public GameObject exitPoint;

	public GameObject transitionMenu;
	public GoliathAvatar goliath;

	[HideInInspector]
	public bool GameRunning = false;
	private bool goliathReady = false;
	private bool gameToStart = false;
	private bool gameToEnd = false;
	[HideInInspector]
	public bool awaitingEndConfirm = false;

	public PlayerNetSend networkManager;

	public float uiTransitionTime = 0.4f;
	private float invUiTransitionTime = 1;
	private float lastTimeCheck = 0;
	private float endAnimProgress = 0;
	private bool timeRunningOutPlayed = false;
	
	public CanvasGroup victoryModal;
	public CanvasGroup defeatModal;
	private CanvasGroup currentModal;

	//Announcer audio
	public SplitAudioListener splitListener;
	public AudioSource matchStartSound;
	public AudioSource gotFlagSound;
	public AudioSource[] droppedFlagSound;
	public AudioSource goliathFlagSound;
	public AudioSource noTimeSound;

	// Use this for initialization
	void Start () {
		players = new Player[playerWrapper.transform.childCount];
		for (int i = 0; i < playerWrapper.transform.childCount; i++){
			players[i] = playerWrapper.transform.GetChild(i).GetComponent<Player>();
		}

		invUiTransitionTime = 1 / uiTransitionTime;

		if(splitListener){
			splitListener.StoreAudioSource(matchStartSound);
			splitListener.StoreAudioSource(gotFlagSound);
			foreach(AudioSource flagDropVariation in droppedFlagSound){
				splitListener.StoreAudioSource(flagDropVariation);
			}
			splitListener.StoreAudioSource(goliathFlagSound);
			splitListener.StoreAudioSource(noTimeSound);
		}
		//BeginRound();
	}
	
	// Update is called once per frame
	void Update () {
		if (GameRunning){
			remainingTime = Mathf.Max(0, remainingTime - Time.deltaTime);

			string minutes = Mathf.Floor(remainingTime / 60).ToString();
			string seconds = Mathf.Floor(remainingTime % 60).ToString();

			if (seconds.Length == 1){
				seconds = "0" + seconds;
			}

			timerText.text = minutes + ":" + seconds;
			if(remainingTime > 0 && remainingTime <= 60){
				if(!timeRunningOutPlayed){
					if(splitListener){
						splitListener.PlayAudioSource(noTimeSound);
					}
					else {
						noTimeSound.Play();
					}
					timeRunningOutPlayed = true;
				}
			}
			else if (remainingTime <= 0){
				GameLost();
			}
		}
		else{
			// Game ended
			if (gameToEnd){
				if (endAnimProgress > 0){
					float deltaTime = Time.realtimeSinceStartup - lastTimeCheck;
					endAnimProgress -= deltaTime;

					currentModal.alpha = Mathf.Lerp(0, 1, 1 - endAnimProgress/invUiTransitionTime);

					if (endAnimProgress <= 0){
						currentModal.alpha = 1;
						awaitingEndConfirm = true;
					}
				}
				else{
					bool playersConfirmed = true;
					foreach(Player player in players){
						if (player.gameObject.GetActive() && !player.readyToEnd){
							playersConfirmed = false;
						}
					}

					// Load menu
					if (playersConfirmed){
						PhotonNetwork.Disconnect();
						Time.timeScale = 1;
						Application.LoadLevel("ScavengerMenu");
					}
				}
			}
		
			// Game not yet started
			if (goliathReady && !gameToStart){
				BeginRound();
			}
		}
	}

	public void GoliathOnline(){
		goliathReady = true;
	}

	public void BeginRound(){
		remainingTime = StartMatchTime;
		GameRunning = true;
		transitionMenu.SetActive(false);
		gameToStart = true;
		if(splitListener){
			splitListener.PlayAudioSource(matchStartSound);
		}
		else {
			matchStartSound.Play();
		}
	}

	public void FlagRetrieved(GameObject retriever){
		foreach (Player player in players){
			player.display.UpdateObjective (retriever);
			flag.SetActive(false);
		}
		if(retriever.tag == "Goliath"){
			if(splitListener){
				splitListener.PlayAudioSource(goliathFlagSound);
			}
			else {
				goliathFlagSound.Play();
			}
		}
		else {
			if(splitListener){
				splitListener.PlayAudioSource(gotFlagSound);
			}
			else {
				gotFlagSound.Play();
			}
		}
	}

	public void FlagDropped(Vector3 flagPos){
		foreach (Player player in players){
			flag.transform.position = flagPos + Vector3.up;
			player.display.UpdateObjective (flag);
			flag.SetActive(true);
			flag.GetComponent<Rigidbody>().velocity = Vector3.zero;

			//flag.rigidbody.AddForce(direction * 10);
		}
		if(splitListener){
			splitListener.PlayAudioSource(droppedFlagSound[Random.Range(0,2)]);
		}
		else {
			droppedFlagSound[Random.Range(0,2)].Play();
		}
	}

	void GameLost(){
		// Endgame UI
		networkManager.photonView.RPC("GoliathWin", PhotonTargets.All);
		GameRunning = false;
		lastTimeCheck = Time.realtimeSinceStartup;
		endAnimProgress = uiTransitionTime;
		currentModal = defeatModal;
		gameToEnd = true;
		Time.timeScale = 0;
	}

	public void GameWon(){
		// Endgame UI
		networkManager.photonView.RPC("ScavengerWin", PhotonTargets.All);
		GameRunning = false;
		lastTimeCheck = Time.realtimeSinceStartup;
		endAnimProgress = uiTransitionTime;
		currentModal = victoryModal;
		gameToEnd = true;
		Time.timeScale = 0;
	}
}
