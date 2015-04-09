using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class cockpitUI : MonoBehaviour {

	public MinigunFirer minigun;
	public mechMovement holoVars;
	public MechShoot mechShoot;
	
	public GameObject mechHolagram;
	public GameObject screens;
	public GameObject camPos;
	public GameObject objectivePointer;
	public GameObject flag;
	public GameObject diamondForObjective;
	public GameObject KillForObjective;

	private Transform[] playerAvatars;

	public Light cockPitLight;

	//Goliath minimap stuffs
	public GameObject [] miniMapIndicatorsList;
	public GameObject [] miniMapFlag;
	public GameObject[] disabledSparks;
	public GameObject[] sightedImage;

	public Image miniGunImage;
	public GameObject miniGunImageOutline;
	public GameObject overHeatedImage;
	public Image cannonReload;
	public GameObject cannonReloadOutline;
	public GameObject missleImageOutline;
	public Image missleImage;
	public Image minionModeImage;

	public Image healthBar;
	public Image ShieldBar;
	public Image dashBar;

	GameObject player;

	float coolDown;
	float mechShield;
	float currMechHealth;
	float overHeatedTimer;
	float cannonCDR;
	float coolDownRocket;
	float shieldAmount; 
	float dashTimer;

	int playerNumber;
	//bools
	bool overHeated;
	bool shieldActive;
	bool flagTaken;
	bool mechHasFlag;
	bool dash;
	//modes
	bool minigunMode;
	bool missleMode;
	bool minionMode;

	public GoliathNetworking networker;
	// Use this for initialization
	void Start () {
		playerAvatars = new Transform[networker.playerAvatarWrapper.transform.childCount];
		for (int i = 0; i <networker.playerAvatarWrapper.transform.childCount; i++){
			playerAvatars[i] = networker.playerAvatarWrapper.transform.GetChild(i);
		}
	}

	
	// Update is called once per frame
	void Update () {
		//updating things from other scripts for cockpit ui
		minigunMode = mechShoot.miniGunMode;
		missleMode = mechShoot.rocketMode;
		minionMode = mechShoot.minionMode;
		mechHolagram.transform.Rotate(Vector3.up * Time.deltaTime * 8);	
		//change this to the same as the minigun filling thingy
		currMechHealth = holoVars.currMechHealth;
		mechShield = holoVars.mechShield;
		shieldAmount = holoVars.mechShield;
		dashTimer = holoVars.dashTimer;
		shieldActive = holoVars.shieldActive;
		mechHasFlag = mechShoot.carrying;

		updateObjectiveLocator();
		updateObjectiveDiamond();

		//healthbar stuff
		float fillAmountHealth = currMechHealth / 750;
		healthBar.fillAmount = fillAmountHealth;
		healthBar.color = Color.Lerp(Color.blue, Color.blue, fillAmountHealth);

		float fillAmountShield = mechShield / 1000;
		ShieldBar.fillAmount = fillAmountShield;
		ShieldBar.color = Color.Lerp(Color.red, Color.green, fillAmountShield);

		if (dash == true) {
			dashBar.fillAmount = 0;
		} else {
			float fillAmountDash = 1 - (dashTimer / 5);
			dashBar.fillAmount = fillAmountDash;
			dashBar.color = Color.Lerp (Color.red, Color.red, fillAmountDash);
		}


			for(int i = 0;i < playerAvatars.Length;i++){
					//turn the diamon to look at the camera in the mech
					Vector3 diamondLook = sightedImage[i].transform.position - camPos.transform.position;
					Vector3 newDir = Vector3.RotateTowards(transform.forward, diamondLook,100,100);
					sightedImage[i].transform.rotation =  Quaternion.LookRotation(newDir);

					Vector3 direction = (playerAvatars[i].position + Vector3.up) - camPos.transform.position ;
					float dist = direction.magnitude;
					
					sightedImage[i].transform.position = camPos.transform.position + direction.normalized * 5;
					
				if(dist <= 100){
					Ray ray = new Ray(camPos.transform.position,direction);
					RaycastHit[] hitInfoFire;

					hitInfoFire = Physics.RaycastAll(ray, dist);
					int j = 0;

					bool hitRet = false;
					bool hitTerrain = false;

					while (j < hitInfoFire.Length) {
						if(hitInfoFire[j].collider.tag == "Reticle"){
							hitRet = true;
						}

						if(hitInfoFire[j].collider.tag == "Terrain"){
							hitTerrain = true;
						}
							j++;
						}	

						if(hitTerrain == true && hitRet == true){
							Color tempColour = sightedImage[i].renderer.material.color;
							tempColour.a = 0.5f;
							sightedImage[i].renderer.material.color = tempColour;
						}
						
						if(hitRet == true && hitTerrain == false){
							Color tempColour = sightedImage[i].renderer.material.color;
							tempColour.a = 1f;
							sightedImage[i].renderer.material.color = tempColour;
						}

						else if(hitRet == false){
							Color tempColour = sightedImage[i].renderer.material.color;
							tempColour.a = 0f;
							sightedImage[i].renderer.material.color = tempColour;
						}
				}
					else if(dist > 100){
						
						Color tempColour = sightedImage[i].renderer.material.color;
						tempColour.a = 0f;
						sightedImage[i].renderer.material.color = tempColour;
					}

					
		    }

		//minigun mode graphics for weapons
		if(minigunMode == true){
			resetImages();
			miniGunImage.gameObject.SetActive(true);
			miniGunImageOutline.gameObject.SetActive(true);
			overHeatedImage.gameObject.SetActive(true);
			cannonReload.gameObject.SetActive(true);
			cannonReloadOutline.SetActive(true);

			//update the vars from the other script
			overHeated = minigun.overHeated;
			coolDown = minigun.overHeat;
			cannonCDR = minigun.cannonCDR;

			float fillAmount = coolDown / 150;
			miniGunImage.fillAmount = fillAmount;
			miniGunImage.color = Color.Lerp(Color.green, Color.red, fillAmount);

			//overheated image handling!
			if(overHeated == true){
				overHeatedTimer += Time.deltaTime;

				if(overHeatedTimer >= 0.1){
					overHeatedImage.SetActive(true);
				}
				if(overHeatedTimer >= 0.2){
					overHeatedImage.SetActive(false);
					overHeatedTimer = 0;
				}
			}
			else overHeatedImage.SetActive(false);

			//fill the cannon reload thing
			float fillAmountCannon =1-( cannonCDR/8);
			if(cannonCDR < 0){
				cannonReload.fillAmount = 1;
			}
			else cannonReload.fillAmount = fillAmountCannon;
		}

		//missle reload effect graphic
		if(missleMode == true){
			resetImages();

			missleImage.gameObject.SetActive(true);
			missleImageOutline.gameObject.SetActive(true);
			coolDownRocket = mechShoot.cooldownRemainingRocket;
			float fillAmountMissle = 1 - (coolDownRocket/30);

			if(coolDownRocket <= 0){
				missleImage.color = Color.green;
				missleImage.fillAmount = 1;
			}

			else{ 
				missleImage.color = Color.grey;
				missleImage.fillAmount = fillAmountMissle;
			}
		}

		//minion mode graphic
		if(minionMode == true){
			resetImages();
			minionModeImage.gameObject.SetActive(true);
		}

		//update the mech health hologram
		if(mechShield <=0){
			shieldActive = false;
		}
		
		if(shieldActive == true){
			mechHolagram.renderer.material.color = Color.blue;
			Color tempColour = mechHolagram.renderer.material.color;
			tempColour.a = 0.5f;
			mechHolagram.renderer.material.color = tempColour;

			cockPitLight.color = Color.white;
		}
		else if(currMechHealth > 0){
			float lerpAmnt = currMechHealth / 1000;
			mechHolagram.renderer.material.color = Color.Lerp(Color.red, Color.green, lerpAmnt);
			Color tempColour = mechHolagram.renderer.material.color;
			tempColour.a = 0.5f;
			mechHolagram.renderer.material.color = tempColour;

			screens.renderer.material.color = Color.green;
			Color screenTemp = screens.renderer.material.color;
			screenTemp.a = 0f;
			screens.renderer.material.color = screenTemp;
			cockPitLight.color = Color.white;
		}
		else if(currMechHealth <= 0){
			mechHolagram.renderer.material.color = Color.black;
			Color tempColour2 = mechHolagram.renderer.material.color;
			tempColour2.a = 0.5f;
			mechHolagram.renderer.material.color = tempColour2;

			//add the disabled schtuff and change window texture
			screens.renderer.material.color = Color.grey;
			Color screenTemp = screens.renderer.material.color;
			screenTemp.a = 0.5f;
			screens.renderer.material.color = screenTemp;

			//turn on sparks and shit then off and whatnot
			int i = Random.Range(0,4);
			disabledSparks[i].SetActive(true);
			cockPitLight.color = Color.red;
		}



	}//end of update

	void resetImages(){
		//fills
		miniGunImage.gameObject.SetActive(false);
		overHeatedImage.gameObject.SetActive(false);
		cannonReload.gameObject.SetActive(false);
		missleImage.gameObject.SetActive(false);
		minionModeImage.gameObject.SetActive(false);
		//outlines
		miniGunImageOutline.SetActive(false);
		cannonReloadOutline.SetActive(false);
		missleImageOutline.SetActive(false);
	}

	public void miniMapIndicators(int indicatorNum){
		//indicatorNum = indicatorNum -1;
		float dist = Vector3.Distance(transform.position,miniMapIndicatorsList[indicatorNum].transform.position);

		if(dist < 200){
			if(miniMapIndicatorsList[indicatorNum].GetActive() != true){
				miniMapIndicatorsList[indicatorNum].SetActive(true);
			}
			else{
				miniMapIconScript miniMap = miniMapIndicatorsList[indicatorNum].GetComponent<miniMapIconScript>();
				miniMap.life = 0.5f;
			}
		}
		//check distance between to see if he is ont he minimap.
		else{
			placeDirectionIndicator(miniMapIndicatorsList[indicatorNum]);
		}
	}

	void placeDirectionIndicator(GameObject direction){

		float amountFromForward = Vector3.Angle(direction.transform.position,transform.forward);

	}

	public void switchToFlag(int playerNum){
		flagTaken = true;
		playerNumber = playerNum;
		miniMapIndicatorsList[playerNum].SetActive(false);
		miniMapFlag[playerNum].SetActive(true);
	}

	public void droppedFlag(int playerNum){
		flagTaken = false;
		miniMapFlag[playerNum].SetActive(false);
	}

	void updateObjectiveLocator(){
		if(flagTaken == true){

			if(objectivePointer.GetActive() == false){
				objectivePointer.SetActive(true);
			}

			Vector3 playerPoint = miniMapIndicatorsList[playerNumber].transform.position;
			playerPoint.y = objectivePointer.transform.position.y;
			Vector3 targetDir = playerPoint - objectivePointer.transform.position;

			objectivePointer.transform.forward = targetDir;
		}
		else if(mechHasFlag == true){
			objectivePointer.SetActive(false);
		}
		else{

			if(objectivePointer.GetActive() == false){
				objectivePointer.SetActive(true);
			}
			Vector3 flagPoint = flag.transform.position;
			flagPoint.y = objectivePointer.transform.position.y;
			Vector3 targetDir = flagPoint - objectivePointer.transform.position;
			objectivePointer.transform.forward = targetDir;
		}
	}

	public void updateObjectiveDiamond(GameObject playerWithFlag = null){
		if(playerWithFlag!= null){
			 player = playerWithFlag;
		}

		if(flagTaken == true){	

			diamondForObjective.SetActive(false);
			KillForObjective.SetActive(true);

			Vector3 diamondLook = player.transform.position - camPos.transform.position;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, diamondLook,100,100);
			KillForObjective.transform.rotation =  Quaternion.LookRotation(newDir);
			
			Vector3 direction = (player.transform.position + Vector3.up) - camPos.transform.position ;
			
			KillForObjective.transform.position = camPos.transform.position + direction.normalized * 5;
		}
		else if(mechHasFlag == true){
			KillForObjective.SetActive(false);
			diamondForObjective.SetActive(false);
		}
		else{

			diamondForObjective.SetActive(true);
			KillForObjective.SetActive(false);
			Vector3 diamondLook = flag.transform.position - camPos.transform.position;
			Vector3 newDir = Vector3.RotateTowards(transform.forward, diamondLook,100,100);
			diamondForObjective.transform.rotation =  Quaternion.LookRotation(newDir);
			
			Vector3 direction = (flag.transform.position + Vector3.forward) - camPos.transform.position ;

			diamondForObjective.transform.position = camPos.transform.position + direction.normalized * 5;

		}
	}
}
