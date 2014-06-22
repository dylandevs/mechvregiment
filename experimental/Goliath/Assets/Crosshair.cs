using UnityEngine;
using System.Collections;

public class Crosshair : MonoBehaviour {

	// Iterable line storage
	Texture2D[] lines = new Texture2D[4];

	public Texture2D topLine;
	public Texture2D rgtLine;
	public Texture2D botLine;
	public Texture2D lftLine;
	
	float crossSpread = 0;

	// Top, right, bottom, left
	Rect[] positions = new Rect[4];

	// Use this for initialization
	void Start () {
		// Storing lines for easier access
		lines[0] = topLine;
		lines[1] = rgtLine;
		lines[2] = botLine;
		lines[3] = lftLine;

		float baseOffset = lines[0].width/2;

		// Calculating draw positions
		positions[0] = new Rect((Screen.width - lines[0].width)/2, Screen.height/2 - lines[0].height - baseOffset, lines[0].width, lines[0].height);
		positions[1] = new Rect(Screen.width/2 + baseOffset, (Screen.height - lines[1].height)/2, lines[1].width, lines[1].height);
		positions[2] = new Rect((Screen.width - lines[2].width)/2, Screen.height/2 + baseOffset, lines[2].width, lines[2].height);
		positions[3] = new Rect(Screen.width/2 - lines[3].width - baseOffset, (Screen.height - lines[3].height)/2, lines[3].width, lines[3].height);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void updateSpread(float factor){

	}

	// Draw crosshair to screen
	void OnGUI(){
		for (int i = 0; i < lines.Length; i++){
			GUI.DrawTexture(positions[i], lines[i]);
		}
	}
}
