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
	public Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	public Vector3 perpFacing = new Vector3(1, 0, 0);
	public Vector3 cameraOffset = Vector3.zero;
	Vector3 groundCheckVector = new Vector3(0, 0.1f, 0);
	Vector3 halfColliderX;
	Vector3 halfColliderZ;
	float speedFactor = 1;

	// XInput variables
	private GamePadState state;
	private GamePadState prevState;

	// Inputs
	public GameObject playerCam;
	public Player player;
	public Animator anim;
	public Animator weaponAnim;
	public Animator cameraAnim;
	public Animator gunCamAnim;
	
	// Animation hash id
	int fwdSpeedHash = Animator.StringToHash("FwdSpeed");
	int rgtSpeedHash = Animator.StringToHash("RgtSpeed");
	int speedHash = Animator.StringToHash("Speed");
	int fireHash = Animator.StringToHash("Firing");
	int sprintHash = Animator.StringToHash("Sprinting");
	int adsHash = Animator.StringToHash("Aiming");
	int jumpHash = Animator.StringToHash("Jump");
	int crouchHash = Animator.StringToHash ("Crouching");
	
	// Keyboard trackers
	Vector2 deltaMousePos = Vector2.zero;
	bool aimingDownSight = false;

	// State trackers
	bool isCrouching = false;
	
	// Use this for initialization
	void Start () {
		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
		cameraOffset = playerCam.transform.localPosition;
		groundCheckVector.y += collider.bounds.extents.y * 0.5f;
		halfColliderX = new Vector3 (collider.bounds.extents.x * 0.5f, 0, 0);
		halfColliderZ = new Vector3 (0, 0, collider.bounds.extents.z * 0.5f);
	}
	
	// Update is called once per frame
	void Update () {

		// Updating attributes
		Vector3 newVel = rigidbody.velocity;
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;

		bool currentlyGrounded = IsGrounded();
		float spread = 0;
		speedFactor = 1;
		
		if (currentlyGrounded){
			newVel = new Vector3(0, rigidbody.velocity.y, 0);
		}
		
		Weapon currentWeapon = player.getCurrentWeapon ();

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
			
			// Getting controller values
			bool A_Press = (state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released);
			bool B_Press = (state.Buttons.B == ButtonState.Pressed && prevState.Buttons.B == ButtonState.Released);
			bool X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
			bool Y_Press = (state.Buttons.Y == ButtonState.Pressed && prevState.Buttons.Y == ButtonState.Released);

			bool DPad_Next = (state.DPad.Right == ButtonState.Pressed && prevState.DPad.Right == ButtonState.Released);
			bool DPad_Prev = (state.DPad.Left == ButtonState.Pressed && prevState.DPad.Left == ButtonState.Released);
			
			float R_XAxis = state.ThumbSticks.Right.X;
			float R_YAxis = -state.ThumbSticks.Right.Y;
			bool RS_Press = (state.Buttons.RightStick == ButtonState.Pressed && prevState.Buttons.RightStick == ButtonState.Released);
			
			float L_XAxis = state.ThumbSticks.Left.X;
			float L_YAxis = -state.ThumbSticks.Left.Y;
			bool LS_Held = (state.Buttons.LeftStick == ButtonState.Pressed);
			
			float TriggersR = state.Triggers.Right;
			float TriggersL = state.Triggers.Left;
			
			if (RS_Press){
				
			}

			// Toggle crouching
			if (B_Press && currentlyGrounded){
				setCrouching(!isCrouching);
			}

			// Trigger change weapon
			/*if (X_Press){
				player.cycleWeapons();
				weaponAnim = player.getCurrentWeapon().animator;
			}*/
			if (DPad_Next){
				player.cycleWeapons(1);
				weaponAnim = player.getCurrentWeapon().animator;
			}
			else if (DPad_Prev){
				player.cycleWeapons(-1);
				weaponAnim = player.getCurrentWeapon().animator;
			}

			if (currentlyGrounded){
				
				// Jumping
				if (A_Press){
					newVel.y += JumpSpeed;

					anim.SetTrigger(jumpHash);

					// Cancel crouch
					setCrouching(false);
				}
				
				// Lateral movement (strafing)
				if (L_XAxis != 0){
					if (Mathf.Abs(L_XAxis) > RunThresh){
						newVel += RunSpeed * perpFacing * signOf(L_XAxis);
						spread += currentWeapon.RunSpreadAdjust;
					}
					else{
						newVel += CrouchSpeed * perpFacing * signOf(L_XAxis);
						spread += currentWeapon.WalkSpreadAdjust;
					}
				}
				
				// Longitudinal movement
				if (L_YAxis != 0){
					// Sprint
					if (LS_Held && L_YAxis < RunThresh){
						newVel += SprintSpeed * facing2D;
						anim.SetBool(sprintHash, true);
						spread += currentWeapon.SprintSpreadAdjust;

						// Cancel crouch
						setCrouching(false);
					}
					// Run
					else if (Mathf.Abs(L_YAxis) > RunThresh){
						if (isCrouching){
							spread += currentWeapon.CrouchSpreadAdjust;
						}
						newVel += RunSpeed * facing2D * -signOf(L_YAxis);
						anim.SetBool(sprintHash, false);
						spread += currentWeapon.RunSpreadAdjust;
					}
					// Walk
					else{
						if (isCrouching){
							spread += currentWeapon.CrouchSpreadAdjust;
						}
						newVel += Mathf.Lerp(0, RunSpeed, Mathf.Abs(L_YAxis)/RunThresh) * facing2D * -signOf(L_YAxis);
						anim.SetBool(sprintHash, false);
						spread += currentWeapon.WalkSpreadAdjust;
					}
				}			
			}
			else{
				spread += currentWeapon.JumpSpreadAdjust;
			}
			
			// Toggle ADS
			if (TriggersL != 0 && currentlyGrounded && !currentWeapon.getReloading()) {
				player.toggleADS(true);
				anim.SetInteger(fireHash, 2);
				weaponAnim.SetBool(adsHash, true);
				cameraAnim.SetBool(adsHash, true);
				gunCamAnim.SetBool(adsHash, true);
				aimingDownSight = true;
				spread += currentWeapon.AdsSpreadAdjust;
				speedFactor *= ADSSpeedFactor;
			}
			else{
				player.toggleADS(false);
				anim.SetInteger(fireHash, 0);
				weaponAnim.SetBool(adsHash, false);
				cameraAnim.SetBool(adsHash, false);
				gunCamAnim.SetBool(adsHash, false);
				aimingDownSight = false;
			}

			// Apply speed factor for crouching
			if (isCrouching){
				speedFactor *= CrouchSpeedFactor;
			}

			// Rotation about Y axis
			if (R_XAxis != 0){
				float adjustment = R_XAxis * R_XAxis * signOf(R_XAxis) * 5;
				
				// Slow movement for ADS
				if (aimingDownSight){
					adjustment *= 0.5f;
				}
				
				setFacing(Quaternion.AngleAxis(adjustment, Vector3.up) * facing);
			}
			
			// Vertical tilt of camera
			if (R_YAxis != 0){
				float adjustment = R_YAxis * R_YAxis * signOf(R_YAxis) * 5;
				
				// Slow movement for ADS
				if (aimingDownSight){
					adjustment *= 0.5f;
				}
				
				Vector3 newFacing = Quaternion.AngleAxis(adjustment, perpFacing) * facing;
				float vertAngle = Vector3.Angle(newFacing, Vector3.up);

				// Limit angle so straight up/down are not possible
				if (vertAngle >= 10 && vertAngle <= 170){
					setFacing(newFacing);
				}
				else{
					// Do nothing
				}
			}
			
			// Firing script
			if (TriggersR != 0){
				player.setFiringState(true);
				if (anim.GetInteger(fireHash) != 2){
					anim.SetInteger (fireHash, 1);
				}
			}
			else{
				player.setFiringState(false);
				if (anim.GetInteger(fireHash) != 2){
					anim.SetInteger (fireHash, 0);
				}
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
			if (Mouse_Right && currentlyGrounded && !currentWeapon.getReloading()) {
				player.toggleADS(true);
				anim.SetInteger(fireHash, 2);
				weaponAnim.SetBool(adsHash, true);
				cameraAnim.SetBool(adsHash, true);
				gunCamAnim.SetBool(adsHash, true);
				aimingDownSight = true;
				spread += currentWeapon.AdsSpreadAdjust;
				speedFactor *= ADSSpeedFactor;
			}
			else{
				player.toggleADS(false);
				anim.SetInteger(fireHash, 0);
				weaponAnim.SetBool(adsHash, false);
				cameraAnim.SetBool(adsHash, false);
				gunCamAnim.SetBool(adsHash, false);
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
					setFacing(Quaternion.AngleAxis(170, perpFacing) * Vector3.up);
				}
				else if (newVertAngle < 10){
					setFacing(Quaternion.AngleAxis(10, perpFacing) * Vector3.up);
				}
				else{
					setFacing(newFacing);
				}
			}
			
			// Rotation about Y axis
			if (deltaMousePos.x != 0){
				// Slow movement for ADS
				if (aimingDownSight){
					deltaMousePos.x *= 0.5f;
				}
				
				setFacing(Quaternion.AngleAxis(deltaMousePos.x, Vector3.up) * facing);
			}
			
			// Firing script
			if (Mouse_Left){
				//player.tryFire();
				player.setFiringState(true);
				if (anim.GetInteger(fireHash) != 2){
					anim.SetInteger (fireHash, 1);
				}
			}
			else{
				player.setFiringState(false);
				if (anim.GetInteger(fireHash) != 2){
					anim.SetInteger (fireHash, 0);
				}
			}
		}

		newVel.x *= speedFactor;
		newVel.z *= speedFactor;

		// Apply velocity and force
		rigidbody.velocity = newVel;
		
		// Apply spread to weapon based on actions
		currentWeapon.setTargetSpread (spread);

		// Update previous controller state
		prevState = state;

		// Apply calculated speed animation
		//anim.SetFloat(speedHash, rigidbody.velocity.magnitude);
		Quaternion revFacingRot = Quaternion.FromToRotation(facing2D, Vector3.forward);
		Vector3 rotatedVelocity = revFacingRot * rigidbody.velocity;

		anim.SetFloat(fwdSpeedHash, rotatedVelocity.z);
		anim.SetFloat(rgtSpeedHash, rotatedVelocity.x);
		anim.SetFloat(speedHash, rigidbody.velocity.magnitude);
		anim.SetBool (crouchHash, isCrouching);
	}

	void setCrouching(bool crouchState){
		isCrouching = crouchState;
		player.setCrouching(isCrouching);

		// Trigger crouch animation
	}

	// Sets facing according to input
	public void setFacing(Vector3 newFacing){
		facing = newFacing;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;
		transform.LookAt(transform.position + facing2D);
		playerCam.transform.LookAt(transform.position + facing + cameraOffset);
	}

	public Vector2 getFacing2D(){
		return facing2D;
	}
	
	// Testing for ground directly beneath and at edges of collider
	bool IsGrounded(){
		bool groundState = false;

		if (!groundState){
			groundState = Physics.Raycast(transform.position + groundCheckVector, -Vector3.up, groundCheckVector.y);
		}
		if (!groundState) {
			groundState = Physics.Raycast(transform.position + groundCheckVector + halfColliderZ, -Vector3.up, groundCheckVector.y);
		}
		if (!groundState){
			groundState = Physics.Raycast(transform.position + groundCheckVector - halfColliderZ, -Vector3.up, groundCheckVector.y);
		}
		if (!groundState){
			groundState = Physics.Raycast(transform.position + groundCheckVector + halfColliderX, -Vector3.up, groundCheckVector.y);
		}
		if (!groundState){
			groundState = Physics.Raycast(transform.position + groundCheckVector - halfColliderX, -Vector3.up, groundCheckVector.y);
		}
		
		return groundState;
	}
	
	// Sets controller that this player will be associated with
	public void setController(int newId){
		if (newId > 0 && newId < 5){
			controllerId = newId - 1;
		}
	}
	
	// Gets sign of given float value
	int signOf(float number){
		if (number < 0){
			return -1;
		}
		return 1;
	}
}
