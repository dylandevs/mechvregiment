/**
 * 
 * Basic player movement script
 * Manages lateral/longitudinal movement, camera movement, and jumping
 * 
 **/

using UnityEngine;
using System.Collections;

public class ControllerScript : MonoBehaviour {

	const float SPRINT_SPEED = 12f;
	const float RUN_SPEED = 6f;
	const float WALK_SPEED = 1.5f;
	const float RUN_THRESH = 0.5f;
	const float JUMP_SPEED = 8f;
	const float MAX_LOOK_ANGLE = 88;

	int controllerId = 1;
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	Vector3 perpFacing = new Vector3(1, 0, 0);
	Vector3 cameraOffset = Vector3.zero;
	Vector3 groundCheckVector = new Vector3(0, 0.05f, 0);

	// Inputs
	public Camera playerCam;
	public Player player;
	public Animator anim;

	// Animation hash id
	int speedHash = Animator.StringToHash("Speed");
	int fireHash = Animator.StringToHash("Firing");

	// Use this for initialization
	void Start () {
		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
		cameraOffset = playerCam.transform.localPosition;
	}
	
	// Update is called once per frame
	void Update () {

		anim.SetFloat(speedHash, rigidbody.velocity.magnitude);

		Vector3 newVel = new Vector3(0, rigidbody.velocity.y, 0);
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;

		// Getting controller values
		bool A_Down = Input.GetButtonDown("A_" + controllerId);

		float R_XAxis = Input.GetAxis("R_XAxis_" + controllerId);
		float R_YAxis = Input.GetAxis("R_YAxis_" + controllerId);
		bool RS_Press = Input.GetButtonDown("RS_" + controllerId);

		float L_XAxis = Input.GetAxis("L_XAxis_" + controllerId);
		float L_YAxis = Input.GetAxis("L_YAxis_" + controllerId);
		bool LS_Held = Input.GetButton("LS_" + controllerId);

		float TriggersR = Input.GetAxis("TriggersR_" + controllerId);
		float TriggersL = Input.GetAxis("TriggersL_" + controllerId);

		if (RS_Press){

		}

		if (IsGrounded()){

			// Jumping
			if (A_Down){
				newVel.y += JUMP_SPEED;
				//playerCam.transform.localPosition = new Vector3 (0, 0, 0);
			}

		}

		// Toggle ADS
		if (RS_Press) {
			if (player.toggleADS()){
				anim.SetInteger(fireHash, 2);
			}
			else{
				anim.SetInteger(fireHash, 0);
			}
		}


		// Lateral movement (strafing)
		if (L_XAxis != 0){
			if (Mathf.Abs(L_XAxis) > RUN_THRESH){
				newVel += RUN_SPEED * perpFacing * signOf(L_XAxis);
			}
			else{
				newVel += WALK_SPEED * perpFacing * signOf(L_XAxis);
			}
		}

		// Longitudinal movement
		if (L_YAxis != 0){
			// Sprint
			if (LS_Held && L_YAxis < RUN_THRESH){
				newVel += SPRINT_SPEED * facing2D;
			}
			// Run
			else if (Mathf.Abs(L_YAxis) > RUN_THRESH){
				newVel += RUN_SPEED * facing2D * -signOf(L_YAxis);
			}
			// Walk
			else{
				newVel += Mathf.Lerp(0, RUN_SPEED, Mathf.Abs(L_YAxis)/RUN_THRESH) * facing2D * -signOf(L_YAxis);
			}
		}

		// Rotation about Y axis
		if (R_XAxis != 0){
			facing = Quaternion.AngleAxis(R_XAxis * R_XAxis * signOf(R_XAxis) * 5, Vector3.up) * facing;
			facing2D = new Vector3(facing.x, 0, facing.z).normalized;
			transform.LookAt(transform.position + facing2D);
			playerCam.transform.LookAt(transform.position + facing + cameraOffset);
		}

		// Vertical tilt of camera
		if (R_YAxis != 0){
			Vector3 newFacing = Quaternion.AngleAxis(R_YAxis * R_YAxis * signOf(R_YAxis) * 5, perpFacing) * facing;
			float vertAngle = Vector3.Angle(newFacing, Vector3.up);
			//Debug.Log (Vector3.Angle(newFacing, Vector3.up));

			// Limit angle so straight up/down are not possible
			if (vertAngle >= 10 && vertAngle <= 170){
				facing = newFacing;
				playerCam.transform.LookAt(transform.position + facing + cameraOffset);
			}
			else{
				// Do nothing
			}
		}

		// Firing script
		if (TriggersR != 0){
			player.tryFire(facing, transform.position + facing + cameraOffset);
			if (anim.GetInteger(fireHash) != 2){
				anim.SetInteger (fireHash, 1);
			}
		}
		else{
			if (anim.GetInteger(fireHash) != 2){
				anim.SetInteger (fireHash, 0);
			}
		}

		// Apply velocity and force
		rigidbody.velocity = newVel;
	}

	bool IsGrounded(){
		return Physics.Raycast(transform.position + groundCheckVector, -Vector3.up, groundCheckVector.y * 2);
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
