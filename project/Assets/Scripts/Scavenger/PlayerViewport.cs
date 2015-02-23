/**
 * 
 * Manages rendering of player camera to screen
 * 
 **/

using UnityEngine;
using System.Collections;

public class PlayerViewport : MonoBehaviour {

	public GameObject playerCam;
	public GameObject playerGunCam;
	public Crosshair crossScript;
	float[] renderWindow = new float[]{0, 0, 1, 1};
	float[] viewportCentre = new float[]{0.5f, 0.5f};
	float scale = 1;

	// Use this for initialization
	void Start () {
		//crossScript = playerCam.GetComponent<Crosshair>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	// Sets viewport dimensions
	public void InitializePlayerInterface(float x, float y, float width, float height, float baseWeaponSpread = 0){
		viewportCentre[0] = x + (width)/2;
		viewportCentre[1] = y + (height)/2;
		scale = height;
		
		crossScript.setScaleCentre(scale, viewportCentre[0], viewportCentre[1]);
		crossScript.calculateDrawPositions(baseWeaponSpread);
		playerCam.camera.rect = new Rect(x, y, width, height);
		playerGunCam.camera.rect = new Rect(x, y, width, height);
	}


}
