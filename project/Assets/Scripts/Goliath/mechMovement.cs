using UnityEngine;
using System.Collections;

public class mechMovement : MonoBehaviour {

	//game objects and positions
	public GameObject bottomHalf;
	public GameObject topHalfX;
	public GameObject topHalfY;
	public GameObject miniMapCam;

	//for taking damage
	public GameObject damageIndicatorLeft;
	public GameObject damageIndicatorRight;


	public MechShoot triggerFlagDropThing;

	public Vector3 topDir;
	public Vector3 bottomDir;

	//mech health stuff
	public float currMechHealth;
	private float damagedTime;
	private float mechHealth;
	private float restartTimer;

	//shield things
	public float mechShield;
	public bool shieldActive;

	//move speed stuff
	private float moveSpeedY;
	private float rotSpeedY;
	private float moveSpeedX;
	private float rotSpeedX;

	//hydra variables
	const int left = 0;
	const int right = 1;

	float lStickX; 
	float lStickY;
	float rStickX;
	float rStickY;
	float damageTurnOff;

	// Use this for initialization
	void Start () {
		mechHealth = 1000;
		currMechHealth = 1000;
		mechShield = 200;
		shieldActive = true;
		moveSpeedY = 10;
		rotSpeedY = 1.5f;

		moveSpeedX = 7.5f;
		rotSpeedX = 2f;
	}
	
	// Update is called once per frame
	void Update () {

		if(damageTurnOff > 0){
			damageTurnOff -= Time.fixedDeltaTime;
		}

		if(damageTurnOff <=0){
			damageIndicatorLeft.SetActive(false);
			damageIndicatorRight.SetActive(false);
		}

		if (SixenseInput.Controllers[left] != null){
			//Updating the joystick input
			lStickX = SixenseInput.Controllers[left].JoystickX;
			lStickY = SixenseInput.Controllers[left].JoystickY;
			
			rStickX = SixenseInput.Controllers[right].JoystickX;
			rStickY = SixenseInput.Controllers[right].JoystickY;
		}
		else{
			rStickX = 0;
			lStickY = 0;
			rStickY = 0;
			if (Input.GetKey(KeyCode.W)){
				lStickY = 1;
			}
			if (Input.GetKey(KeyCode.S)){
				lStickY = -1;
			}
			if (Input.GetKey(KeyCode.A)){
				rStickX = -1;
			}
			if (Input.GetKey(KeyCode.D)){
				rStickX = 1;
			}
			if (Input.GetKey(KeyCode.Q)){
				rStickY = -1;
			}
			if (Input.GetKey(KeyCode.E)){
				rStickY = 1;
			}
		}

		bool isMoving = false;
		Quaternion currRot = topHalfY.transform.localRotation;
		Vector3 nextRot = currRot.eulerAngles;

		//is moving calcs
		if(lStickX >= 0.05f){
			isMoving = true;
		}
		
		if(lStickY >= 0.05f){
			isMoving = true;
		}
		
		if(lStickX <= -0.05f){
			isMoving = true;
		}
		
		if(lStickY <= -0.05f){
			isMoving = true;
		}
		//rotations tuffs
		if (nextRot.x >= 180){
			nextRot.x = nextRot.x - 360;
		}

		//rotations
		float nextRotX = nextRot.x + (rotSpeedY * -rStickY);
		if (nextRotX <= 4 && nextRotX >= -30) {
			topHalfY.transform.localRotation = Quaternion.Euler(nextRotX,nextRot.y,0);
		}
		topHalfX.transform.RotateAround(topHalfX.transform.position, Vector3.up, (rotSpeedX * rStickX));

		if(currMechHealth >=0){
			Vector3 currVel = bottomHalf.rigidbody.velocity;
			currVel.x = 0;
			currVel.z = 0;

			if (lStickY != 0){
				Vector3 velMod = bottomHalf.transform.forward * moveSpeedY * lStickY;
				currVel += velMod;
			}

			if (lStickX != 0){
				Vector3 velMod = bottomHalf.transform.right * moveSpeedX * lStickX;
				currVel += velMod;
			}

			rigidbody.velocity = currVel;

			//Hydra Movement
			//bottomHalf.rigidbody.MovePosition(bottomHalf.rigidbody.position + bottomHalf.transform.forward * moveSpeedY * lStickY);

			//bottomHalf.rigidbody.MovePosition(bottomHalf.rigidbody.position + bottomHalf.transform.right * moveSpeedY * lStickX);

			//bottomHalf.transform.Translate(Vector3.forward* moveSpeedY * lStickY,Space.Self);
			//bottomHalf.transform.Translate(Vector3.right * moveSpeedX * lStickX,Space.Self);
		}
		else print("disabled");
		/*
		if(mechHealth >=1){
			//if a key is pushed move the location of the mech
			if (Input.GetKey (KeyCode.W)){
				bottomHalf.transform.Translate(transform.forward * moveSpeed);
				isMoving = true;
			}
			
			if (Input.GetKey (KeyCode.S)) {
				bottomHalf.transform.Translate(transform.forward * moveSpeed * -1);
				isMoving = true;
			}
			
			if (Input.GetKey (KeyCode.D)) {
				bottomHalf.transform.Translate(transform.right * moveSpeed);
				isMoving = true;
			}
			
			if (Input.GetKey (KeyCode.A)) {
				bottomHalf.transform.Translate(transform.right * moveSpeed * -1);
				isMoving = true;
			}
		}
		*/
		//do rotations
		//limit rotations so that mech stops looking down at a certain point
		/*
		if (topHalf.transform.eulerAngles.x <= 25||topHalf.transform.eulerAngles.x >= 180) {
			if (Input.GetKey (KeyCode.DownArrow)) {
				topHalf.transform.Rotate (topHalf.transform.right * rotSpeed, Space.World);
			}
		}

		if (topHalf.transform.eulerAngles.x < 180||topHalf.transform.eulerAngles.x >= 330) {
			if (Input.GetKey (KeyCode.UpArrow)) {
				topHalf.transform.Rotate (topHalf.transform.right * -rotSpeed, Space.World);
			}
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			topHalf.transform.Rotate(Vector3.up*rotSpeed,Space.World);
		}
		
		if (Input.GetKey (KeyCode.LeftArrow)) {
			topHalf.transform.Rotate(-Vector3.up*rotSpeed,Space.World);	
		}
		*/
		
		// when mech is disabled start timer to restart
		if(currMechHealth <=0){
			if(triggerFlagDropThing.carrying == true){
				triggerFlagDropThing.releaseFlag();
			}
			restartTimer += Time.deltaTime;
		}
		
		if(restartTimer >= 8){
			currMechHealth = mechHealth;
			restartTimer = 0;
		}
		if(damagedTime > 0){
			damagedTime += Time.deltaTime;
		}

		if(currMechHealth > 0 && currMechHealth < 1000 && shieldActive == false && damagedTime <= 0){
			currMechHealth += Time.deltaTime;
		}

		if(mechShield<= 0){
			shieldActive = false;
		}
		//match the top half to the bottom half when not moving
		topDir = topHalfX.transform.eulerAngles;
		bottomDir = bottomHalf.transform.eulerAngles;
	
		if (isMoving == false) {
			float topDirY = topDir.y;
			float bottomDirY = bottomDir.y;
			float q = topDirY - bottomDirY;
			bottomHalf.transform.Rotate(0,q,0);
		}
		

	}// End of update

