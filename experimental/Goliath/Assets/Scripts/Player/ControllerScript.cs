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
	const float WalkSpeed = 1.5f;
	const float RunThresh = 0.5f;
	const float JumpSpeed = 8f;
	const float MaxLookAngle = 88;
	
	public int controllerId = -1;
	public Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	public Vector3 perpFacing = new Vector3(1, 0, 0);
	public Vector3 cameraOffset = Vector3.zero;
	Vector3 groundCheckVector = new Vector3(0, 0.1f, 0);
	Vector3 halfColliderX;
	Vector3 halfColliderZ;

	//XInput variables
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
	int speedHash = Animator.StringToHash("Speed");
	int fireHash = Animator.StringToHash("Firing");
	int sprintHash = Animator.StringToHash("Sprinting");
	int adsHash = Animator.StringToHash("Aiming");
	
	// Keyboard trackers
	Vector2 deltaMousePos = Vector2.zero;
	bool aimingDownSight = false;
	
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

		// Ignore input if unassigned
		if (controllerId == -1) {
			return;
		}
		else{
			state = GamePad.GetState((PlayerIndex)controllerId);

			if (!state.IsConnected){
				return;
			}

			//print (state);
		}

		anim.SetFloat(speedHash, rigidbody.velocity.magnitude);
		
		Vector3 newVel = rigidbody.velocity;
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;
		
		bool currentlyGrounded = IsGrounded();
		float spread = 0;
		
		if (currentlyGrounded){
			newVel = new Vector3(0, rigidbody.velocity.y, 0);
		}
		
		Weapon currentWeapon = player.getCurrentWeapon ();
		
		// Controller connected
		if (Input.GetJoystickNames ().Length > 0){
			
			// Getting controller values
			/*bool A_Press = Input.GetButtonDown("A_" + controllerId);
			
			float R_XAxis = Input.GetAxis("R_XAxis_" + controllerId);
			float R_YAxis = Input.GetAxis("R_YAxis_" + controllerId);
			bool RS_Press = Input.GetButtonDown("RS_" + controllerId);
			
			float L_XAxis = Input.GetAxis("L_XAxis_" + controllerId);
			float L_YAxis = Input.GetAxis("L_YAxis_" + controllerId);
			bool LS_Held = Input.GetButton("LS_" + controllerId);
			
			float TriggersR = Input.GetAxis("TriggersR_" + controllerId);
			float TriggersL = Input.GetAxis("TriggersL_" + controllerId);*/
			//print (TriggersL + " " + controllerId);

			bool A_Press = (state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released);
			
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
			
			if (currentlyGrounded){
				
				// Jumping
				if (A_Press){
					newVel.y += JumpSpeed;
					//playerCam.transform.localPosition = new Vector3 (0, 0, 0);
				}
				
				// Lateral movement (strafing)
				if (L_XAxis != 0){
					if (Mathf.Abs(L_XAxis) > RunThresh){
						newVel += RunSpeed * perpFacing * signOf(L_XAxis);
						spread += currentWeapon.RunSpreadAdjust;
					}
					else{
						newVel += WalkSpeed * perpFacing * signOf(L_XAxis);
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
					}
					// Run
					else if (Mathf.Abs(L_YAxis) > RunThresh){
						newVel += RunSpeed * facing2D * -signOf(L_YAxis);
						anim.SetBool(sprintHash, false);
						spread += currentWeapon.RunSpreadAdjust;
					}
					// Walk
					else{
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
			}
			else{
				player.toggleADS(false);
				anim.SetInteger(fireHash, 0);
				weaponAnim.SetBool(adsHash, false);
				cameraAnim.SetBool(adsHash, false);
				gunCamAnim.SetBool(adsHash, false);
				aimingDownSight = false;
			}
			
			//print (cameraAnim.GetBool(adsHash));
			
			// Rotation about Y axis
			if (R_XAxis != 0){
				float adjustment = R_XAxis * R_XAxis * signOf(R_XAxis) * 5;
				
				// Slow movement for ADS
				if (aimingDownSight){
					adjustment *= 0.5f;
				}
				
				/*facing = Quaternion.AngleAxis(adjustment, Vector3.up) * facing;
				facing2D = new Vector3(facing.x, 0, facing.z).normalized;
				transform.LookAt(transform.position + facing2D);
				playerCam.transform.LookAt(transform.position + facing + cameraOffset);*/
				
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
				//Debug.Log (Vector3.Angle(newFacing, Vector3.up));
				
				// Limit angle so straight up/down are not possible
				if (vertAngle >= 10 && vertAngle <= 170){
					setFacing(newFacing);
					//playerCam.transform.LookAt(transform.position + facing + cameraOffset);
				}
				else{
					// Do nothing
				}
			}
			
			// Firing script
			if (TriggersR != 0){
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
		// Limited keyboard fallback
		else{
			Screen.showCursor = false;
			
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
					//playerCam.transform.localPosition = new Vector3 (0, 0, 0);
				}
				
				// Lateral movement (strafing)
				if (Key_A){
					if (Ctrl){
						newVel += WalkSpeed * -perpFacing;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * -perpFacing;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
				else if (Key_D){
					if (Ctrl){
						newVel += WalkSpeed * perpFacing;
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
						newVel += WalkSpeed * facing2D;
						spread += currentWeapon.WalkSpreadAdjust;
					}
					else{
						newVel += RunSpeed * facing2D;
						spread += currentWeapon.RunSpreadAdjust;
					}
				}
				else if(Key_S){
					if (Ctrl){
						newVel += WalkSpeed * facing2D * -1;
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
		
		// Apply velocity and force
		rigidbody.velocity = newVel;
		
		// Apply spread to weapon based on actions
		currentWeapon.setTargetSpread (spread);

		// Update previous state
		prevState = state;
	}
	
	// Sets facing according to input
	public void setFacing(Vector3 newFacing){
		facing = newFacing;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;
		transform.LookAt(transform.position + facing2D);
		playerCam.transform.LookAt(transform.position + facing + cameraOffset);
	}
	
	// Testing for ground directly beneath and at edges of collider
	bool IsGrounded(){
		bool groundState = false;//Physics.Raycast (transform.position + groundCheckVector, -Vector3.up, groundCheckVector.y);
		
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
			controllerId = newId;
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
