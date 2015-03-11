using UnityEngine;
using System.Collections;

public class mechMovement : MonoBehaviour {

	//game objects and positions
	public GameObject bottomHalf;
	public GameObject topHalfX;
	public GameObject topHalfY;
	public GameObject miniMapCam;
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

	// Use this for initialization
	void Start () {
		mechHealth = 1000;
		currMechHealth = 1000;
		mechShield = 200;
		shieldActive = true;
		moveSpeedY = 15 * Time.deltaTime;
		rotSpeedY = Time.deltaTime * 80;

		moveSpeedX = 15 * Time.deltaTime;
		rotSpeedX = Time.deltaTime * 100;
	}
	
	// Update is called once per frame
	void Update () {

		//Updating the joystick input
		lStickX = SixenseInput.Controllers[left].JoystickX;
		lStickY = SixenseInput.Controllers[left].JoystickY;
		
		rStickX = SixenseInput.Controllers[right].JoystickX;
		rStickY = SixenseInput.Controllers[right].JoystickY;


		bool isMoving = false;

		if(mechHealth >=1){

			//Hydra Movement
			bottomHalf.transform.Translate(Vector3.forward* moveSpeedY * lStickY,Space.Self);
			bottomHalf.transform.Translate(Vector3.right * moveSpeedX * lStickX,Space.Self);

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

			Quaternion currRot = topHalfY.transform.localRotation;
			Vector3 nextRot = currRot.eulerAngles;

			if (nextRot.x >= 180){
				nextRot.x = nextRot.x - 360;
			}

			float nextRotX = nextRot.x + (rotSpeedY * -rStickY);
			if (nextRotX <= 30 && nextRotX >= -30) {
				topHalfY.transform.localRotation = Quaternion.Euler(nextRotX,nextRot.y,0);
				//print ("I am turning around by " + -1*lStickY);
			}

			print (nextRotX);
			
			/*Quaternion currRotY = topHalfX.transform.localRotation;
			Vector3 nextRotY = currRot.eulerAngles;
			float nextRotYY = nextRotY.y + (rotSpeedX * rStickX);
				
			print(nextRotYY);

			topHalfX.transform.localRotation = Quaternion.Euler(nextRotY.x,nextRotYY,0);*/

			topHalfX.transform.RotateAround(topHalfX.transform.position, Vector3.up, (rotSpeedX * rStickX));

			//print ("I am looking updown by " + -1*lStickX);


		}
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

		damagedTime += Time.deltaTime;

		if(currMechHealth > 0 && currMechHealth < 1000 && shieldActive == false && damagedTime > 15){
			currMechHealth += Time.deltaTime;
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

	void takeDamage(float amount){

		damagedTime = 0;

		if(shieldActive == false){
			currMechHealth -= amount;
		}
		else{
			mechShield -= amount;
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
