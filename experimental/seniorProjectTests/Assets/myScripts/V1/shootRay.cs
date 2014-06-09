using UnityEngine;
using System.Collections;

public class shootRay : MonoBehaviour
{
	
	public string fireButton = "Fire1";
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () 
	{
		if(Input.GetButtonDown(fireButton))
		{
			//shoot a ray from the screen and send a message to any hit object
			Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f,0.5f,0f));
			
			RaycastHit info;
			
			if(Physics.Raycast(ray, out info))
			{
				info.collider.gameObject.SendMessage("hasBeenShot",info,SendMessageOptions.DontRequireReceiver);
			}
		}
		
	}
}
