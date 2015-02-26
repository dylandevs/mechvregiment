using UnityEngine;
using System.Collections;

public class cockpitUI : MonoBehaviour {

	public MinigunFirer minigun;
	public mechMovement holoVars;


	public GameObject mechHolagramB;
	public GameObject mechHolagramG;
	public GameObject mechHolagramY;
	public GameObject mechHolagramR;
	public GameObject mechHologramDisabled;
	public GameObject templeShield;

	float coolDown;
	float mechShield;
	float currMechHealth;
	bool shieldActive;



	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		//update the vars from the other script
		coolDown = minigun.overHeat;
		currMechHealth = holoVars.currMechHealth;
		mechShield = holoVars.mechShield;
		shieldActive = holoVars.shieldActive;

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
}
