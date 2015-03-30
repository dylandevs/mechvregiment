using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using XInputDotNetPure;

public class StartMenu : MonoBehaviour {

	// XInput variables
	private GamePadState state;
	private GamePadState prevState;
	public Button[] mainOptions;

	private int currentSelectedOption = 0;

	// Use this for initialization
	void Start () {
		//Application.LoadLevelAsync("ScavengerScene");
		// Wait for X press

	}
	
	// Update is called once per frame
	void Update () {
	
		state = GamePad.GetState((PlayerIndex)0);
		if (!state.IsConnected){
			return;
		}
		
		bool X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
		
		if (X_Press){// && goliathReady){
			AsyncOperation loadScene = Application.LoadLevelAsync("ScavengerScene");
		}
		
		prevState = state;

	}

	void AdjustSelection(int adjustment){
		currentSelectedOption += adjustment;
		if (currentSelectedOption < 0){
			currentSelectedOption = mainOptions.Length - 1;
		}
		else if (currentSelectedOption >= mainOptions.Length){
			currentSelectedOption = 0;
		}
	}
}
