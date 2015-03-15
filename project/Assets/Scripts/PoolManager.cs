using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {
	public int startingPool = 1;
	public bool loopable = false;
	public bool retainLocal = false;
	public GameObject prefabObject;

	// Networking
	public PlayerNetSend scavNetworker = null;
	public GoliathNetworking goliathNetworker = null;

	private List<GameObject> inactiveObjectPool = new List<GameObject>();
	private List<GameObject> activeObjectPool = new List<GameObject>();
	private List<GameObject> temporaryObjectPool = new List<GameObject>();

	// Use this for initialization
	void Start () {
		int poolCounter = 0;

		// Add existing children to pool
		foreach (Transform child in transform){
			if (child.gameObject.GetActive()){
				activeObjectPool.Add(child.gameObject);
			}
			else{
				inactiveObjectPool.Add(child.gameObject);
			}

			Pooled pooledScript = null;
			if (pooledScript = child.GetComponent<Pooled>() as Pooled){
				pooledScript.index = poolCounter;
				poolCounter++;

				if (scavNetworker){
					pooledScript.scavNetworker = scavNetworker;
				}
				if (goliathNetworker){
					pooledScript.goliathNetworker = goliathNetworker;
				}
			}
		}

		// Populates inactive list of objects
		for (int i = 0; i < startingPool; i++){
			inactiveObjectPool.Add(Instantiate(prefabObject) as GameObject);
			inactiveObjectPool[i].SetActive(false);
			inactiveObjectPool[i].transform.SetParent(transform, !retainLocal);

			Pooled pooledScript = null;
			if (pooledScript = inactiveObjectPool[i].GetComponent<Pooled>() as Pooled){
				pooledScript.index = transform.childCount - 1;
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
			returnedObj.transform.SetParent(transform, !retainLocal);

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
