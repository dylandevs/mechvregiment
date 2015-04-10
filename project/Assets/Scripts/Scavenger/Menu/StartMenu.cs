using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using XInputDotNetPure;

public class StartMenu : MonoBehaviour {

	// Transition variables
	public float menuTransitionTime = 0.6f;
	private float invMenuTransitionTime = 1;
	private float menuTransitionProg = 0;
	private CanvasGroup transitionFrom;
	private CanvasGroup transitionTo;
	private RectTransform rectFrom;
	private delegate void TransitionCallback();
	private TransitionCallback callback;

	// XInput variables
	private GamePadState state;
	private GamePadState prevState;
	public Button[] mainOptions;

	public CanvasGroup mainMenu;
	public CanvasGroup controlsScreen;
	public CanvasGroup creditsScreen;
	public CanvasGroup loadingScreen;

	private CanvasGroup currentGroup;

	private int currentSelectedOption = 0;
	private BaseEventData eventExecutor;

	private bool isSubMenu = false;
	private bool loading = false;

	public RectTransform selfRect;

	public AudioSource switchSound;
	public AudioSource selectSound;

	// Use this for initialization
	void Start () {
		//Application.LoadLevelAsync("ScavengerScene");
		// Wait for X press
		eventExecutor = new PointerEventData(EventSystem.current);
		ExecuteEvents.Execute(mainOptions[currentSelectedOption].gameObject, eventExecutor, ExecuteEvents.pointerEnterHandler);
		invMenuTransitionTime = 1 / menuTransitionTime;
	}
	
	// Update is called once per frame
	void Update () {

		eventExecutor = new PointerEventData(EventSystem.current);
		state = GamePad.GetState((PlayerIndex)0);
		if (!state.IsConnected){
			return;
		}
		
		bool A_Press = (state.Buttons.A == ButtonState.Pressed && prevState.Buttons.A == ButtonState.Released);
		bool B_Press = (state.Buttons.B == ButtonState.Pressed && prevState.Buttons.B == ButtonState.Released);
		bool X_Press = (state.Buttons.X == ButtonState.Pressed && prevState.Buttons.X == ButtonState.Released);
		bool Y_Press = (state.Buttons.Y == ButtonState.Pressed && prevState.Buttons.Y == ButtonState.Released);

		bool Select = (A_Press || X_Press);
		bool Cancel = (B_Press || Y_Press);

		bool DecOptionIndex = ((state.ThumbSticks.Right.Y > 0.5f && prevState.ThumbSticks.Right.Y < 0.5f) ||
		                       (state.ThumbSticks.Left.Y > 0.5f && prevState.ThumbSticks.Left.Y < 0.5f) ||
		                       (state.DPad.Up == ButtonState.Pressed && prevState.DPad.Up == ButtonState.Released));

		bool IncOptionIndex = ((state.ThumbSticks.Right.Y < -0.5f && prevState.ThumbSticks.Right.Y > -0.5f) ||
		                       (state.ThumbSticks.Left.Y < -0.5f && prevState.ThumbSticks.Left.Y > -0.5f) ||
		                       (state.DPad.Down == ButtonState.Pressed && prevState.DPad.Down == ButtonState.Released));

		if (menuTransitionProg > 0){
			menuTransitionProg -= Time.deltaTime;
			transitionFrom.alpha = Mathf.Lerp(1, 0, (1 - menuTransitionProg * invMenuTransitionTime));
			float scaleProg = Mathf.Lerp(1, 0.95f, (1 - menuTransitionProg * invMenuTransitionTime));
			rectFrom.localScale = Vector3.one * scaleProg;
			transitionTo.alpha = Mathf.Lerp(0, 1, (1 - menuTransitionProg * invMenuTransitionTime));


			if (menuTransitionProg <= 0){
				transitionFrom.alpha = 0;
				transitionTo.alpha = 1;
				if (callback != null){
					callback();
					callback = null;
				}
			}
		}
		else if (!loading){
			if (!isSubMenu){
				if (Select){
					RegisterOptionSelect();
				}
				else{
					if (IncOptionIndex){
						AdjustSelection(1);
					}
					else if (DecOptionIndex){
						AdjustSelection(-1);
					}
				}
			}
			else{
				if (Cancel){
					// Return to main
					TriggerTransition(currentGroup, mainMenu);
					isSubMenu = false;
				}
			}
		}
		
		prevState = state;

	}

	void AdjustSelection(int adjustment){
		ExecuteEvents.Execute(mainOptions[currentSelectedOption].gameObject, eventExecutor, ExecuteEvents.pointerExitHandler);
		currentSelectedOption += adjustment;
		if (currentSelectedOption < 0){
			currentSelectedOption = mainOptions.Length - 1;
		}
		else if (currentSelectedOption >= mainOptions.Length){
			currentSelectedOption = 0;
		}
		ExecuteEvents.Execute(mainOptions[currentSelectedOption].gameObject, eventExecutor, ExecuteEvents.pointerEnterHandler);
		switchSound.Play ();
	}

	void RegisterOptionSelect(){
		selectSound.Play();
		switch (mainOptions[currentSelectedOption].tag){
		case "StartButton":
			TriggerTransition(mainMenu, loadingScreen);
			loading = true;
			callback = LoadGame;
			break;
		case "ControlsButton":
			TriggerTransition(mainMenu, controlsScreen);
			isSubMenu = true;
			currentGroup = controlsScreen;
			break;
		case "CreditsButton":
			TriggerTransition(mainMenu, creditsScreen);
			isSubMenu = true;
			currentGroup = creditsScreen;
			break;
		}
	}

	void TriggerTransition(CanvasGroup from, CanvasGroup to){
		menuTransitionProg = menuTransitionTime;
		transitionFrom = from;
		transitionTo = to;
		rectFrom = transitionFrom.GetComponent<RectTransform> ();
		transitionTo.GetComponent<RectTransform> ().localScale = Vector3.one;
	}

	void LoadGame(){
		Application.LoadLevel("ScavengerScene");
	}
}
