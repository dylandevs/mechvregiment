using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScavInstructions : MonoBehaviour {

	private int currInstruction = 0;

	public float instructionInputDelay = 2;

	public CanvasGroup[] views;

	public float mapAnimationDuration = 8;
	public float mapPathDisplayDelay = 0.3f; // Ratio of each section time during which path is finished animating and is delaying next path to draw
	private float mapPathDisplayDelayInv = 1;
	private float mapAnimationProg = 0;
	private float mapAnimationSection = 1;
	private float mapAnimationSectionInv = 1;
	public Image[] mapPaths;

	private bool acceptingInput = false;
	private float acceptInputProg = 0;

	public GameObject nextPrompt;

	// Use this for initialization
	void Start () {
		mapAnimationSection = mapAnimationDuration / (mapPaths.Length);
		mapAnimationSectionInv = 1 / mapAnimationSection;
		mapPathDisplayDelayInv = 1 / (1 - mapPathDisplayDelay);
	}
	
	// Update is called once per frame
	void Update () {

		// Delay accepting input to prevent flicking through screens
		if (!acceptingInput){
			acceptInputProg += Time.deltaTime;
			if (acceptInputProg > instructionInputDelay){
				acceptingInput = true;
			}
		}

		// Map animation
		if (currInstruction == 0){
			mapAnimationProg += Time.deltaTime;
			if (mapAnimationProg < mapAnimationDuration){
				int currentPath = (int)Mathf.Floor(mapAnimationProg * mapAnimationSectionInv);
				float currentSectionProg = Mathf.Min(1, (mapAnimationProg % (mapAnimationSection)) * mapAnimationSectionInv * mapPathDisplayDelayInv);

				mapPaths[currentPath].fillAmount = currentSectionProg;
				mapPaths[currentPath].color = new Color(1, 1, 1, currentSectionProg);
			}
		}
		// Static instructions
		else if (currInstruction == 1){
			// No current behaviour
		}
	}

	public bool AdvanceInstructions(){
		if (acceptingInput){
			if (currInstruction++ >= views.Length){
				return true;
			}

			if (currInstruction == views.Length - 1){
				nextPrompt.SetActive(false);
			}

			// Swap canvas groups
			views [currInstruction - 1].alpha = 0;
			views [currInstruction].alpha = 1;

			// Hide button
			acceptingInput = false;
			acceptInputProg = 0;
		}

		return false;
	}
}
