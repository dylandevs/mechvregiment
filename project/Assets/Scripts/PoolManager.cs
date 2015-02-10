﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PoolManager : MonoBehaviour {

	public int startingPool = 1;
	public bool loopable = false;
	public GameObject prefabObject;

	private List<GameObject> inactiveObjectPool = new List<GameObject>();
	private List<GameObject> activeObjectPool = new List<GameObject>();
	private List<GameObject> temporaryObjectPool = new List<GameObject>();

	// Use this for initialization
	void Start () {
		// Populates inactive list of objects
		for (int i = 0; i < startingPool; i++){
			inactiveObjectPool.Add(Instantiate(prefabObject) as GameObject);
			inactiveObjectPool[i].SetActive(false);
			inactiveObjectPool[i].transform.SetParent(transform);
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

			returnedObj.transform.position = pos;
			returnedObj.transform.rotation = rot;
			returnedObj.SetActive(true);

			return returnedObj;
		}
		// Select available active object and return
		else if (loopable && activeObjectPool.Count > 0){
			GameObject returnedObj = activeObjectPool[0];
			activeObjectPool.RemoveAt(0);
			activeObjectPool.Add(returnedObj);

			returnedObj.transform.position = pos;
			returnedObj.transform.rotation = rot;
			
			return returnedObj;
		}
		// Temporarily instantiate new object
		else{
			GameObject returnedObj = Instantiate(prefabObject, pos, rot) as GameObject;
			temporaryObjectPool.Add(returnedObj);

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
