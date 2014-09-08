/**
 * 
 * Manages rendering of player camera to screen
 * 
 **/

using UnityEngine;
using System.Collections;

public class PlayerViewport : MonoBehaviour {

	public GameObject playerCam;
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
	public void InitializePlayerInterface(float xLow, float xHigh, float yLow, float yHigh, float baseWeaponSpread = 0){
		renderWindow[0] = xLow;
		renderWindow[1] = xHigh;
		renderWindow[2] = yLow;
		renderWindow[3] = yHigh;
		viewportCentre[0] = xLow + (xHigh - xLow)/2;
		viewportCentre[1] = yLow + (yHigh - yLow)/2;
		scale = xHigh - xLow;
		
		crossScript.setScaleCentre(scale, viewportCentre[0], viewportCentre[1]);
		crossScript.calculateDrawPositions(baseWeaponSpread);
		playerCam.camera.rect = new Rect(renderWindow[0], renderWindow[2], renderWindow[1] - renderWindow[0], renderWindow[3] - renderWindow[2]);
	}


}
