using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GoliathGameScript : MonoBehaviour {
	//mech stuff
	public GameObject spawnPoint;
	public GameObject mechMesh;
	public GameObject waitingForPlayers;
	public GameObject blackOut;
	public GameObject menu1;
	public GameObject menu2;
	public GameObject menu3;
	public GameObject minimap;
	public GameObject goliathUI;

	public mechMovement movement;
	public MechShoot mechShoot;
	public GoliathNetworking network;

	public bool allConditions; 
	public bool netWorkReady;
	public bool restartMatch;
	public bool reLoadScene;

	bool menu1B;
	bool readyToGo;

	public float remainingTime;
	public UnityEngine.UI.Text timerText;
	// Use this for initialization
	void Start () {
		 minimap.SetActive(false);
	}
	
	// Update is called once per frame
	void Update () {

		if(remainingTime > 0){
			remainingTime = Mathf.Max(0, remainingTime - Time.deltaTime);
			
			string minutes = Mathf.Floor(remainingTime / 60).ToString();
			string seconds = Mathf.Floor(remainingTime % 60).ToString();
			
			if (seconds.Length == 1){
				seconds = "0" + seconds;
			}
			
			timerText.text = minutes + ":" + seconds;
		}


		if(reLoadScene == true){
			reLoad();
		}

		if(restartMatch == true){
			restartMatchFunction();
		}
		if(readyToGo == true){
			readyToStart();
		}
	}

	public void restartMatchFunction(){
		movement.allowedToMove = false;

		float lTrig = SixenseInput.Controllers[0].Trigger;
		float rTrig = SixenseInput.Controllers[1].Trigger;

		//turn the mech off so he can't move
		mechMesh.SetActive(false);
		goliathUI.SetActive(false);
		//removes oculus vision except for menues
		blackOut.SetActive(true);
		mechShoot.allowedToShoot = false;
		mechMesh.transform.position = spawnPoint.transform.position;
		if(readyToGo == false){
			if(menu1B == true){
				menu1.SetActive(true);
			}
			//turn on mech menues and wait to see if they go through
			if(lTrig >= 0.8){
				menu1B = false;
				menu1.SetActive(false);
				menu1.SetActive(false);
				menu2.SetActive(true);
			}
			if(rTrig >= 0.8){
				menu1.SetActive(false);
				menu2.SetActive(false);
				menu3.SetActive(true);
			}
			if(SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.START)){
				menu3.SetActive(false);
				readyToGo = true;
			}
		}
		if(readyToGo == true && netWorkReady == true){
			allConditions = true;
			network.photonView.RPC("GoliathConnected",PhotonTargets.All);
		}
		//check is they have clicked left fire then right fire then start
	}

	public void readyToStart(){
		waitingForPlayers.SetActive(true);

		if(allConditions = true && netWorkReady == true){

			menu1.SetActive(false);
			menu2.SetActive(false);
			menu3.SetActive(false);
			blackOut.SetActive(false);
			waitingForPlayers.SetActive(false);

			mechShoot.allowedToShoot = true;
			goliathUI.SetActive(true);
			minimap.SetActive(true);
			mechMesh.SetActive(true);
			movement.allowedToMove = true;

			restartMatch = false;
		}
	}

	public void reLoad(){
		Application.LoadLevel ("GoliathScene"); 
	}

	void OnEnable(){
		restartMatch = true;
	}

	public void goliathWon(){
		blackOut.SetActive(true);

		if(SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.START)){
			reLoad();
		}
	}

	public void goliathLost(){
		blackOut.SetActive(true);

		if(SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.START)){
			reLoad();
		}
	}
}
