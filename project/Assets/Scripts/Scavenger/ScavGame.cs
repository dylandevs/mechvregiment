using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using XInputDotNetPure;

public class ScavGame : MonoBehaviour {
	
	public bool forceStart = false;
	
	public float StartMatchTime = 300;
	private float remainingTime = 0;
	public UnityEngine.UI.Text timerText;
	
	public GameObject flag;
	public GameObject playerWrapper;
	private Player[] players;
	public GameObject exitPoint;
	
	public GameObject loader;
	public GameObject connecter;
	public GameObject startPrompt;
	public GoliathAvatar goliath;
	
	[HideInInspector]
	public bool GameRunning = false;
	private bool connectionReady = false;
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
	
	public ScavInstructions instructions;
	
	// Announcer audio
	public SplitAudioListener splitListener;
	public AudioSource matchStartSound;
	public AudioSource gotFlagSound;
	public AudioSource[] droppedFlagSound;
	public AudioSource goliathFlagSound;
	public AudioSource noTimeSound;
	
	// Soldier audio
	public AudioSource shieldDown;
	public AudioSource victory;
	public AudioSource defeat;
	public AudioSource goliathDisabled;
	public AudioSource goliathOnline;
	
	// Delayed audio sources
	public float destroyShieldDelay = 15;
	private bool destroyShieldPlayed = false;
	public AudioSource destroyShield;
	
	private GamePadState[] states = new GamePadState[4];
	private GamePadState[] prevStates = new GamePadState[4];
	
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
			
			splitListener.StoreAudioSource(shieldDown);
			splitListener.StoreAudioSource(victory);
			splitListener.StoreAudioSource(defeat);
			splitListener.StoreAudioSource(goliathDisabled);
			splitListener.StoreAudioSource(goliathOnline);
			
			splitListener.StoreAudioSource(destroyShield);
		}
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
			
			// Play delayed sounds
			if (StartMatchTime - remainingTime >= destroyShieldDelay && !destroyShieldPlayed){
				destroyShieldPlayed = true;
				splitListener.PlayAudioSource(destroyShield);
			}
			
			timerText.text = minutes + ":" + seconds;
			if(remainingTime > 0 && remainingTime <= 60){
				if(!timeRunningOutPlayed){
					splitListener.PlayAudioSource(noTimeSound);
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
					bool singlePlayerConfirmed = false;
					foreach(Player player in players){
						if (player.gameObject.GetActive() && player.readyToEnd){
							singlePlayerConfirmed = true;
							break;
						}
					}
					
					// Load menu
					if (singlePlayerConfirmed){
						PhotonNetwork.Disconnect();
						Time.timeScale = 1;
						Application.LoadLevel("ScavengerMenu");
					}
				}
			}
			else {
				// Reading instructions
				bool A_Press = false;
				bool X_Press = false;
				
				// Detecting any actives button presses
				for (int i = 0; i < 4; i++){
					states[i] = GamePad.GetState((PlayerIndex)i);
					if (states[i].IsConnected){
						if (!A_Press){
							A_Press = (states[i].Buttons.A == ButtonState.Pressed && prevStates[i].Buttons.A == ButtonState.Released);
						}
						if (!X_Press){
							X_Press = (states[i].Buttons.X == ButtonState.Pressed && prevStates[i].Buttons.X == ButtonState.Released);
						}
					}
					else{
						continue;
					}
					prevStates[i] = states[i];
				}
				
				if (A_Press){
					instructions.AdvanceInstructions();
				}
				
				if (X_Press){
					// Game not yet started
					if (forceStart){
						BeginRound();
					}
					else if (connectionReady && !gameToStart){
						networkManager.BeginAttemptingHandshake();
						connecter.SetActive(true);
						startPrompt.SetActive(false);
					}
				}
			}
		}
	}
	
	public void GoliathOnline(){
		BeginRound();
	}
	
	public void ReadyToConnect(){
		connectionReady = true;
		loader.SetActive (false);
		startPrompt.SetActive (true);
	}
	
	public void BeginRound(){
		remainingTime = StartMatchTime;
		GameRunning = true;
		gameToStart = true;
		instructions.gameObject.SetActive (false);
		
		splitListener.GetComponent<AudioListener>().enabled = true;
		splitListener.PlayAudioSource(matchStartSound);
	}
	
	public void FlagRetrieved(GameObject retriever){
		foreach (Player player in players){
			player.display.UpdateObjective (retriever);
			flag.SetActive(false);
		}
		if(retriever.tag == "Goliath"){
			splitListener.PlayAudioSource(goliathFlagSound);
		}
		else {
			splitListener.PlayAudioSource(gotFlagSound);
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
		splitListener.PlayAudioSource(droppedFlagSound[Random.Range(0,2)]);
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
		splitListener.PlayAudioSource(defeat);
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
		splitListener.PlayAudioSource(victory);
	}
	
	public void ShieldBroken(){
		splitListener.PlayAudioSource(shieldDown);
	}
}
