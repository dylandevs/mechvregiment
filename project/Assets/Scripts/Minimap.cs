using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Minimap : MonoBehaviour {

	// Inputs
	public RectTransform minimapMask;
	public RectTransform minimapImg;
	public GameObject mapObj;
	public Player player;

	Vector2 minimapSize;
	Vector2 terrainSize;
	Vector2 mapRatio;
	Vector3 terrainOffset;

	public GameObject[] players;
	public GameObject[] bots;
	public GameObject objective;
	public GameObject goliath;
	public GameObject mapIconPrefab;

	public List<MinimapIcon> icons = new List<MinimapIcon>();

	public Sprite objectiveIcon;

	// Use this for initialization
	void Start () {
		minimapSize = minimapImg.sizeDelta;
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
	}

	void UpdateMapTransforms(){
		minimapMask.localRotation = CalculateMapRotation();
		minimapImg.localPosition = CalculateMapTranslation();
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

	void PopulateMapIcons(){
		GameObject icon = Instantiate (mapIconPrefab) as GameObject;
		icon.transform.SetParent (minimapMask.transform);
		icon.transform.localPosition = Vector3.zero;
		icon.transform.localScale = Vector3.one;
		icon.transform.localRotation = Quaternion.identity;
		icon.GetComponent<Image> ().sprite = objectiveIcon;

		MinimapIcon iconScript = icon.GetComponent<MinimapIcon> ();
		iconScript.associatedObject = objective;
		iconScript.type = MinimapIcon.MMIconType.Objective;

		icons.Add(iconScript);
	}

	void UpdateMapIcons(){
		foreach (MinimapIcon icon in icons){
			if (icon.type == MinimapIcon.MMIconType.Objective){
				icon.transform.localPosition = CalculateIconTranslation(icon.associatedObject.transform.position);
			}
		}
	}
}
