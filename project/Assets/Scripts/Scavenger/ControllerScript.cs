/**
 * 
 * Basic player movement script
 * Manages lateral/longitudinal movement, camera movement, and jumping
 * 
 **/

using UnityEngine;
using System.Collections;
using XInputDotNetPure;

public class ControllerScript : MonoBehaviour {
	
	const float SprintSpeed = 15f;
	const float RunSpeed = 8f;
	const float CrouchSpeed = 1.5f;
	const float RunThresh = 0.5f;
	const float JumpSpeed = 6f;
	const float ADSSpeedFactor = 0.7f;
	const float CrouchSpeedFactor = 0.5f;
	const float StandHeight = 3.33f;
	const float CrouchHeight = 2.8f;

	private CapsuleCollider terrainCollider;

	public bool isKeyboard = false;
	public int controllerId = -1;
	[HideInInspector]
	public Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	[HideInInspector]
	public Vector3 perpFacing = new Vector3(1, 0, 0);
	[HideInInspector]
	public Vector3 cameraOffset = Vector3.zero;
	float speedFactor = 1;
	Vector3 initialSpineAngles;

	private float mainRad;
	private float margin;
	private float colliderRad;
	private float offset;

	// XInput variables
	private GamePadState state;
	private GamePadState prevState;

	// Inputs
	private GameObject playerCam;
	public Player player;
	private Animator anim;
	private Animator fpsAnim;
	public Animator cameraAnim;
	public Animator gunCamAnim;
	public GameObject spineJoint;
	
	// Animation hash id
	int fwdSpeedHash = Animator.StringToHash("FwdSpeed");
	int rgtSpeedHash = Animator.StringToHash("RgtSpeed");
	int speedHash = Animator.StringToHash("Speed");
	int fireHash = Animator.StringToHash("Firing");
	int sprintHash = Animator.StringToHash("Sprinting");
	int adsHash = Animator.StringToHash("Aiming");
	int jumpHash = Animator.StringToHash("Jump");
	int crouchHash = Animator.StringToHash ("Crouching");
	int weaponHash = Animator.StringToHash ("WeaponNum");
	int changeWeapHash = Animator.StringToHash ("ChangeWeapon");
	int flagCarryHash = Animator.StringToHash ("CarryFlag");
	int resetHash = Animator.StringToHash("Reset");


	// Keyboard trackers
	Vector2 deltaMousePos = Vector2.zero;

	// State trackers
	[HideInInspector]
	public bool isCrouching = false;
	[HideInInspector]
	public bool isSprinting = false;
	[HideInInspector]
	public bool aimingDownSight = false;
	private float adsHoldTime = 1;
	
	// Publicly accessible controller attributes
	[HideInInspector]
	public bool A_Press = false;
	[HideInInspector]
	public bool B_Press = false;
	[HideInInspector]
	public bool X_Press = false;
	[HideInInspector]
	public bool Y_Press = false;
	[HideInInspector]
	public bool DPad_Next = false;
	[HideInInspector]
	public bool DPad_Prev = false;
	[HideInInspector]
	public float R_XAxis = 0;
	[HideInInspector]
	public float R_YAxis = 0;
	[HideInInspector]
	public bool RS_Press = false;
	[HideInInspector]
	public float L_XAxis = 0;
	[HideInInspector]
	public float L_YAxis = 0;
	[HideInInspector]
	public bool LS_Held = false;
	[HideInInspector]
	public float TriggersR = 0;
	[HideInInspector]
	public float TriggersL = 0;
	[HideInInspector]
	public bool currentlyGrounded = false;

	// Publicly-accessible animation attributes
	[HideInInspector]
	public float forwardSpeed = 0;
	[HideInInspector]
	public float rightSpeed = 0;
	[HideInInspector]
	public float speed = 0;

	// Weapon swap variables
	private bool isSwapping = false;
	private float swapTime = 0;
	private float expectedSwapTime = 1;
	private int swapAdjustment = 0;

