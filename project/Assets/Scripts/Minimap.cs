using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Minimap : MonoBehaviour {

	// Inputs
	public RectTransform minimapRot;
	public RectTransform minimapTrans;
	public RectTransform minimapMask;
	public RectTransform objDirection;
	public GameObject mapObj;
	public Player player;

	Vector2 minimapSize;
	Vector2 terrainSize;
	Vector2 mapRatio;
	Vector3 terrainOffset;
	
	public GameObject playerGroup;
	public GameObject minionGroup;
	public GameObject objective;
	public GameObject goliath;
	public GameObject mapIconPrefab;

	public List<MinimapIcon> icons = new List<MinimapIcon>();

	public Sprite objectiveIcon;
	public Sprite goliathIcon;
	public Sprite minionIcon;
	public Sprite scavengerIcon;

	// Use this for initialization
	void Start () {
		minimapSize = minimapTrans.sizeDelta;
		terrainSize.x = mapObj.transform.collider.bounds.extents.x * 2;
		terrainSize.y = mapObj.transform.collider.bounds.extents.z * 2;
		terrainOffset = mapObj.transform.position;
		mapRatio.x = minimapSize.x / terrainSize.x;
		mapRatio.y = minimapSize.y / terrainSize.y;

		PopulateMapIcons ();
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMapTransforms();
		UpdateMapIcons ();
		UpdateObjectiveDirection ();
	}

	void UpdateMapTransforms(){
		minimapRot.localRotation = CalculateMapRotation();
		minimapTrans.localPosition = CalculateMapTranslation();
	}

	Vector3 CalculateMapTranslation(){
		Vector3 mapTrans = Vector3.zero;

		Vector3 relPlayerPos = player.transform.position - terrainOffset;
		mapTrans.x = -relPlayerPos.x * mapRatio.x;
		mapTrans.y = -relPlayerPos.z * mapRatio.y;

		return mapTrans;
	}

	Quaternion CalculateMapRotation(){
		Quaternion mapRot = Quaternion.identity;

		mapRot = Quaternion.FromToRotation(Vector3.forward, player.transform.forward);
		mapRot = Quaternion.AngleAxis(mapRot.eulerAngles.y, Vector3.forward);

		return mapRot;
	}

	Vector3 CalculateIconTranslation(Vector3 objTrans){
		Vector3 iconTrans = objTrans - player.transform.position;

		iconTrans.x *= mapRatio.x;
		iconTrans.y = iconTrans.z * mapRatio.y;
		iconTrans.z = 0;

		return iconTrans;
	}

	Quaternion CalculateIconRotation(Quaternion objRot){
		Quaternion iconRot = Quaternion.Euler(0, 0, objRot.eulerAngles.y);
		return iconRot;
	}

	GameObject CreateIcon(Sprite sprite){
		GameObject icon = Instantiate (mapIconPrefab) as GameObject;
		icon.transform.SetParent (minimapMask.transform);
		icon.transform.localPosition = Vector3.zero;
		icon.transform.localScale = Vector3.one;
		icon.transform.localRotation = Quaternion.identity;
		icon.GetComponent<Image> ().sprite = sprite;

		return icon;
	}

	void PopulateMapIcons(){
		GameObject icon;
		MinimapIcon iconScript;

		// Create objective icon
		icon = CreateIcon(objectiveIcon);
		iconScript = icon.GetComponent<MinimapIcon> ();
		iconScript.associatedObject = objective;
		iconScript.type = MinimapIcon.MMIconType.Objective;
		icons.Add(iconScript);

		// Create Goliath icons
		icon = CreateIcon(goliathIcon);
		iconScript = icon.GetComponent<MinimapIcon> ();
		iconScript.associatedObject = goliath;
		iconScript.type = MinimapIcon.MMIconType.Goliath;
		icons.Add(iconScript);

		// Create Minion icon
		foreach (Transform minion in minionGroup.transform){
			icon = CreateIcon(minionIcon);
			iconScript = icon.GetComponent<MinimapIcon> ();
			iconScript.associatedObject = minion.gameObject;
			iconScript.type = MinimapIcon.MMIconType.Minion;
			iconScript.img.color = new Color(1, 1, 1, 0);
			icons.Add(iconScript);
		}

		// Create Scavenger icons
		foreach (Transform scavenger in playerGroup.transform){
			// Verify that current player is not selected
			if (scavenger.gameObject != player.gameObject){
				icon = CreateIcon(scavengerIcon);
				iconScript = icon.GetComponent<MinimapIcon> ();
				iconScript.associatedObject = scavenger.gameObject;
				iconScript.type = MinimapIcon.MMIconType.Scavenger;
				icons.Add(iconScript);
			}
		}
	}

	void UpdateMapIcons(){
		foreach (MinimapIcon icon in icons){
			if (icon.type == MinimapIcon.MMIconType.Objective){
				icon.transform.localPosition = CalculateIconTranslation(icon.associatedObject.transform.position);
			}
			else if (icon.type == MinimapIcon.MMIconType.Scavenger){
				icon.transform.localPosition = CalculateIconTranslation(icon.associatedObject.transform.position);
				icon.transform.localRotation = CalculateIconRotation(icon.associatedObject.transform.rotation);
			}
			else if (icon.type == MinimapIcon.MMIconType.Goliath){
				icon.transform.localPosition = CalculateIconTranslation(icon.associatedObject.transform.position);
				icon.transform.localRotation = CalculateIconRotation(icon.associatedObject.transform.rotation);
			}
			else if (icon.type == MinimapIcon.MMIconType.Minion){
				icon.transform.localPosition = CalculateIconTranslation(icon.associatedObject.transform.position);
				icon.img.color = new Color(1, 1, 1, icon.GetEnemyOpacity());
			}
		}
	}

	void UpdateObjectiveDirection(){
		Vector3 flatPlayerPos = player.transform.position;
		flatPlayerPos.y = 0;
		Vector3 flatObjPos = objective.transform.position;
		flatObjPos.y = 0;

		Vector3 diff = flatObjPos - flatPlayerPos;
		Quaternion diffRot = Quaternion.FromToRotation (player.transform.forward, diff);

		objDirection.localRotation = Quaternion.Euler (0, 0, -diffRot.eulerAngles.y);
	}
}
