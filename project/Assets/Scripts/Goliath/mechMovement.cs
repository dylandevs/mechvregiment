using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class mechMovement : MonoBehaviour {
	//cameras
	public GameObject oculus;
	public GameObject tvCam;

	//game objects and positions
	public GameObject bottomHalf;
	public GameObject topHalfX;
	public GameObject topHalfY;
	public GameObject miniMapCam;
	public GameObject locatorLocation; 

	//for taking damage
	public GameObject damageIndicatorLeft;
	public GameObject damageIndicatorRight;
	public GameObject templeShield;
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
	public GameObject chargeEffects;
	//move speed stuff
	private float moveSpeedY;
	private float rotSpeedY;
	private float moveSpeedX;
	private float rotSpeedX;

	//hydra variables
	const int left = 0;
	const int right = 1;

	public bool forceKeyboard = false;
	public bool allowedToMove;
	
	public GoliathNetworking networker;
	public PoolManager damageMiniMap;

	float lStickX; 
	float lStickY;
	float rStickX;
	float rStickY;
	float damageTurnOffRight;
	float damageTurnOffLeft;
	float dashSpeed;
	float dashTimer;


	bool allowedToMoveRay;
	bool dash;

	// Use this for initialization
	void Start () {
		mechHealth = 1000;
		currMechHealth = 1000;
		mechShield = 500;
		shieldActive = true;
		moveSpeedY = 10;
		rotSpeedY = 1.5f;
		dashSpeed = 20;
		moveSpeedX = 7.5f;
		rotSpeedX = 2f;
	}
	
	// Update is called once per frame
	void Update () {

		groundingCast();

		if(damageTurnOffLeft > 0){
			damageTurnOffLeft -= Time.fixedDeltaTime;
		}
		if(damageTurnOffRight > 0){
			damageTurnOffRight -= Time.fixedDeltaTime;
		}

		if(damageTurnOffRight <= 0){

			damageIndicatorRight.SetActive(false);
		}

		if(damageTurnOffLeft <= 0){
			damageIndicatorLeft.SetActive(false);
		}

		if (SixenseInput.Controllers[left] != null && !forceKeyboard){
			//Updating the joystick input
			if(allowedToMove == true && allowedToMoveRay == true){
				lStickX = SixenseInput.Controllers[left].JoystickX;
				lStickY = SixenseInput.Controllers[left].JoystickY;
				
				rStickX = SixenseInput.Controllers[right].JoystickX;
				rStickY = SixenseInput.Controllers[right].JoystickY;
			}
			else{
				lStickX = 0;
				rStickX = 0;
				lStickY = 0;
				rStickY = 0;
			}
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

		//coutns down until next dash
		if(dashTimer >=0){
			dashTimer -= Time.deltaTime;
		}
		//dashing stuff
		if(SixenseInput.Controllers[left].GetButtonDown(SixenseButtons.JOYSTICK)){
			if(dashTimer <= 0){
				networker.photonView.RPC ("GoliathDashingStart",PhotonTargets.All);
				dash = true;
				triggerFlagDropThing.dash = true;
				chargeEffects.SetActive(true);
			}
		}

		if(SixenseInput.Controllers[left].GetButtonUp(SixenseButtons.JOYSTICK)){
			if(dashTimer <=0){
				dashTimer = 5f;
			}
			networker.photonView.RPC ("GoliathDashingEnd",PhotonTargets.All);
			chargeEffects.SetActive(false);
			dash = false;
			triggerFlagDropThing.dash = false;
		}


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
		if (nextRotX <= 4 && nextRotX >= -30 && dash == false) {
			topHalfY.transform.localRotation = Quaternion.Euler(nextRotX,nextRot.y,0);
		}

		//not able to turn when dashing
		if(dash == false){

			topHalfX.transform.RotateAround(topHalfX.transform.position, Vector3.up, (rotSpeedX * rStickX));
		}

		if(currMechHealth >=0){
			Vector3 currVel = bottomHalf.rigidbody.velocity;

			if(allowedToMoveRay == true){
				currVel.x = 0;
				currVel.z = 0;
			}

			if (lStickY != 0 && dash == false){
				Vector3 velMod = bottomHalf.transform.forward * moveSpeedY * lStickY;
				currVel += velMod;
			}

			if (dash == true){
				//should set the rotation to math where your looking
				float topDirY = topDir.y;
				float bottomDirY = bottomDir.y;
				float q = topDirY - bottomDirY;
				bottomHalf.transform.Rotate(0,q,0);

				Vector3 velMod = bottomHalf.transform.forward * dashSpeed;
				currVel += velMod;
			}

			if (lStickX != 0 && dash == false){
				Vector3 velMod = bottomHalf.transform.right * moveSpeedX * lStickX;
				currVel += velMod;
			}
			if (lStickX != 0 && dash == true){
				Vector3 velMod = bottomHalf.transform.right * 0 * lStickX;
				currVel += velMod;
			}
			if(allowedToMoveRay == true){
				rigidbody.velocity = currVel;
			}
			//Hydra Movement
			//bottomHalf.rigidbody.MovePosition(bottomHalf.rigidbody.position + bottomHalf.transform.forward * moveSpeedY * lStickY);

			//bottomHalf.rigidbody.MovePosition(bottomHalf.rigidbody.position + bottomHalf.transform.right * moveSpeedY * lStickX);

			//bottomHalf.transform.Translate(Vector3.forward* moveSpeedY * lStickY,Space.Self);
			//bottomHalf.transform.Translate(Vector3.right * moveSpeedX * lStickX,Space.Self);
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
				triggerFlagDropThing.carrying = false;
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

		miniMapDamage(direction);

		damagedTime = 10;

		if(shieldActive == true){
			if(mechShield <= 0){
				networker.photonView.RPC("BrokenShield",PhotonTargets.All);
				templeShield.SetActive(false);
				shieldActive = false;
			}
		}
		if(shieldActive == false){
			currMechHealth -= amount;
		}
		else{
			mechShield -= amount;
		}

		//damage direction indicators calculations
		float amountFromForward = Vector3.Angle(direction,topHalfX.transform.forward);
		float amountFromRight = Vector3.Angle(direction,topHalfX.transform.right);
		float amountFromLeft = Vector3.Angle(direction,topHalfX.transform.right * -1);

		if(amountFromRight < amountFromLeft){
			// its on the left side
			damageIndicatorLeft.SetActive(true);
			damageTurnOffLeft = 0.5f;
		}
		else if(amountFromForward > 150f && amountFromForward < 180f){
			//shownon of them
		}
		else{
			//its ont he right side
			damageIndicatorRight.SetActive(true);
			damageTurnOffRight= 0.5f;
		}

	}

	void miniMapDamage(Vector3 direction){
		GameObject damageMini = damageMiniMap.Retrieve(locatorLocation.transform.position);
		//Vector3 targetDir = direction - topHalfX.transform.position;

		//print(targetDir);

		damageMini.transform.forward = -direction;
	}

	void FixedUpdate(){
		//aligns the body top and bottom half
		Vector3 newPos = bottomHalf.transform.position;
		newPos += new Vector3 (0,1f,0);
		topHalfX.transform.position = newPos;

		//update minimap
		Vector3 newPosCam = bottomHalf.transform.position + new Vector3(0,150,0);
		miniMapCam.transform.position = newPosCam;
		Quaternion camRot = Quaternion.Euler(90,0,0);
		miniMapCam.transform.rotation = camRot;
	}

	void groundingCast(){

		Vector3 colliderBounds = collider.bounds.extents;

		//add half on x and negative half on x then half on z and negative half on z
		Vector3 xPosPos = bottomHalf.transform.position + new Vector3 (colliderBounds.x * 0.5f,0,0);
		Vector3 xPosNeg = bottomHalf.transform.position + new Vector3 (colliderBounds.x * -0.5f,0,0);
		Vector3 zPosPos = bottomHalf.transform.position + new Vector3 (0,0,colliderBounds.z * 0.5f);
		Vector3 zPosNeg = bottomHalf.transform.position + new Vector3 (0,0,colliderBounds.z * -0.5f);

		Ray downRayX1 = new Ray(xPosPos,bottomHalf.transform.up * -1);
		RaycastHit rayHitX1;

		if(Physics.Raycast (downRayX1, out rayHitX1,4f)){
			if(rayHitX1.collider.gameObject.tag == "Terrain"){
				allowedToMoveRay = true;
			}
		}
		else{
			allowedToMoveRay = false;
		}

		Ray downRayX2 = new Ray(xPosNeg,bottomHalf.transform.up * -1);
		RaycastHit rayHitX2;

		if(Physics.Raycast (downRayX2, out rayHitX2,4f)){
			if(rayHitX2.collider.gameObject.tag == "Terrain"){
				allowedToMoveRay = true;
			}
		}
		else{
			allowedToMoveRay = false;
		}

		Ray downRayZ1 = new Ray(zPosPos,bottomHalf.transform.up * -1);
		RaycastHit rayHitZ1;
		
		if(Physics.Raycast (downRayZ1, out rayHitZ1,4f)){
			if(rayHitZ1.collider.gameObject.tag == "Terrain"){
				allowedToMoveRay = true;
			}
		}
		else{
			allowedToMoveRay = false;
		}
		
		Ray downRayZ2 = new Ray(zPosNeg,bottomHalf.transform.up * -1);
		RaycastHit rayHitZ2;

		if(Physics.Raycast (downRayZ2, out rayHitZ2,4f)){
			if(rayHitZ2.collider.gameObject.tag == "Terrain"){
				allowedToMoveRay = true;
			}
		}
		else{
			allowedToMoveRay = false;
		}


		//straightDownCast
		Ray downRay = new Ray(bottomHalf.transform.position,bottomHalf.transform.up * -1);
		RaycastHit rayHit;

		if(Physics.Raycast (downRay, out rayHit,4f)){
			if(rayHit.collider.gameObject.tag == "Terrain"){
				allowedToMoveRay = true;
			}
		}
		else{
			allowedToMoveRay = false;
		}

	}

	void OnTriggerEnter(Collider collider){
		if (collider.tag == "Player"){
			PlayerAvatar avatarScript = collider.transform.parent.GetComponent<PlayerAvatar>();
			networker.photonView.RPC ("LaunchPlayer", PhotonTargets.All, avatarScript.PlayerNum);
		}
	}
}
