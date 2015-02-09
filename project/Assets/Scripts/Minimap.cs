using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {

	// Inputs
	public RectTransform minimapMask;
	public RectTransform minimapImg;
	public GameObject mapObj;
	public Player player;
	public MapNoteManager notes;

	Vector2 minimapSize;
	Vector2 terrainSize;
	Vector2 mapRatio;
	Vector3 terrainOffset;

	// Use this for initialization
	void Start () {
		minimapSize = minimapImg.sizeDelta;
		terrainSize.x = mapObj.transform.collider.bounds.extents.x * 2;
		terrainSize.y = mapObj.transform.collider.bounds.extents.z * 2;
		terrainOffset = mapObj.transform.position;
		mapRatio.x = minimapSize.x / terrainSize.x;
		mapRatio.y = minimapSize.y / terrainSize.y;
		print (terrainSize);
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMapTransforms();
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


		print (mapTrans);
		return mapTrans;
	}

	Quaternion CalculateMapRotation(){
		Quaternion mapRot = Quaternion.identity;

		mapRot = Quaternion.FromToRotation(Vector3.forward, player.transform.forward);
		mapRot = Quaternion.AngleAxis(mapRot.eulerAngles.y, Vector3.forward);

		return mapRot;
	}
}
