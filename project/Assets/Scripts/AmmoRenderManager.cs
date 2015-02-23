using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AmmoRenderManager : MonoBehaviour {

	private int MaxAmmoCount = 1;
	private int remainingAmmo = 1;

	public Color readyRoundColor = new Color(1, 1, 1, 0.8f);
	public Color spentRoundColor = new Color(0.7f, 0.7f, 0.7f, 0.5f);

	public float columns = 1;
	public Vector2 iconOffsets = Vector2.zero;
	public Image ammoGraphic;
	public CanvasGroup ammoWrapper;
	private Image[] ammoGroup;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
	
	}

	public void Initialize(int ammoCount){
		MaxAmmoCount = ammoCount;
		remainingAmmo = MaxAmmoCount;

		float perCol = Mathf.Ceil(MaxAmmoCount / columns);
		float invPerCol = 1 / perCol;

		ammoGroup = new Image[MaxAmmoCount];

		// TODO: Populate ammo group
		for (int i = 0; i < MaxAmmoCount; i++){
			Image ammoImg = Instantiate(ammoGraphic) as Image;
			ammoImg.transform.SetParent(ammoWrapper.transform, false);
			ammoImg.color = readyRoundColor;

			// Setting positions
			Vector3 newPos = Vector3.zero;

			newPos.x = Mathf.Floor(i * invPerCol) * iconOffsets.x;
			newPos.y = (i % perCol) * iconOffsets.y;

			ammoImg.transform.localPosition = newPos;

			ammoGroup[i] = ammoImg;
		}
	}

	public void ExpendSingleRound(){
		--remainingAmmo;
		if (remainingAmmo >= 0){
			ammoGroup [remainingAmmo].color = spentRoundColor;
		}
	}

	public void Reload(int ammoCount){
		for (int i = 0; i < ammoCount; i++){
			ammoGroup [i].color = readyRoundColor;
		}
		remainingAmmo = ammoCount;
	}

	public void DeactivateRender(){
		ammoWrapper.alpha = 0;
	}

	public void ActivateRender(){
		ammoWrapper.alpha = 1;
	}
}
