using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class cockpitUI : MonoBehaviour {

	public MinigunFirer minigun;
	public mechMovement holoVars;
	public MechShoot mechShoot;
	
	public GameObject mechHolagram;
	public GameObject templeShield;
	public GameObject screens;
	public Light cockPitLight;

	//Goliath minimap stuffs
	public GameObject [] miniMapIndicatorsList;
	public GameObject[] disabledSparks;

	public Image miniGunImage;
	public Image miniGunImageOutline;
	public Image overHeatedImage;
	public Image cannonReload;
	public Image cannonReloadOutline;
	public Image missleImageOutline;
	public Image missleImage;
	public Image minionModeImage;

	float coolDown;
	float mechShield;
	float currMechHealth;
	float overHeatedTimer;
	float cannonCDR;
	float coolDownRocket;

	//bools
	bool overHeated;
	bool shieldActive;

	//modes
	bool minigunMode;
	bool missleMode;
	bool minionMode;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {

		minigunMode = mechShoot.miniGunMode;
		missleMode = mechShoot.rocketMode;
		minionMode = mechShoot.minionMode;
		mechHolagram.transform.Rotate(Vector3.up * Time.deltaTime * 8);

		//minigun mode graphics for weapons
		if(minigunMode == true){
			resetImages();
			miniGunImage.gameObject.SetActive(true);
			miniGunImageOutline.gameObject.SetActive(true);
			overHeatedImage.gameObject.SetActive(true);
			cannonReload.gameObject.SetActive(true);
			cannonReloadOutline.gameObject.SetActive(true);

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
					overHeatedImage.fillAmount = 0;
				}
				if(overHeatedTimer >= 0.2){
					overHeatedImage.fillAmount = 1;
					overHeatedTimer = 0;
				}
			}
			else overHeatedImage.fillAmount = 0;

			//fill the cannon reload thing
			float fillAmountCannon =1-( cannonCDR/8);
			if(cannonCDR < 0){
				cannonReload.fillAmount = 1;
			}
			else cannonReload.fillAmount = fillAmountCannon;
			//change this to the same as the minigun filling thingy
			currMechHealth = holoVars.currMechHealth;
			mechShield = holoVars.mechShield;
			shieldActive = holoVars.shieldActive;
		}

		//missle reload effect graphic
		if(missleMode == true){
			resetImages();

			missleImage.gameObject.SetActive(true);
			missleImageOutline.gameObject.SetActive(true);
			coolDownRocket = mechShoot.cooldownRemainingRocket;
			float fillAmountMissle = 1 - (coolDownRocket/10);

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
			templeShield.SetActive(false);
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
		miniGunImageOutline.gameObject.SetActive(false);
		cannonReloadOutline.gameObject.SetActive(false);
		missleImageOutline.gameObject.SetActive(false);
	}

	public void miniMapIndicators(int indicatorNum){
		float dist = Vector3.Distance(transform.position,miniMapIndicatorsList[indicatorNum].transform.position);

		if(dist < 50){
			if(miniMapIndicatorsList[indicatorNum].GetActive() != true){
				miniMapIndicatorsList[indicatorNum].SetActive(true);
			}
			else{
				miniMapIconScript miniMap = miniMapIndicatorsList[indicatorNum].GetComponent<miniMapIconScript>();
				miniMap.life = 3;
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

	public void turnOffIndicator(){

	}
}
