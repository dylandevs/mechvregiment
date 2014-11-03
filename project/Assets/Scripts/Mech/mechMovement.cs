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

	//move speed stuff
	private float moveSpeed;
	private float rotSpeed;

	// Use this for initialization
	void Start () {
		mechHealth = 100;
		currMechHealth = 100;
		moveSpeed = 5 * Time.deltaTime;
		rotSpeed = Time.deltaTime * 50;
	}
	
	// Update is called once per frame
	void Update () {

		bool isMoving = false;
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

		// when mech is disabled start timer to restart
		if(currMechHealth <=0){
			restartTimer += Time.deltaTime;
		}

		if(restartTimer >= 5){
			currMechHealth = mechHealth;
		}
		//do rotations
		//limit rotations so that mech stops looking down at a certain point
		if (transform.rotation.x <= 25) {
			if (Input.GetKey (KeyCode.DownArrow)) {
					topHalf.transform.Rotate (topHalf.transform.right * rotSpeed, Space.World);

			}
		}
		if (transform.rotation.x >= -25) {
			if (Input.GetKey (KeyCode.UpArrow)) {
					topHalf.transform.Rotate (-topHalf.transform.right * rotSpeed, Space.World);
			}
		}
		if (Input.GetKey (KeyCode.RightArrow)) {
			topHalf.transform.Rotate(Vector3.up*rotSpeed,Space.World);
		}
		
		if (Input.GetKey (KeyCode.LeftArrow)) {
			topHalf.transform.Rotate(-Vector3.up*rotSpeed,Space.World);	
		}


		topDir = topHalf.transform.eulerAngles;
		bottomDir = bottomHalf.transform.eulerAngles;
	
		if (isMoving == false) {
			float topDirY = topDir.y;
			float bottomDirY = bottomDir.y;
			float q = topDirY - bottomDirY;
			bottomHalf.transform.Rotate(0,q,0);
		}
		

	}

	void FixedUpdate(){
		Vector3 newPos = bottomHalf.transform.position;
		newPos += new Vector3 (0,3,0);
		topHalf.transform.position = newPos;
	}
}