	public void takeDamage(float amount,Vector3 direction){

		damageTurnOff = 0.5f;
		damagedTime = 10;

		if(shieldActive == false){
			currMechHealth -= amount;
		}
		else{
			mechShield -= amount;
		}

		float amountFromForward = Vector3.Angle(direction,topHalfX.transform.forward);
		float amountFromRight = Vector3.Angle(direction,topHalfX.transform.right);
		float amountFromLeft = Vector3.Angle(direction,topHalfX.transform.right * -1);
		if(amountFromRight < amountFromLeft){
			// its on the left side
			damageIndicatorLeft.SetActive(true);
		}
		else if(amountFromForward > 150f && amountFromForward < 180f){
			//shownon of them
		}
		else{
			//its ont he right side
			damageIndicatorRight.SetActive(true);
		}

	}

	void FixedUpdate(){
		//aligns the body top and bottom half
		Vector3 newPos = bottomHalf.transform.position;
		newPos += new Vector3 (0,1.5f,0);
		topHalfX.transform.position = newPos;

		//update minimap
		Vector3 newPosCam = bottomHalf.transform.position + new Vector3(0,30,0);
		miniMapCam.transform.position = newPosCam;
		Quaternion camRot = Quaternion.Euler(90,topHalfX.transform.localEulerAngles.y,0);
		miniMapCam.transform.rotation = camRot;
	}
}