	// Flag pickup variables
	private bool flagInRange = false;
	[HideInInspector]
	public bool flagPickedUp = false;
	
	// Use this for initialization
	void Start () {
		playerCam = cameraAnim.gameObject;

		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
		cameraOffset = playerCam.transform.localPosition;

		initialSpineAngles = spineJoint.transform.localRotation.eulerAngles;

		anim = player.anim;
		fpsAnim = player.fpsAnim;
		terrainCollider = GetComponent<CapsuleCollider>();

		mainRad = terrainCollider.radius;
		margin = Mathf.Sqrt(2 * mainRad) - mainRad;
		colliderRad = Mathf.Sqrt(((mainRad + margin)/2) * ((mainRad + margin)/2) - (mainRad/2) * (mainRad/2));
		offset = (mainRad + margin) / 2;
	}
	
	// Update is called once per frame
	void Update () {
		// Updating attributes
		Vector3 newVel = GetComponent<Rigidbody>().velocity;
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;

		currentlyGrounded = IsGrounded();

		// Setting default attributes
		float spread = 0;
		speedFactor = 1;
		isSprinting = false;
		
		if (currentlyGrounded){
			newVel = new Vector3(0, GetComponent<Rigidbody>().velocity.y, 0);
		}
		
		Weapon currentWeapon = player.GetCurrentWeapon ();

		if (!isKeyboard){

			// Ignore input if unassigned
			if (controllerId == -1) {
				return;
			}
			else{
				state = GamePad.GetState((PlayerIndex)controllerId);

				// If controller becomes disconnected, stop
				if (!state.IsConnected){
					return;
				}
			}

			// Emergency fallback for ending swapping state
			if (isSwapping){
				swapTime -= Time.deltaTime;
				if (swapTime <= 0){
					isSwapping = false;
				}
			}
			else{
				swapTime = expectedSwapTime;
			}

			// Ignore all input if dead
			if (player.isDead || !player.game.GameRunning){
				A_Press = false;
				B_Press = false;
				X_Press = false;
				Y_Press = false;
				
				DPad_Next = false;
				DPad_Prev = false;
				
				R_XAxis = 0;
				R_YAxis = 0;
				RS_Press = false;
				
				L_XAxis = 0;
				L_YAxis = 0;
				LS_Held = false;
				
				TriggersR = 0;
				TriggersL = 0;
			}
			else{
			// Getting controller values
				A_Press = (state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released);
				B_Press = (state.Buttons.B == ButtonState.Pressed && prevState.Buttons.B == ButtonState.Released);
				X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
				Y_Press = (state.Buttons.Y == ButtonState.Pressed && prevState.Buttons.Y == ButtonState.Released);

				DPad_Next = (state.DPad.Right == ButtonState.Pressed && prevState.DPad.Right == ButtonState.Released);
				DPad_Prev = (state.DPad.Left == ButtonState.Pressed && prevState.DPad.Left == ButtonState.Released);
				
				R_XAxis = state.ThumbSticks.Right.X;
				R_YAxis = -state.ThumbSticks.Right.Y;
				RS_Press = (state.Buttons.RightStick == ButtonState.Pressed && prevState.Buttons.RightStick == ButtonState.Released);
				
				L_XAxis = state.ThumbSticks.Left.X;
				L_YAxis = -state.ThumbSticks.Left.Y;
				LS_Held = (state.Buttons.LeftStick == ButtonState.Pressed);
				
				TriggersR = state.Triggers.Right;
				TriggersL = state.Triggers.Left;
			}
			
			if (RS_Press){
				
			}

			if (currentlyGrounded){

				// Toggle crouching
				if (B_Press){
					SetCrouching(!isCrouching);
				}
				
				// Jumping
				if (A_Press && !flagPickedUp){
					newVel.y += JumpSpeed;
					
					anim.SetTrigger(jumpHash);
					fpsAnim.SetTrigger(jumpHash);
					
					// Cancel crouch
					SetCrouching(false);
					
					player.networkManager.photonView.RPC ("PlayerJump", PhotonTargets.All, player.initializer.Layer - 1);
				}
				
				// Lateral movement (strafing)
				if (L_XAxis != 0){
					if (Mathf.Abs(L_XAxis) > RunThresh){
						newVel += RunSpeed * perpFacing * SignOf(L_XAxis);
						spread += currentWeapon.RunSpreadAdjust;
					}
					else{
						newVel += CrouchSpeed * perpFacing * SignOf(L_XAxis);
						spread += currentWeapon.WalkSpreadAdjust;
					}
				}
				
				// Longitudinal movement
				if (L_YAxis != 0){
					
					// Sprint
					if (LS_Held && L_YAxis < RunThresh && !flagPickedUp){
						newVel += SprintSpeed * facing2D;
						spread += currentWeapon.SprintSpreadAdjust;
						isSprinting = true;
						
						// Cancel crouch
						SetCrouching(false);
					}
					// Run
					else if (Mathf.Abs(L_YAxis) > RunThresh){
						if (isCrouching){
							spread += currentWeapon.CrouchSpreadAdjust;
						}
						newVel += RunSpeed * facing2D * -SignOf(L_YAxis);
						spread += currentWeapon.RunSpreadAdjust;
					}
					// Walk
					else{
						if (isCrouching){
							spread += currentWeapon.CrouchSpreadAdjust;
						}
						newVel += Mathf.Lerp(0, RunSpeed, Mathf.Abs(L_YAxis)/RunThresh) * facing2D * -SignOf(L_YAxis);
						spread += currentWeapon.WalkSpreadAdjust;
					}
				}			
			}
			else{
				spread += currentWeapon.JumpSpreadAdjust;
			}

			if (!flagPickedUp){
				
				// Picking up flag
				if (X_Press && flagInRange && !isSwapping && !currentWeapon.IsReloading()){
					flagPickedUp = true;
					anim.SetBool(flagCarryHash, true);
					fpsAnim.SetBool(flagCarryHash, true);
					
					player.FlagRetrieved();
				}

				// Trigger change weapon
				if (!isSwapping){
					if (DPad_Next || Y_Press){
						swapAdjustment = 1;
						int nextWeaponIndex = player.GetExpectedWeaponIndex(swapAdjustment);

						anim.SetTrigger(changeWeapHash);
						anim.SetInteger(weaponHash, nextWeaponIndex);
						fpsAnim.SetTrigger(changeWeapHash);
						fpsAnim.SetInteger(weaponHash, nextWeaponIndex);

						isSwapping = true;
						swapTime = expectedSwapTime;
					}
					else if (DPad_Prev){
						swapAdjustment = -1;
						int nextWeaponIndex = player.GetExpectedWeaponIndex(swapAdjustment);

						anim.SetTrigger(changeWeapHash);
						anim.SetInteger(weaponHash, nextWeaponIndex);
						fpsAnim.SetTrigger(changeWeapHash);
						fpsAnim.SetInteger(weaponHash, nextWeaponIndex);

						isSwapping = true;
						swapTime = expectedSwapTime;
					}
				}

				// Reloading
				if (X_Press && !isSwapping && !flagPickedUp){
					player.TriggerReload();
				}

				// Toggle ADS
				if (TriggersL != 0 && !currentWeapon.IsReloading() && !isSprinting && !isSwapping) {
					if (currentlyGrounded){
						player.ToggleADS(true);
						fpsAnim.SetBool(adsHash, true);
						cameraAnim.SetBool(adsHash, true);
						gunCamAnim.SetBool(adsHash, true);
						anim.SetBool(adsHash, true);
						aimingDownSight = true;
						spread += currentWeapon.AdsSpreadAdjust;
						speedFactor *= ADSSpeedFactor;
					
						adsHoldTime = 1;
					}
					else{
						// Some tolerance for falling and holding ADS
						if (aimingDownSight){
							adsHoldTime -= Time.deltaTime;
							if (adsHoldTime <= 0){
								player.ToggleADS(false);
								fpsAnim.SetBool(adsHash, false);
								cameraAnim.SetBool(adsHash, false);
								gunCamAnim.SetBool(adsHash, false);
								anim.SetBool(adsHash, false);
								aimingDownSight = false;
							}
						}
					}
				}
				else{
					player.ToggleADS(false);
					fpsAnim.SetBool(adsHash, false);
					cameraAnim.SetBool(adsHash, false);
					gunCamAnim.SetBool(adsHash, false);
					anim.SetBool(adsHash, false);
					aimingDownSight = false;
				}

				// Firing script
				if (TriggersR != 0 && !isSwapping){
					player.SetFiringState(true);
				}
				else{
					player.SetFiringState(false);
				}
			}
			else{
				// Picking up flag
				if (X_Press){
					DropFlag();
				}
			}

			// Apply speed factor for crouching
			if (isCrouching){
				speedFactor *= CrouchSpeedFactor;
				spread += currentWeapon.CrouchSpreadAdjust;
			}

			// Rotation about Y axis
			if (R_XAxis != 0){
				float adjustment = R_XAxis * R_XAxis * SignOf(R_XAxis) * 5;
				
				// Slow movement for ADS
				if (aimingDownSight){
					adjustment *= 0.5f;
				}
				
				SetFacing(Quaternion.AngleAxis(adjustment, Vector3.up) * facing);
			}
			
			// Vertical tilt of camera
			if (R_YAxis != 0){
				float adjustment = R_YAxis * R_YAxis * SignOf(R_YAxis) * 5;
				
				// Slow movement for ADS
				if (aimingDownSight){
					adjustment *= 0.5f;
				}
				
				Vector3 newFacing = Quaternion.AngleAxis(adjustment, perpFacing) * facing;
				float vertAngle = Vector3.Angle(newFacing, Vector3.up);

				// Limit angle so straight up/down are not possible
				if (vertAngle >= 10 && vertAngle <= 170){
					SetFacing(newFacing);
				}
				else{
					// Do nothing
				}
			}
		}

		// Limited keyboard fallback
		else{
			Cursor.visible = false;

			// Getting key states
			bool Space_Down = Input.GetKeyDown(KeyCode.Space);
			bool Key_W = Input.GetKey(KeyCode.W);
			bool Key_A = Input.GetKey(KeyCode.A);
			bool Key_S = Input.GetKey(KeyCode.S);
			bool Key_D = Input.GetKey(KeyCode.D);
			bool Shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
			bool Mouse_Left = Input.GetMouseButton(0);
			bool Mouse_Right = Input.GetMouseButton(1);
			bool Ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
			deltaMousePos.x = Input.GetAxis("Mouse X");
			deltaMousePos.y = Input.GetAxis("Mouse Y");
			
			if (currentlyGrounded){
				
				// Jumping
				if (Space_Down){
					newVel.y += JumpSpeed;
				}
				
				// Lateral movement (strafing)
				if (Key_A){
					if (Ctrl){
						newVel += CrouchSpeed * -perpFacing;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * -perpFacing;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
				else if (Key_D){
					if (Ctrl){
						newVel += CrouchSpeed * perpFacing;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * perpFacing;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
				
				// Longitudinal movement
				if (Key_W){
					if (Shift){
						newVel += SprintSpeed * facing2D;
						spread += currentWeapon.SprintSpreadAdjust;
					}
					else if (Ctrl){
						newVel += CrouchSpeed * facing2D;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * facing2D;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
				else if(Key_S){
					if (Ctrl){
						newVel += CrouchSpeed * facing2D * -1;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * facing2D * -1;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
			}
			else{
				spread += currentWeapon.JumpSpreadAdjust;
			}
			
			// Toggle ADS
			if (Mouse_Right && currentlyGrounded && !currentWeapon.IsReloading()) {
				player.ToggleADS(true);
				//anim.SetBool(fireHash, true);
				fpsAnim.SetBool(adsHash, true);
				cameraAnim.SetBool(adsHash, true);
				gunCamAnim.SetBool(adsHash, true);
				anim.SetBool(adsHash, true);
				aimingDownSight = true;
				spread += currentWeapon.AdsSpreadAdjust;
				speedFactor *= ADSSpeedFactor;
			}
			else{
				player.ToggleADS(false);
				//anim.SetBool(fireHash, false);
				fpsAnim.SetBool(adsHash, false);
				cameraAnim.SetBool(adsHash, false);
				gunCamAnim.SetBool(adsHash, false);
				anim.SetBool(adsHash, false);
				aimingDownSight = false;
			}
			
			anim.SetBool(sprintHash, Shift);
			
			// Vertical tilt of camera
			if (deltaMousePos.y != 0){
				// Slow movement for ADS
				if (aimingDownSight){
					deltaMousePos.y *= 0.5f;
				}
				
				float newVertAngle = Vector3.Angle(facing, Vector3.up) - deltaMousePos.y;
				Vector3 newFacing = Quaternion.AngleAxis(-deltaMousePos.y, perpFacing) * facing;
				
				// Limit angle so straight up/down are not possible
				if (newVertAngle > 170){
					SetFacing(Quaternion.AngleAxis(170, perpFacing) * Vector3.up);
				}
				else if (newVertAngle < 10){
					SetFacing(Quaternion.AngleAxis(10, perpFacing) * Vector3.up);
				}
				else{
					SetFacing(newFacing);
				}
			}
			
			// Rotation about Y axis
			if (deltaMousePos.x != 0){
				// Slow movement for ADS
				if (aimingDownSight){
					deltaMousePos.x *= 0.5f;
				}
				
				SetFacing(Quaternion.AngleAxis(deltaMousePos.x, Vector3.up) * facing);
			}
			
			// Firing script
			if (Mouse_Left){
				//player.tryFire();
				player.SetFiringState(true);
				anim.SetBool(fireHash, true);
				fpsAnim.SetBool(fireHash, true);
			}
			else{
				player.SetFiringState(false);
				anim.SetBool(fireHash, false);
				fpsAnim.SetBool(fireHash, true);
			}
		}

		newVel.x *= speedFactor;
		newVel.z *= speedFactor;

		// Apply velocity and force
		if (!GetComponent<Rigidbody>().isKinematic && !player.isStunned){
			GetComponent<Rigidbody>().velocity = newVel;
		}

		// Apply spread to weapon based on actions
		currentWeapon.SetTargetSpread (spread);

		// Behaviour for if game is not running
		if (player.game.awaitingEndConfirm){
			X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
			
			if (X_Press){
				player.readyToEnd = true;
				player.display.blackout.SetActive(true);
			}
		}

		// Update previous controller state
		prevState = state;

		// Apply calculated speed animation
		Quaternion revFacingRot = Quaternion.FromToRotation(facing2D, Vector3.forward);
		Vector3 rotatedVelocity = revFacingRot * GetComponent<Rigidbody>().velocity;

		forwardSpeed = rotatedVelocity.z;
		rightSpeed = rotatedVelocity.x;
		speed = GetComponent<Rigidbody>().velocity.magnitude;

		anim.SetFloat(fwdSpeedHash, forwardSpeed);
		anim.SetFloat(rgtSpeedHash, rightSpeed);
		anim.SetFloat(speedHash, speed);
		anim.SetBool (crouchHash, isCrouching);
		anim.SetBool(sprintHash, isSprinting);

		fpsAnim.SetFloat(fwdSpeedHash, forwardSpeed);
		fpsAnim.SetFloat(rgtSpeedHash, rightSpeed);
		fpsAnim.SetFloat(speedHash, speed);
		fpsAnim.SetBool (crouchHash, isCrouching);
		fpsAnim.SetBool(sprintHash, isSprinting);

		// Snap spine back to initial rotations
		if (player.isDead){
			spineJoint.transform.localRotation = Quaternion.Euler(initialSpineAngles);
			flagInRange = false;
			player.display.grabFlagPrompt.SetActive(false);
		}
	}

	public void OnTriggerEnter(Collider collider){
		if (collider.gameObject.tag == "Crystal"){
			flagInRange = true;
			player.display.grabFlagPrompt.SetActive(true);
		}
		else if (collider.gameObject.tag == "ExitGoal" && flagPickedUp){
			player.game.GameWon();
		}
	}

	public void OnTriggerExit(Collider collider){
		if (collider.gameObject.tag == "Crystal"){
			flagInRange = false;
			player.display.grabFlagPrompt.SetActive(false);
		}
	}

	public void DropFlag(){
		flagPickedUp = false;
		anim.SetBool(flagCarryHash, false);
		fpsAnim.SetBool(flagCarryHash, false);
		anim.SetTrigger(resetHash);
		fpsAnim.SetTrigger(resetHash);
		
		player.FlagDropped();
	}
	
	void SetCrouching(bool crouchState){
		isCrouching = crouchState;

		cameraAnim.SetBool(crouchHash, isCrouching);
		gunCamAnim.SetBool(crouchHash, isCrouching);

		if (isCrouching){
			terrainCollider.height = CrouchHeight;
			terrainCollider.center = new Vector3(0, CrouchHeight * 0.5f, 0);
		}
		else{
			terrainCollider.height = StandHeight;
			terrainCollider.center = new Vector3(0, StandHeight * 0.5f, 0);
		}
	}

	// Sets facing according to input
	public void SetFacing(Vector3 newFacing){
		cameraOffset = playerCam.transform.localPosition;
		facing = newFacing;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;
		transform.LookAt(transform.position + facing2D);
		playerCam.transform.LookAt(transform.position + facing + cameraOffset);

		if (!player.isDead){
			// Sets spine angle by determining angle of elevation of facing
			Quaternion recoveryRotation = Quaternion.FromToRotation(transform.forward, Vector3.forward);
			Vector3 straightenedFacing = recoveryRotation * facing;

			float spineAdjust = Quaternion.FromToRotation(straightenedFacing, Vector3.forward).eulerAngles.x;
			if (spineAdjust >= 180){
				spineAdjust = spineAdjust - 360;
			}

			spineAdjust *= 0.75f;

			spineJoint.transform.localRotation = Quaternion.Euler(initialSpineAngles.x - spineAdjust, initialSpineAngles.y, initialSpineAngles.z);
		}
	}

	public Vector2 getFacing2D(){
		return facing2D;
	}
	
	// Testing for ground directly beneath and at edges of collider
	public bool IsGrounded(){

		Vector3 groundCheckCenter = new Vector3(GetComponent<Collider>().bounds.center.x, GetComponent<Collider>().bounds.min.y + offset * 0.5f + 0.12f, GetComponent<Collider>().bounds.center.z);

		if (Physics.CheckSphere(groundCheckCenter, colliderRad, player.groundedLayer)){
			return true;
		}


		return false;
	}
	
	// Sets controller that this player will be associated with
	public void SetController(int newId){
		if (newId > 0 && newId < 5){
			controllerId = newId - 1;
			player.isDead = false;
		}
	}
	
	// Gets sign of given float value
	int SignOf(float number){
		if (number < 0){
			return -1;
		}
		return 1;
	}

	public void ShowNewWeapon(){
		player.CycleWeapons(swapAdjustment);
	}

	public void EndWeaponSwap(){
		isSwapping = false;
	}

	public void ResetWeaponSelected(){
		swapAdjustment = -player.currentWeaponIndex;
		int nextWeaponIndex = player.GetExpectedWeaponIndex(swapAdjustment);
		anim.SetInteger(weaponHash, nextWeaponIndex);
		fpsAnim.SetInteger(weaponHash, nextWeaponIndex);
		player.CycleWeapons(swapAdjustment);
	}
}
