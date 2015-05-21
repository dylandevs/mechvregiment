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
	public GameObject tutorialZone;
	public GameObject[] dummies;

	public GameObject movementMenu;
	public GameObject statsMenu;
	public GameObject plasmaShotMenu;
	public GameObject AIMenu;
	public GameObject miniGunShot;
	public GameObject meteorShotMenu;
	public GameObject pressStartLeftCont;
	public GameObject tutShading;
	public GameObject[] allMenus;

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
	bool moveDone;

	
	bool pressed;
	bool previous;
	bool pressedr;

	float life = 0;
	float lTrig;
	float rTrig;
	float timer;

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
		mechShoot.connected = false;
		timer = 5;
	}
	
	// Update is called once per frame
	void Update () {
		
		if (Input.GetKey (KeyCode.R)) {
			tutorialPass = true;
			readyToGo = true;

		}

		lTrig = SixenseInput.Controllers[left].Trigger;
		rTrig = SixenseInput.Controllers[right].Trigger;
		
		if (Input.GetKey(KeyCode.S)) {

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

			allMenusOff();

			if (remainingTime > 0) {
				remainingTime = Mathf.Max (0, remainingTime - Time.deltaTime);
			
				string minutes = Mathf.Floor (remainingTime / 60).ToString ();
				string seconds = Mathf.Floor (remainingTime % 60).ToString ();
			
				if (remainingTime <= 60 && !noTimePlayed) {
					noTimeSound.Play ();
					noTimePlayed = true;
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
				if(tutShading.GetActive()==true){
					tutShading.SetActive(false);
				}
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
		tutorialZone.SetActive (false);
		foreach(GameObject j in dummies){
			j.SetActive(false);
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

		mechShoot.allowedToShootGame = true;
		movement.allowedToMove = true;

		if (timer >= 0) {
			movementMenu.SetActive (true);
		} 
		else if (timer <= 0) {
			movementMenu.SetActive(false);
		}

		if (timer >= 0) {
			timer-= Time.deltaTime;
		}

		if (timer <= 0) {
			moveDone = true;
		}

		if (tutorialNumber == 1 && moveDone == true) {
				statsMenu.SetActive(true);
			mapfunction();
		}

		if (tutorialNumber == 2) {
			healthFunction();
		}

		if (tutorialNumber == 3) {

			statsMenu.SetActive(false);

			leftHandShooting();
		}

		if (tutorialNumber == 4) {

			AIMenu.SetActive(false);

			rightHandShooting();
		}

		if (tutorialNumber == 5) {
			previousr = false;
			meteorShotMenu.SetActive(false);

			pressStartLeftCont.SetActive(true);

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
		

		//fires the ray and gets hit info while ognoring layer 14 well it's supposed to
		if (Physics.Raycast (minMode, out minHit, 75)) {
			if (minHit.collider.tag == "miniMap") {
				tutorialNumber = 2;
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

		if(statsMenu.GetActive() == true){
			statsMenu.SetActive(false);
		}

		if(plasmaShotMenu.GetActive() == false){
			plasmaShotMenu.SetActive(true);
		}

		if (previous == true) {
			if(AIMenu.GetActive()==false){
				AIMenu.SetActive(true);
			}
				plasmaShotMenu.SetActive(false);
		}

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
			//display next ui
			//turn off previous ui
			previous = true;

		}
		if (lTrig >= 0.8 && pressed == true && previous == true ) {
			//turn off all UI
			tutorialNumber = 4;
		}
	}

	void rightHandShooting(){

		if(miniGunShot.GetActive() == false){
			miniGunShot.SetActive(true);
		}
		
		if (previousr == true) {

			meteorShotMenu.SetActive(true);
				miniGunShot.SetActive(false);

		}

		miniGunArm.transform.localEulerAngles = new Vector3(250,12,2);
		cannonArm.transform.localEulerAngles =  new Vector3(250,12,354);
		
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
			
		}

		if (rTrig >= 0.8 && pressedr == true && previousr == true ) {
			//turn off all UI
			tutorialNumber = 5;
		}

	}

	void finalStep(){
		if (SixenseInput.Controllers[left].GetButtonDown (SixenseButtons.START)){
			//test it out
			tutorialPass = true;
		}
	}

	void allMenusOff(){
		foreach(GameObject t in allMenus){
			t.SetActive(false);
		}
	}
}
