using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavGame : MonoBehaviour {

	public float StartMatchTime = 300;
	private float remainingTime = 0;
	private float inv60 = 1;
	public UnityEngine.UI.Text timerText;

	public GameObject flag;
	public GameObject playerWrapper;
	private Player[] players;

	bool GameRunning = false;

	// Use this for initialization
	void Start () {
		BeginRound ();
		inv60 = 1 / 60;

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
				// Round over

			}
		}
	}

	public void BeginRound(){
		remainingTime = StartMatchTime;
		GameRunning = true;
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

			//flag.rigidbody.AddForce(direction * 10);
		}
	}
}
