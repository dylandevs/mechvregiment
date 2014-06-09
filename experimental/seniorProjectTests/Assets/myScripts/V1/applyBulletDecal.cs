using UnityEngine;
using System.Collections;

public class applyBulletDecal : MonoBehaviour {

	public GameObject decal;
	public float offset = 0.01f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
	
	void hasBeenShot(RaycastHit info)
	{
		//tells you what's shot
		Debug.Log ("I'm Shot", gameObject);

		Quaternion rotation = Quaternion.LookRotation(info.normal); 
		//gets the shot location and adds the decal(red squyare) as well sets it as a child of the wall therefor moving with the wall
		//the offset moves the texture slightly out as to not clip
		GameObject instance = Instantiate(decal,info.point + info.normal * offset ,rotation)as GameObject;
		instance.transform.parent = transform;
	}
}
