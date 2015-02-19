/**
 * 
 * Manages rendering of crosshairs to player viewport (scale, bloom, etc.)
 * 
 **/

using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {

	// Iterable line storage
	Texture2D[] lines = new Texture2D[4];

	public Texture2D topLine;
	public Texture2D rgtLine;
	public Texture2D botLine;
	public Texture2D lftLine;
	
	float[] centre = new float[]{0.5f, 0.5f};
	float scale = 1;
	float baseOffset = 0;

	// Screen offsets
	float offSetX = 0;
	float offSetY = 0;

	// Top, right, bottom, left
	Rect[] positions = new Rect[4];

	// Use this for initialization
	void Start () {
		loadTextures();
	}
	
	// Update is called once per frame
	void Update () {

	}

	private void loadTextures(){
		if (!lines[0]){
			// Storing lines for easier access
			lines[0] = topLine;
			lines[1] = rgtLine;
			lines[2] = botLine;
			lines[3] = lftLine;
		}
	}

	// Determining draw positions based on scale, crosshair images, and viewport
	public void calculateDrawPositions(float baseSpread = 0){
		loadTextures();
		baseOffset = lines[0].width*scale/2;

		// Centering scene accordingly
		offSetX = (centre[0] - 0.5f) * Screen.width;
		offSetY = (0.5f - centre[1]) * Screen.height;

		// Initialize spread at given base
		updateSpread(baseSpread);
	}

	// Updates drawing rectangles with new positions, based on spread specified
	// Note: Up/down movement is inverted
	public void updateSpread(float newSpread){
		positions[0] = new Rect((Screen.width - lines[0].width*scale)/2 + offSetX,
		                        Screen.height/2 - lines[0].height*scale - baseOffset + offSetY - newSpread,
		                        lines[0].width*scale,
		                        lines[0].height*scale);
		positions[1] = new Rect(Screen.width/2 + baseOffset + offSetX + newSpread,
		                        (Screen.height - lines[1].height*scale)/2 + offSetY,
		                        lines[1].width*scale,
		                        lines[1].height*scale);
		positions[2] = new Rect((Screen.width - lines[2].width*scale)/2 + offSetX,
		                        Screen.height/2 + baseOffset + offSetY + newSpread,
		                        lines[2].width*scale,
		                        lines[2].height*scale);
		positions[3] = new Rect(Screen.width/2 - lines[3].width*scale - baseOffset + offSetX - newSpread,
		                        (Screen.height - lines[3].height*scale)/2 + offSetY,
		                        lines[3].width*scale,
		                        lines[3].height*scale);
	}

	public void setScaleCentre(float newScale, float x, float y){
		scale = newScale;
		centre[0] = x;
		centre[1] = y;
		calculateDrawPositions();
	}

	// Draw crosshair to screen
	void OnGUI(){
		for (int i = 0; i < lines.Length; i++){
			GUI.DrawTexture(positions[i], lines[i]);
		}
	}
}
