using UnityEngine;
using System.Collections;

public class bottomMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//check if the mech is moving or not for rotation
		bool isMoving = false;
		float moveSpeed = 5 * Time.deltaTime;

		//if a key is pushed move the location of the mech
		if (Input.GetKey (KeyCode.W)) {
			transform.Translate(Vector3.forward * moveSpeed);
			isMoving = true;
		}

		if (Input.GetKey (KeyCode.S)) {
			transform.Translate(Vector3.forward * -1 * moveSpeed);
			isMoving = true;
		}

		if (Input.GetKey (KeyCode.D)) {
			transform.Translate(Vector3.right * moveSpeed);
			isMoving = true;
		}

		if (Input.GetKey (KeyCode.A)) {
			transform.Translate(Vector3.right * -1 * moveSpeed);
			isMoving = true;
		}


		/**if (isMoving == false) {

			//finding the tophalf gameobject
			GameObject topHalf = GameObject.Find("topHalf");
			Vector3 topHalfDirection =topHalf.GetComponent<currentRot>;

			Transform target = topHalfDirection;
			float speed = 5;
			Vector3 targetDir = target.position - transform.position;
			float step = speed * Time.deltaTime;
			Vector3 newDir = Vector3.RotateTowards(transform.forward,targetDir,step,0.0F);
			transform.rotation = Quaternion.LookRotation(newDir);
		}**/

}
}
