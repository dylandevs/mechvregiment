/**
 * 
 * Basic player movement script
 * Manages lateral/longitudinal movement, camera movement, and jumping
 * 
 **/

using UnityEngine;
using System.Collections;

public class ControllerScript : MonoBehaviour {

	const float SPRINT_SPEED = 1.2f;
	const float RUN_SPEED = 0.6f;
	const float WALK_SPEED = 0.15f;
	const float RUN_THRESH = 0.5f;
	const float JUMP_FORCE = 200;

	string controllerId = "1";
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);
	Vector3 perpFacing = new Vector3(1, 0, 0);

	public Camera playerCam;

	// Use this for initialization
	void Start () {
		// Adjust facing direction based on starting rotation
		facing = transform.rotation * facing;
	}
	
	// Update is called once per frame
	void Update () {

		Vector3 newForce = Vector3.zero;
		Vector3 newVel = Vector3.zero;
		perpFacing = Vector3.Cross(up, facing);
		perpFacing /= perpFacing.magnitude;

		// Getting controller values
		bool A_Down = Input.GetButtonDown("A_" + controllerId);

		float R_XAxis = Input.GetAxis("R_XAxis_" + controllerId);
		float R_YAxis = Input.GetAxis("R_YAxis_" + controllerId);

		float L_XAxis = Input.GetAxis("L_XAxis_" + controllerId);
		float L_YAxis = Input.GetAxis("L_YAxis_" + controllerId);
		bool LS_Held = Input.GetButton("LS_" + controllerId);

		if (A_Down){
			newForce.y += JUMP_FORCE * rigidbody.mass;
			//playerCam.transform.localPosition = new Vector3 (0, 0, 0);
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
				newVel += SPRINT_SPEED * facing;
			}
			// Run
			else if (Mathf.Abs(L_YAxis) > RUN_THRESH){
				newVel += RUN_SPEED * facing * -signOf(L_YAxis);
			}
			// Walk
			else{
				newVel += WALK_SPEED * facing * -signOf(L_YAxis);
			}
		}

		// Rotation about Y axis
		if (R_XAxis != 0){
			facing = Quaternion.AngleAxis(R_XAxis * 5, up) * facing;
			transform.rotation *= Quaternion.AngleAxis(R_XAxis * 5, up);
		}

		// Vertical tilt of camera
		if (R_YAxis != 0){
			playerCam.transform.Rotate(new Vector3(R_YAxis * 3, 0, 0));
		}

		// Apply velocity and force
		rigidbody.velocity = new Vector3(newVel.x * 10, rigidbody.velocity.y, newVel.z * 10);
		rigidbody.AddForce(newForce);
	}

	// Sets controller that this player will be associated with
	void setController(int newId){
		if (newId > 0 && newId < 5){
			controllerId = newId.ToString();
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
