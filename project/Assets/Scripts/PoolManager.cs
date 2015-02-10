using UnityEngine;
using System.Collections;

public class PoolManager : MonoBehaviour {

	public int startingPool = 1;
	public bool loopable = false;
	public GameObject prefabObject;

	private GameObject[] objectPool;
	private GameObject[] inactiveObjectPool;


	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
