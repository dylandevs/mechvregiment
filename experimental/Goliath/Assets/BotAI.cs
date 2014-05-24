using UnityEngine;
using System.Collections;

public class BotAI : MonoBehaviour {

	// Distance thresholds
	const int THRESH_CLOSE = 12;
	const int THRESH_MED = 20;
	const float VIEW_ANGLE = 120 / 2;
	const float MOVE_ACCEL = 600;
	const float MAX_SPEED = 5;
	const float FIRE_RATE = 1.2f;

	// Predefined state colours
	Color safeCol = new Color32(0, 255, 0, 1);
	Color warnCol = new Color32(255, 255, 0, 1);
	Color dngrCol = new Color32(255, 0, 0, 1);

	public GameObject opponent;
	public GameObject ammunition;
	float reloadProg = FIRE_RATE;

	Vector3 baseFacing = new Vector3(0, 0, 1);
	Vector3 facing = new Vector3(0, 0, 1);
	Vector3 up = new Vector3(0, 1, 0);

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		facing = transform.rotation * baseFacing;

		// Calculating useful values
		Vector3 diffVec = opponent.transform.position - transform.position;
		float angle = Vector3.Angle(facing, diffVec);

		switch (checkInSight(angle, diffVec)){

		// Out of view
		case 0:
			setSurfaceColour(safeCol);
			break;

		// In view, out of range
		case 1:
			faceTarget(opponent);
			rigidbody.AddForce(facing * MOVE_ACCEL);
			setSurfaceColour(warnCol);
			break;

		// In range
		case 2:
			if (reloadProg >= FIRE_RATE){
				fireInDirection(diffVec);
				reloadProg = 0;
			}
			else{
				reloadProg += Time.deltaTime;
			}
			faceTarget(opponent);
			setSurfaceColour(dngrCol);
			break;

		default:
			break;
		}

		// Clamp velocity
		rigidbody.velocity = new Vector3(Mathf.Clamp(rigidbody.velocity.x, -MAX_SPEED, MAX_SPEED), rigidbody.velocity.y, Mathf.Clamp(rigidbody.velocity.z, -MAX_SPEED, MAX_SPEED));
	}

	byte checkInSight(float angle, Vector3 diffVec){
		float distance = diffVec.magnitude;

		if (distance < THRESH_CLOSE && angle <= VIEW_ANGLE){
			return 2;
		}
		else if (distance < THRESH_MED && angle <= VIEW_ANGLE){
			return 1;
		}
		else{
			return 0;
		}
	}

	void fireInDirection(Vector3 direction){
		Vector3 bulletGenPos = transform.position + facing;
		GameObject bullet = Instantiate(ammunition, bulletGenPos, Quaternion.identity) as GameObject;
		TempProjectile bulletScript = bullet.GetComponent<TempProjectile>();
		bulletScript.setDirection(direction);
	}

	void faceTarget(GameObject target){
		Vector3 targetVec = target.transform.position;
		targetVec.y = transform.position.y;
		transform.LookAt(targetVec);
	}

	void setSurfaceColour(Color newCol){
		renderer.material.color = newCol;
	}
}
