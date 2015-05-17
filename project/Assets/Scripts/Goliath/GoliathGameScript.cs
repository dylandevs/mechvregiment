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
	public GameObject winScreen;
	public GameObject looseScreen;
	public GameObject screens;
	public GameObject tutorialPos;
	public GameObject mech;
	public GameObject cameraPos;
	public GameObject cannonArm;
	public GameObject miniGunArm;


	public mechMovement movement;
	public MechShoot mechShoot;
	public GoliathNetworking network;

	public bool allConditions; 
	public bool netWorkReady;
	public bool restartMatch;
	public bool reLoadScene;
	public bool tutorial;
	public bool tutorialPass = false;
	public bool previousr;

	public Image artifactLostImage;
	public Image artifactDefendedImage;

	public int tutorialNumber;
	
	public AudioSource matchStartSound;
	public AudioSource flagTakenSound;
	public AudioSource noTimeSound;
	public AudioSource heavyDamageSound;

	public float remainingTime;
	public UnityEngine.UI.Text timerText;

	
	bool menu1B;
	bool readyToGo;
	bool oneTime;
	bool gameEnded;
	bool one;
	bool two;
	bool win;
	bool loose;
	bool noTimePlayed = false;
	
	bool pressed;
	bool previous;
	bool pressedr;

	float life = 0;
	float lTrig;
	float rTrig;
	
	int left = 0;
	int right = 1;

	// Use this for initialization
	void Start () {
		tutorialNumber = 1;
		tutorial = true;
		tutorialPass = false;
		previousr = false;
		pressed = false;
		pressedr = false;
		menu1B = true;
	}
	
	// Update is called once per frame
	void Update () {

		lTrig = SixenseInput.Controllers[left].Trigger;
		rTrig = SixenseInput.Controllers[right].Trigger;
		
		if (Input.GetKey(KeyCode.S)) {

			print ("swapped");

			if(left == 0){
				left = 1;
				right = 0;
			}
			else{
				left = 0;
				right = 1;
			}
		}

		if (tutorial == true) {
			tutuorialFuncion();
		}

		if (tutorialPass == true) {
			if (remainingTime > 0) {
				remainingTime = Mathf.Max (0, remainingTime - Time.deltaTime);
			
				string minutes = Mathf.Floor (remainingTime / 60).ToString ();
				string seconds = Mathf.Floor (remainingTime % 60).ToString ();
			
				if (remainingTime <= 60 && !noTimePlayed) {
					noTimeSound.Play ();
				}

				if (seconds.Length == 1) {
					seconds = "0" + seconds;
				}
			
				timerText.text = minutes + ":" + seconds;
			}


			if (reLoadScene == true) {
				reLoad ();
			}

			if (restartMatch == true) {
				restartMatchFunction ();
			}
			if (readyToGo == true) {
				readyToStart ();
			}
		}

		//checks if  the game has ended then does stuff
		if (gameEnded == true) {

			if (life >= 0) {
				life -= Time.deltaTime;
			}

			//change colour of the screens
			float lerpAmnt = life / 3;
			screens.GetComponent<Renderer> ().material.color = Color.Lerp (Color.black, Color.white, lerpAmnt);
			Color tempColour = screens.GetComponent<Renderer> ().material.color;
			tempColour.a = Mathf.Lerp (0, 1, 1 - lerpAmnt);
			screens.GetComponent<Renderer> ().material.color = tempColour;

			if (win == true) {
				artifactDefendedImage.fillAmount = 1 - lerpAmnt;
			}
			if (loose == true) {
				artifactLostImage.fillAmount = 1 - lerpAmnt;
			}


			mechShoot.allowedToShootGame = false;
			movement.allowedToDash = false;
			movement.dash = false;

			if (SixenseInput.Controllers [1].GetButtonDown (SixenseButtons.START)) {
				reLoad ();
			}
		}
	}

	public void restartMatchFunction(){

		print ("in restart function");

		mech.transform.position = spawnPoint.transform.position;

		minimap.SetActive(false);
		movement.allowedToMove = false;
		movement.allowedToDash = false;
		lTrig = SixenseInput.Controllers[0].Trigger;
		rTrig = SixenseInput.Controllers[1].Trigger;

		//turn the mech off so he can't move
		mechMesh.SetActive(false);
		goliathUI.SetActive(false);
		//removes oculus vision except for menues
		blackOut.SetActive(true);
		mechShoot.allowedToShootGame = false;


		if (Input.GetKey (KeyCode.R)) {
			PhotonNetwork.Disconnect();
			Application.LoadLevel("GoliathScene"); 
		}

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
				one = true;
			}
			if(rTrig >= 0.8 && one == true){
				menu1.SetActive(false);
				menu2.SetActive(false);
				menu3.SetActive(true);
				two = true;
			}
			if(SixenseInput.Controllers[1].GetButtonDown(SixenseButtons.START) && two == true){
				menu3.SetActive(false);
				readyToGo = true;
			}
		}
		if(readyToGo == true && netWorkReady == true){
			allConditions = true;
			network.photonView.RPC("GoliathConnected",PhotonTargets.All);
			noTimePlayed = false;
		}
		//check is they have clicked left fire then right fire then start
	}

	public void readyToStart(){
		if(oneTime == true){
			waitingForPlayers.SetActive(true);

			if(allConditions = true && netWorkReady == true){
				mech.transform.position = spawnPoint.transform.position;
				menu1.SetActive(false);
				menu2.SetActive(false);
				menu3.SetActive(false);
				blackOut.SetActive(false);
				waitingForPlayers.SetActive(false);

				mechShoot.connected = true;
				mechShoot.allowedToShootGame = true;
				goliathUI.SetActive(true);
				minimap.SetActive(true);
				mechMesh.SetActive(true);
				movement.allowedToMove = true;
				movement.allowedToDash = true;
				restartMatch = false;
				oneTime = false;
				matchStartSound.Play();
			}
		}
	}

	public void reLoad(){
		PhotonNetwork.Disconnect();
		Application.LoadLevel("GoliathStartScene"); 
	}

	void OnEnable(){
		oneTime = true;
	}

	public void goliathWon(){
		//display a win message and turn off movement and shooting stuff and turn off windows
		winScreen.SetActive(true);
		movement.allowedToMove = false;
		life = 3;
		gameEnded = true;
		win = true;
	}

	public void goliathLost(){
		//display a win message and turn off movement and shooting stuff and turn off windows
		looseScreen.SetActive(true);
		movement.allowedToMove = false;
		life = 3;
		gameEnded = true;
		loose = true;
	}

	public void tutuorialFuncion(){

		if (tutorialNumber == 1) {
			mapfunction();
		}

		if (tutorialNumber == 2) {
			healthFunction();
		}

		if (tutorialNumber == 3) {
			leftHandShooting();
		}

		if (tutorialNumber == 4) {
			rightHandShooting();
		}

		if (tutorialNumber == 5) {
			finalStep();
		}

		if (tutorialPass == true) {
			tutorial = false;
			restartMatch = true;
		}
	}

	void mapfunction(){

		// check if he looked at map
		Ray minMode = new Ray(cameraPos.transform.position,cameraPos.transform.forward);
		RaycastHit minHit;
		
		tutorialNumber = 3;
		//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
		if (Physics.Raycast (minMode, out minHit, 75)) {
			if (minHit.collider.tag == "miniMap") {

			}
		}
	}

	void healthFunction(){
		//check if looked at health
		Ray minMode = new Ray(cameraPos.transform.position,cameraPos.transform.forward);
		RaycastHit minHit;

			if (Physics.Raycast (minMode, out minHit, 75)) {
			if (minHit.collider.tag == "Shield") {
				tutorialNumber = 3;
			}
		}
	
	}

	void leftHandShooting(){
		//keep arms up
		miniGunArm.transform.localEulerAngles = new Vector3(290,355,2);
		cannonArm.transform.localEulerAngles =  new Vector3(287,22,354);

		if (SixenseInput.Controllers [left].GetButtonDown (SixenseButtons.ONE) || SixenseInput.Controllers [left].GetButtonDown (SixenseButtons.TWO) ||
		    SixenseInput.Controllers [left].GetButtonDown (SixenseButtons.THREE) || SixenseInput.Controllers [left].GetButtonDown (SixenseButtons.FOUR)) {
			pressed = true;

		}

		if (SixenseInput.Controllers [left].GetButtonUp (SixenseButtons.ONE) || SixenseInput.Controllers [left].GetButtonUp (SixenseButtons.TWO) ||
		    SixenseInput.Controllers [left].GetButtonUp (SixenseButtons.THREE) || SixenseInput.Controllers [left].GetButtonUp (SixenseButtons.FOUR)) {
			pressed = false;
		}

		if (lTrig >= 0.8 && pressed == false ) {
			print("shot L");
			//display next ui
			//turn off previous ui
			previous = true;

		}
		if (lTrig >= 0.8 && pressed == true && previous == true ) {
			//turn off all UI
			print("shot L pressed");
			tutorialNumber = 4;
		}
	}

	void rightHandShooting(){

		miniGunArm.transform.localEulerAngles = new Vector3(290,355,2);
		cannonArm.transform.localEulerAngles =  new Vector3(287,22,354);
		
		if (SixenseInput.Controllers [right].GetButtonDown (SixenseButtons.ONE) || SixenseInput.Controllers [right].GetButtonDown (SixenseButtons.TWO) ||
		    SixenseInput.Controllers [right].GetButtonDown (SixenseButtons.THREE) || SixenseInput.Controllers [right].GetButtonDown (SixenseButtons.FOUR)) {
			pressedr = true;
		}
		
		if (SixenseInput.Controllers [right].GetButtonUp (SixenseButtons.ONE) || SixenseInput.Controllers [right].GetButtonUp (SixenseButtons.TWO) ||
		    SixenseInput.Controllers [right].GetButtonUp (SixenseButtons.THREE) || SixenseInput.Controllers [right].GetButtonUp (SixenseButtons.FOUR)) {
			pressedr = false;
		}
		
		if (rTrig >= 0.8 && pressedr == false ) {
			//display next ui
			//turn off previous ui
			previousr = true;
			print("shot R");
			
		}

		if (rTrig >= 0.8 && pressedr == true && previousr == true ) {

			print("shot R pressed");
			//turn off all UI
			tutorialNumber = 5;
		}

	}

	void finalStep(){
		print("last step active");
		if (SixenseInput.Controllers[right].GetButtonDown (SixenseButtons.START)){
			print ("done");
			//test it out
			tutorialPass = true;
		}
	}
}
