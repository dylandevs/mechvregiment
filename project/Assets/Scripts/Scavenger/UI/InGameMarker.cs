using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InGameMarker : MonoBehaviour {

	public GameObject associatedObject;
	public enum IGMarkerType {Objective, Scavenger, Goliath};
	public IGMarkerType type;
	public Image img;
	public RectTransform rectTransform;
	public Vector3 offset = Vector3.zero;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
