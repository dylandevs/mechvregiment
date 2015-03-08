/**
 * 
 * Remote (Goliath-side) player movement script
 * Manages lateral/longitudinal movement
 * 
 **/

using UnityEngine;
using System.Collections;

public class RemotePlayerController : MonoBehaviour {
	
	const float SprintSpeed = 12f;
	const float RunSpeed = 6f;
	const float CrouchSpeed = 1.5f;
	const float RunThresh = 0.5f;
	const float JumpSpeed = 6f;
	const float ADSSpeedFactor = 0.7f;
	const float CrouchSpeedFactor = 0.5f;

	public Vector3 facing = new Vector3(0, 0, 1);
	Vector3 facing2D = new Vector3(0, 0, 1);
	public Vector3 perpFacing = new Vector3(1, 0, 0);
	Vector3 groundCheckVector = new Vector3(0, 0.1f, 0);
	Vector3 halfColliderX;
	Vector3 halfColliderZ;
	float speedFactor = 1;
	float initialSpineAngle = 0;

	// Inputs
	//public Player player;
	//public Animator anim;
	//private Animator weaponAnim;
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

	// State trackers
	bool isCrouching = false;

	// Controller input trackers
	float R_XAxis = 0;
	float R_YAxis = 0;

	float L_XAxis = 0;
	float L_YAxis = 0;
	bool LS_Held = false;
	
	float TriggersR = 0;
	float TriggersL = 0;
	
	// Use this for initialization
	void Start () {
		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
		groundCheckVector.y += collider.bounds.extents.y * 0.5f;
		halfColliderX = new Vector3 (collider.bounds.extents.x * 0.5f, 0, 0);
		halfColliderZ = new Vector3 (0, 0, collider.bounds.extents.z * 0.5f);
		//initialSpineAngle = spineJoint.transform.localRotation.z;
	}
	
	// Update is called once per frame
	void Update () {

		// Updating attributes
		Vector3 newVel = rigidbody.velocity;
		perpFacing = Vector3.Cross(Vector3.up, facing).normalized;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;

		speedFactor = 1;

		bool currentlyGrounded = IsGrounded();
		
		if (currentlyGrounded){
			newVel = new Vector3(0, rigidbody.velocity.y, 0);
		}
		
		/*Weapon currentWeapon = player.GetCurrentWeapon ();
		weaponAnim = player.GetCurrentWeapon().animator;*/	
		
		// Lateral movement (strafing)
		if (L_XAxis != 0){
			if (Mathf.Abs(L_XAxis) > RunThresh){
				newVel += RunSpeed * perpFacing * SignOf(L_XAxis);
			}
			else{
				newVel += CrouchSpeed * perpFacing * SignOf(L_XAxis);
			}
		}
		
		// Longitudinal movement
		if (L_YAxis != 0){
			// Sprint
			if (LS_Held && L_YAxis < RunThresh){
				newVel += SprintSpeed * facing2D;
				//anim.SetBool(sprintHash, true);

				// Cancel crouch
				SetCrouching(false);
			}
			// Run
			else if (Mathf.Abs(L_YAxis) > RunThresh){
				newVel += RunSpeed * facing2D * -SignOf(L_YAxis);
				//anim.SetBool(sprintHash, false);
			}
			// Walk
			else{
				newVel += Mathf.Lerp(0, RunSpeed, Mathf.Abs(L_YAxis)/RunThresh) * facing2D * -SignOf(L_YAxis);
				//anim.SetBool(sprintHash, false);
			}
		}			

		// Toggle ADS
		if (TriggersL != 0 && currentlyGrounded /*&& !currentWeapon.IsReloading()*/) {
			//player.ToggleADS(true);
			//anim.SetInteger(fireHash, 2);
			//weaponAnim.SetBool(adsHash, true);
			speedFactor *= ADSSpeedFactor;
		}
		else{
			//player.ToggleADS(false);
			//anim.SetInteger(fireHash, 0);
			//weaponAnim.SetBool(adsHash, false);
		}

		// Apply speed factor for crouching
		if (isCrouching){
			speedFactor *= CrouchSpeedFactor;
		}

		/*// Rotation about Y axis
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
		}*/
		
		// Firing script
		/*if (TriggersR != 0){
			player.SetFiringState(true);
			if (anim.GetInteger(fireHash) != 2){
				anim.SetInteger (fireHash, 1);
			}
		}
		else{
			player.SetFiringState(false);
			if (anim.GetInteger(fireHash) != 2){
				anim.SetInteger (fireHash, 0);
			}
		}*/

		newVel.x *= speedFactor;
		newVel.z *= speedFactor;

		// Apply velocity and force
		rigidbody.velocity = newVel;

		// Apply calculated speed animation
		//anim.SetFloat(speedHash, rigidbody.velocity.magnitude);
		Quaternion revFacingRot = Quaternion.FromToRotation(facing2D, Vector3.forward);
		Vector3 rotatedVelocity = revFacingRot * rigidbody.velocity;

		/*anim.SetFloat(fwdSpeedHash, rotatedVelocity.z);
		anim.SetFloat(rgtSpeedHash, rotatedVelocity.x);
		anim.SetFloat(speedHash, rigidbody.velocity.magnitude);
		anim.SetBool (crouchHash, isCrouching);*/
	}

