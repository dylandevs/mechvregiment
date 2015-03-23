using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavGame : MonoBehaviour {

	public float StartMatchTime = 300;
	private float remainingTime = 0;
	private float inv60 = 1;
	public UnityEngine.UI.Text timerText;

	bool GameRunning = false;

	// Use this for initialization
	void Start () {
		BeginRound ();
		inv60 = 1 / 60;
	}
	
	// Update is called once per frame
	void Update () {
		if (GameRunning){
			remainingTime = Mathf.Max(0, remainingTime - Time.deltaTime);

			//print (inv60);

			print (remainingTime * inv60);

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
}
