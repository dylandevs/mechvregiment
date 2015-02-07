using UnityEngine;
using System.Collections;

public class Minimap : MonoBehaviour {

	// Inputs
	public RectTransform minimapMask;
	public RectTransform minimapImg;
	public GameObject mapObj;
	public Player player;
	public MapNoteManager notes;

	Vector2 minimapSize = Vector2.zero;

	// Use this for initialization
	void Start () {
		minimapSize = minimapImg.sizeDelta;
	}
	
	// Update is called once per frame
	void Update () {
		UpdateMapTransforms();
	}

	void UpdateMapTransforms(){
		minimapMask.localRotation = CalculateMapRotation();

	}

	Vector3 CalculateMapTranslation(){
		Vector3 mapTrans = Vector3.zero;



		return mapTrans;
	}

	Quaternion CalculateMapRotation(){
		Quaternion mapRot = Quaternion.identity;

		mapRot = Quaternion.FromToRotation(Vector3.forward, player.transform.forward);
		mapRot = Quaternion.AngleAxis(mapRot.eulerAngles.y, Vector3.forward);

		return mapRot;
	}
}
