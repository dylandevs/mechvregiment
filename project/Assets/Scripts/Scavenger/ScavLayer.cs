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

	public Material invisMat;

	// Set all layers before beginning
	void Awake(){
		int view1Layer = LayerMask.NameToLayer("PlayerView1_" + Layer);
		int view3Layer = LayerMask.NameToLayer("PlayerView3_" + Layer);
		int weaponLayer = LayerMask.NameToLayer("PlayerWpn" + Layer);
		int shotColliderLayer = LayerMask.NameToLayer("PlayerCollider" + Layer);

		SetLayerRecursively(PlayerShadowWrapper, view1Layer);
		SetLayerRecursively(PlayerMeshWrapper, view3Layer);
		SetLayerRecursively(PlayerShotColliderWrapper, shotColliderLayer);
		SetTagRecursively(PlayerShotColliderWrapper, "Player");
		SetLayerRecursively(Weapon1Wrapper, weaponLayer);
		SetLayerRecursively(Weapon3Wrapper, view3Layer);

		// Duplicate weapon3 meshes and convert to invisible shadowcasters
		foreach (Transform weapon3 in Weapon3Wrapper.transform){
			foreach (Transform weaponComponent in weapon3){
				if (weaponComponent.gameObject.GetComponent<MeshFilter>() != null){
					GameObject invisComponent = Instantiate(weaponComponent.gameObject) as GameObject;
					invisComponent.renderer.material = invisMat;
					invisComponent.layer = view1Layer;

					invisComponent.transform.parent = weaponComponent;
					invisComponent.transform.localPosition = Vector3.zero;
					invisComponent.transform.localRotation = Quaternion.identity;
				}
			}
		}

		PlayerCam.cullingMask ^= 1 << view3Layer;
		PlayerCam.cullingMask |= 1 << view1Layer;
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

	void SetTagRecursively(GameObject baseObj, string tag){
		baseObj.tag = tag;

		foreach (Transform child in baseObj.transform){
			SetTagRecursively(child.gameObject, tag);
		}
	}
}
