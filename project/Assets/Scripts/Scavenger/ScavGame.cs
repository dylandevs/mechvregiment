using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using XInputDotNetPure;

public class ScavGame : MonoBehaviour {

	public float StartMatchTime = 300;
	private float remainingTime = 0;
	public UnityEngine.UI.Text timerText;

	public GameObject flag;
	public GameObject playerWrapper;
	private Player[] players;
	public GameObject exitPoint;

	public GameObject transitionMenu;

	[HideInInspector]
	public bool GameRunning = false;
	private bool goliathReady = false;

	// XInput variables
	private GamePadState state;
	private GamePadState prevState;

	// Use this for initialization
	void Start () {
		players = new Player[playerWrapper.transform.childCount];
		for (int i = 0; i < playerWrapper.transform.childCount; i++){
			players[i] = playerWrapper.transform.GetChild(i).GetComponent<Player>();
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

			timerText.text = minutes + ":" + seconds;

			if (remainingTime <= 0){
				GameLost();
			}
		}
		else{
			// Wait for X press
			state = GamePad.GetState((PlayerIndex)0);
			if (!state.IsConnected){
				return;
			}

			bool X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
		
			if (X_Press){// && goliathReady){
				BeginRound();
			}

			prevState = state;
		}
	}

	public void GoliathOnline(){
		goliathReady = true;
	}

	public void BeginRound(){
		remainingTime = StartMatchTime;
		GameRunning = true;
		transitionMenu.SetActive(false);
	}

	public void FlagRetrieved(GameObject retriever){
		foreach (Player player in players){
			player.display.minimap.UpdateObjective (retriever);
			flag.SetActive(false);
		}
	}

	public void FlagDropped(Vector3 flagPos){
		foreach (Player player in players){
			flag.transform.position = flagPos + Vector3.up;
			player.display.minimap.UpdateObjective (flag);
			flag.SetActive(true);
			flag.rigidbody.velocity = Vector3.zero;

			//flag.rigidbody.AddForce(direction * 10);
		}
	}

	void GameLost(){
		// Endgame UI
		print ("defeat");
	}

	public void GameWon(){
		// Endgame UI
		print ("victory");
	}
}
