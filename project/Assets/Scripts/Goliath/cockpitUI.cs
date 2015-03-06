using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class cockpitUI : MonoBehaviour {

	public MinigunFirer minigun;
	public mechMovement holoVars;
	public MechShoot mechShoot;

	public GameObject mechHolagramB;
	public GameObject mechHolagramG;
	public GameObject mechHolagramY;
	public GameObject mechHolagramR;
	public GameObject mechHologramDisabled;
	public GameObject templeShield;

	public Image miniGunImage;
	public Image overHeatedImage;
	public Image cannonReload;
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
		//mode checks
		minigunMode = mechShoot.miniGunMode;
		missleMode = mechShoot.rocketMode;
		minionMode = mechShoot.minionMode;

		if(minigunMode == true){
			resetImages();

			miniGunImage.gameObject.SetActive(true);
			overHeatedImage.gameObject.SetActive(true);
			cannonReload.gameObject.SetActive(true);

			//update the vars from the other script
			overHeated = minigun.overHeated;
			coolDown = minigun.overHeat;
			cannonCDR = minigun.cannonCDR;

			float fillAmount = coolDown / 75;
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

		if(missleMode == true){
			resetImages();

			missleImage.gameObject.SetActive(true);
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

		if(minionMode == true){
			resetImages();
			minionModeImage.gameObject.SetActive(true);
		}

		//update the mech health hologram
		if(mechShield <=0){
			shieldActive = false;
			templeShield.SetActive(false);
			mechHolagramB.SetActive(false);
		}
		
		if(shieldActive == true){
			mechHolagramB.SetActive(true);
		}
		else if(currMechHealth >=501){
			mechHologramDisabled.SetActive(false);
			mechHolagramG.SetActive(true);
		}
		else if(currMechHealth <= 500 && currMechHealth >=251){
			mechHolagramG.SetActive(false);
			mechHolagramY.SetActive(true);
		}
		else if(currMechHealth <=250 && currMechHealth >=1){
			mechHolagramY.SetActive(false);
			mechHolagramR.SetActive(true);
		}
		else if(currMechHealth <= 0){
			mechHolagramR.SetActive(false);
			mechHolagramG.SetActive(false);
			mechHolagramY.SetActive(false);
			mechHologramDisabled.SetActive(true);
			//turn on whatever other graphics are going to show when disabled
		}



	}

	void resetImages(){
		miniGunImage.gameObject.SetActive(false);
		overHeatedImage.gameObject.SetActive(false);
		cannonReload.gameObject.SetActive(false);
		missleImage.gameObject.SetActive(false);
		minionModeImage.gameObject.SetActive(false);
	}
}
