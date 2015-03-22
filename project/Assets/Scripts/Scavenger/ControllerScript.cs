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
	
	const float SprintSpeed = 12f;
	const float RunSpeed = 6f;
	const float CrouchSpeed = 1.5f;
	const float RunThresh = 0.5f;
	const float JumpSpeed = 6f;
	const float ADSSpeedFactor = 0.7f;
	const float CrouchSpeedFactor = 0.5f;

	public bool isKeyboard = false;
	public int controllerId = -1;
	[HideInInspector]
	public Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	[HideInInspector]
	public Vector3 perpFacing = new Vector3(1, 0, 0);
	[HideInInspector]
	public Vector3 cameraOffset = Vector3.zero;
	Vector3 groundCheckVector = new Vector3(0, 0.1f, 0);
	Vector3 halfColliderX;
	Vector3 halfColliderZ;
	float speedFactor = 1;
	Vector3 initialSpineAngles;

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

	// Keyboard trackers
	Vector2 deltaMousePos = Vector2.zero;

	// State trackers
	[HideInInspector]
	public bool isCrouching = false;
	[HideInInspector]
	public bool isSprinting = false;
	[HideInInspector]
	public bool aimingDownSight = false;
	
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
	private int swapAdjustment = 0;
	
	// Use this for initialization
	void Start () {
		playerCam = cameraAnim.gameObject;

		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
		cameraOffset = playerCam.transform.localPosition;
		groundCheckVector.y += collider.bounds.extents.y * 0.5f;
		halfColliderX = new Vector3 (collider.bounds.extents.x * 0.5f, 0, 0);
		halfColliderZ = new Vector3 (0, 0, collider.bounds.extents.z * 0.5f);
		initialSpineAngles = spineJoint.transform.localRotation.eulerAngles;

		anim = player.anim;
		fpsAnim = player.fpsAnim;
	}
	
	// Update is called once per frame
	void Update () {

		// Updating attributes
		Vector3 newVel = rigidbody.velocity;
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;

		currentlyGrounded = IsGrounded();

		// Setting default attributes
		float spread = 0;
		speedFactor = 1;
		isSprinting = false;
		
		if (currentlyGrounded){
			newVel = new Vector3(0, rigidbody.velocity.y, 0);
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

			// Ignore all input if dead
			if (player.isDead){
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

			// Toggle crouching
			if (B_Press && currentlyGrounded){
				SetCrouching(!isCrouching);
			}

			// Trigger change weapon
			if (!isSwapping && !player.GetCurrentWeapon().IsReloading()){
				if (DPad_Next){
					swapAdjustment = 1;
					int nextWeaponIndex = player.GetExpectedWeaponIndex(swapAdjustment);

					anim.SetTrigger(changeWeapHash);
					anim.SetInteger(weaponHash, nextWeaponIndex);
					fpsAnim.SetTrigger(changeWeapHash);
					fpsAnim.SetInteger(weaponHash, nextWeaponIndex);

					isSwapping = true;
				}
				else if (DPad_Prev){
					swapAdjustment = -1;
					int nextWeaponIndex = player.GetExpectedWeaponIndex(swapAdjustment);

					anim.SetTrigger(changeWeapHash);
					anim.SetInteger(weaponHash, nextWeaponIndex);
					fpsAnim.SetTrigger(changeWeapHash);
					fpsAnim.SetInteger(weaponHash, nextWeaponIndex);

					isSwapping = true;
				}
			}

			// Reloading
			if (Y_Press && !isSwapping){
				player.TriggerReload();
			}

			if (currentlyGrounded){
				
				// Jumping
				if (A_Press){
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
					if (LS_Held && L_YAxis < RunThresh){
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

			// Toggle ADS
			if (TriggersL != 0 && currentlyGrounded && !currentWeapon.IsReloading() && !isSprinting && !isSwapping) {
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
			
			// Firing script
			if (TriggersR != 0 && !isSwapping){
				player.SetFiringState(true);
				//anim.SetBool(fireHash, true);
			}
			else{
				player.SetFiringState(false);
				//anim.SetBool(fireHash, false);
			}
		}

		// Limited keyboard fallback
		else{
			Screen.showCursor = false;

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
		rigidbody.velocity = newVel;
		
		// Apply spread to weapon based on actions
		currentWeapon.SetTargetSpread (spread);

		// Update previous controller state
		prevState = state;

		// Apply calculated speed animation
		Quaternion revFacingRot = Quaternion.FromToRotation(facing2D, Vector3.forward);
		Vector3 rotatedVelocity = revFacingRot * rigidbody.velocity;

		forwardSpeed = rotatedVelocity.z;
		rightSpeed = rotatedVelocity.x;
		speed = rigidbody.velocity.magnitude;

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
		}
	}

	void SetCrouching(bool crouchState){
		isCrouching = crouchState;

		cameraAnim.SetBool(crouchHash, isCrouching);
		gunCamAnim.SetBool(crouchHash, isCrouching);
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
	bool IsGrounded(){
		RaycastHit rayHit;

		if (Physics.Raycast(transform.position + groundCheckVector, -Vector3.up, out rayHit, groundCheckVector.y, player.shootableLayer)){
			if (rayHit.collider.tag == "Terrain" || rayHit.collider.tag == "Player"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector + halfColliderZ, -Vector3.up, out rayHit, groundCheckVector.y, player.shootableLayer)){
			if (rayHit.collider.tag == "Terrain" || rayHit.collider.tag == "Player"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector - halfColliderZ, -Vector3.up, out rayHit, groundCheckVector.y, player.shootableLayer)){
			if (rayHit.collider.tag == "Terrain" || rayHit.collider.tag == "Player"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector + halfColliderX, -Vector3.up, out rayHit, groundCheckVector.y, player.shootableLayer)){
			if (rayHit.collider.tag == "Terrain" || rayHit.collider.tag == "Player"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector - halfColliderX, -Vector3.up, out rayHit, groundCheckVector.y, player.shootableLayer)){
			if (rayHit.collider.tag == "Terrain" || rayHit.collider.tag == "Player"){
				return true;
			}
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
}
