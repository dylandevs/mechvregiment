using UnityEngine;
using System.Collections;

public class ScavLayer : MonoBehaviour {

	public int Layer = 1;

	public GameObject Weapon1Wrapper;
	public GameObject Weapon3Wrapper;

	public GameObject PlayerMeshWrapper;
	public GameObject PlayerShadowWrapper;
	public GameObject PlayerShotColliderWrapper;

	public Camera PlayerCam;
	public Camera GunCam;
	public Player player;

	// Set all layers before beginning
	void Awake(){
		int view1Layer = LayerMask.NameToLayer("PlayerView1_" + Layer);
		int view3Layer = LayerMask.NameToLayer("PlayerView3_" + Layer);
		int weaponLayer = LayerMask.NameToLayer("PlayerWpn" + Layer);
		int shotColliderLayer = LayerMask.NameToLayer("PlayerCollider" + Layer);

		SetLayerRecursively(PlayerShadowWrapper, view1Layer);
		SetLayerRecursively(PlayerMeshWrapper, view3Layer);
		SetLayerRecursively(PlayerShotColliderWrapper, shotColliderLayer);
		SetLayerRecursively(Weapon1Wrapper, weaponLayer);
		SetLayerRecursively(Weapon3Wrapper, view3Layer);

		PlayerCam.cullingMask ^= 1 << view3Layer;
		GunCam.cullingMask ^= 1 << weaponLayer;
		player.shootableLayer ^= 1 << shotColliderLayer;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void SetLayerRecursively(GameObject baseObj, int layer){
		baseObj.layer = layer;

		foreach (Transform child in baseObj.transform){
			SetLayerRecursively(child.gameObject, layer);
		}
	}
}
