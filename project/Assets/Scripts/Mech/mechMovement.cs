using UnityEngine;
using System.Collections;

public class mechMovement : MonoBehaviour {

	//game objects and positions
	public GameObject bottomHalf;
	public GameObject topHalf;
	public Vector3 topDir;
	public Vector3 bottomDir;

	//mech health stuff
	public float currMechHealth;
	private float mechHealth;
	private float restartTimer;

	//public GameObject mechHolagram;

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
		mechHealth = 100;
		currMechHealth = 100;

		moveSpeedY = 5 * Time.deltaTime;
		rotSpeedY = Time.deltaTime * 50;

		moveSpeedX = 5 * Time.deltaTime;
		rotSpeedX = Time.deltaTime * 50;

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
			bottomHalf.transform.Translate(transform.forward * moveSpeedY * rStickY);
			bottomHalf.transform.Translate(transform.right * moveSpeedX * rStickX);

			//Hydra Rotaiton
			if (topHalf.transform.eulerAngles.x <= 25||topHalf.transform.eulerAngles.x >= 180) {
				topHalf.transform.Rotate(Vector3.right * rotSpeedY * -lStickY, Space.World);
			}

			// figure out what the frack is going on here
			topHalf.transform.Rotate (topHalf.transform.right * rotSpeedX * -lStickX, Space.World);


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
			restartTimer += Time.deltaTime;
		}
		
		if(restartTimer >= 5){
			currMechHealth = mechHealth;
		}
		//match the top half to the bottom half when not moving
		topDir = topHalf.transform.eulerAngles;
		bottomDir = bottomHalf.transform.eulerAngles;
	
		if (isMoving == false) {
			float topDirY = topDir.y;
			float bottomDirY = bottomDir.y;
			float q = topDirY - bottomDirY;
			bottomHalf.transform.Rotate(0,q,0);
		}
		

	}// End oaf update

	void FixedUpdate(){
		Vector3 newPos = bottomHalf.transform.position;
		newPos += new Vector3 (0,3,0);
		topHalf.transform.position = newPos;
	}
}
