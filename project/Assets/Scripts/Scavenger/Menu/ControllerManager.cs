using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using XInputDotNetPure;

public class ControllerManager : MonoBehaviour {

	private const int NumControllers = 4;

	public WaitingPanel[] panels = new WaitingPanel[4];
	public Color yesColor;
	public Color noColor;
	public Color readyColor;
	public Color waitingColor;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetReady(int player){
		panels[player].buttons.SetActive (false);
		panels[player].readyMessage.SetActive (true);
		panels[player].panelBg.color = readyColor;
	}

	public void UnsetReady(int player){
		panels[player].buttons.SetActive (true);
		panels[player].readyMessage.SetActive (false);
		panels[player].panelBg.color = waitingColor;
	}

	public void SetInverted(int player){
		panels[player].invertState.text = "Yes";
		panels[player].invertState.color = yesColor;
	}

	public void UnsetInverted(int player){
		panels[player].invertState.text = "No";
		panels[player].invertState.color = noColor;
	}

	public void Initialize(int controllerCount){
		for (int i = controllerCount; i < NumControllers; i++){
			panels[i].gameObject.SetActive(false);
		}
	}
}