	void SetCrouching(bool crouchState){
		isCrouching = crouchState;
		//player.SetCrouching(isCrouching);

		// Trigger crouch animation
	}

	// Sets facing according to input
	public void SetFacing(Vector3 newFacing){
		facing = newFacing;
		facing2D = new Vector3(facing.x, 0, facing.z).normalized;
		transform.LookAt(transform.position + facing2D);

		// Sets spine angle by determining angle of elevation of facing
		//Vector3 spineAngles = spineJoint.transform.localRotation.eulerAngles;
		Quaternion recoveryRotation = Quaternion.FromToRotation(transform.forward, Vector3.forward);
		Vector3 straightenedFacing = recoveryRotation * facing;

		float adjustmentZ = Quaternion.FromToRotation(straightenedFacing, Vector3.forward).eulerAngles.x;
		if (adjustmentZ > 180){
			adjustmentZ = adjustmentZ - 360;
		}
		adjustmentZ *= 0.5f;

		//spineJoint.transform.localRotation = Quaternion.Euler(spineAngles.x, spineAngles.y, initialSpineAngle + adjustmentZ);
	}

	// Testing for ground directly beneath and at edges of collider
	bool IsGrounded(){
		RaycastHit rayHit;
		
		if (Physics.Raycast(transform.position + groundCheckVector, -Vector3.up, out rayHit, groundCheckVector.y)){
			if (rayHit.collider.tag == "Terrain"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector + halfColliderZ, -Vector3.up, out rayHit, groundCheckVector.y)){
			if (rayHit.collider.tag == "Terrain"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector - halfColliderZ, -Vector3.up, out rayHit, groundCheckVector.y)){
			if (rayHit.collider.tag == "Terrain"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector + halfColliderX, -Vector3.up, out rayHit, groundCheckVector.y)){
			if (rayHit.collider.tag == "Terrain"){
				return true;
			}
		}
		if (Physics.Raycast(transform.position + groundCheckVector - halfColliderX, -Vector3.up, out rayHit, groundCheckVector.y)){
			if (rayHit.collider.tag == "Terrain"){
				return true;
			}
		}
		
		return false;
	}
	
	// Gets sign of given float value
	int SignOf(float number){
		if (number < 0){
			return -1;
		}
		return 1;
	}

	public void SetControllerInputs(float Rx, float Ry, float Lx, float Ly, bool Ls, float Tr, float Tl){
		R_XAxis = Rx;
		R_YAxis = Ry;
		
		L_XAxis = Lx;
		L_YAxis = Ly;
		LS_Held = Ls;

		TriggersR = Tr;
		TriggersL = Tl;
	}
}
