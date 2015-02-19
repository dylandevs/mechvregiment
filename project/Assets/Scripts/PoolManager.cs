using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {

	public int startingPool = 1;
	public bool loopable = false;
	public bool retainLocal = false;
	public GameObject prefabObject;

	private List<GameObject> inactiveObjectPool = new List<GameObject>();
	private List<GameObject> activeObjectPool = new List<GameObject>();
	private List<GameObject> temporaryObjectPool = new List<GameObject>();

	// Use this for initialization
	void Start () {
		// Add existing children to pool
		foreach (Transform child in transform){
			if (child.gameObject.GetActive()){
				activeObjectPool.Add(child.gameObject);
			}
			else{
				inactiveObjectPool.Add(child.gameObject);
			}
		}

		// Populates inactive list of objects
		for (int i = 0; i < startingPool; i++){
			inactiveObjectPool.Add(Instantiate(prefabObject) as GameObject);
			inactiveObjectPool[i].SetActive(false);

			Vector3 localPos = inactiveObjectPool[i].transform.position;
			Quaternion localRot = inactiveObjectPool[i].transform.rotation;
			Vector3 localScale = inactiveObjectPool[i].transform.localScale;

			inactiveObjectPool[i].transform.SetParent(transform);

			// Restore local transforms
			if (retainLocal){
				inactiveObjectPool[i].transform.localPosition = localPos;
				inactiveObjectPool[i].transform.localRotation = localRot;
				inactiveObjectPool[i].transform.localScale = localScale;
			}
		}
	}

	// Update is called once per frame
	void Update () {
	
	}

	public GameObject Retrieve(Vector3 pos = default(Vector3), Quaternion rot = default(Quaternion)){
		// Select available inactive object and return
		if (inactiveObjectPool.Count > 0){
			GameObject returnedObj = inactiveObjectPool[0];
			inactiveObjectPool.RemoveAt(0);
			activeObjectPool.Add(returnedObj);

			if (retainLocal){
				returnedObj.transform.localPosition = pos;
				returnedObj.transform.localRotation = rot;
			}
			else{
				returnedObj.transform.position = pos;
				returnedObj.transform.rotation = rot;
			}
			returnedObj.SetActive(true);

			return returnedObj;
		}
		// Select available active object and return
		else if (loopable && activeObjectPool.Count > 0){
			GameObject returnedObj = activeObjectPool[0];
			activeObjectPool.RemoveAt(0);
			activeObjectPool.Add(returnedObj);

			if (retainLocal){
				returnedObj.transform.localPosition = pos;
				returnedObj.transform.localRotation = rot;
			}
			else{
				returnedObj.transform.position = pos;
				returnedObj.transform.rotation = rot;
			}
			
			return returnedObj;
		}
		// Temporarily instantiate new object
		else{
			GameObject returnedObj = Instantiate(prefabObject, pos, rot) as GameObject;
			temporaryObjectPool.Add(returnedObj);

			Vector3 localPos = returnedObj.transform.position;
			Quaternion localRot = returnedObj.transform.rotation;
			Vector3 localScale = returnedObj.transform.localScale;

			returnedObj.transform.SetParent(transform);

			if (retainLocal){
				returnedObj.transform.localPosition = localPos;
				returnedObj.transform.localRotation = localRot;
				returnedObj.transform.localScale = localScale;
			}

			return returnedObj;
		}
	}

	public void Deactivate(GameObject obj){
		obj.SetActive(false);

		// Check if object was temporary
		if (temporaryObjectPool.Contains(obj)){
			temporaryObjectPool.Remove(obj);
			Destroy(obj);
		}
		else{
			activeObjectPool.Remove(obj);
			inactiveObjectPool.Add(obj);
		}
	}
}
