using UnityEngine;
using System.Collections;

public class networkScript : MonoBehaviour {
	public GameObject nathan;
	//vars for mech position
	public GameObject topHalf;
	public GameObject bottomHalf;

	//starting and ending positions for projectiles

	//hit points(things i shot/where i shot them)


	private float countdown = 0f;
	private string serverIP = "169.254.59.9";
	private int port = 25000;
	private NetworkPlayer _myNetworkPlayer;
	private string gameName = "GoliathConnection_083";
	private string gameType = "MechvRegimentMatch";

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		//send goliath transfrom to master server
		//NetworkView.RPC("GoliathTransform",RPCMode.All,topHalf.transform.position,topHalf.transform.rotation,bottomHalf.transform.position,bottomHalf.transform.rotation);
		//GoliathTransform(topHalf.transform.position,topHalf.transform.rotation,bottomHalf.transform.position,bottomHalf.transform.rotation);

		if (Network.peerType == NetworkPeerType.Disconnected) {
			if(countdown<=0){
				Network.Connect(serverIP, port);
				countdown = 1f;
			}
			else {
				countdown -= Time.deltaTime;
			}
		} 

	}

	//Recieve
	[RPC]
	void UpdateNathanPos(Vector3 nathanP) {
		nathan.transform.position = nathanP;
	}

	[RPC]
	void UpdateNathanRot(Quaternion nathanR) {
		nathan.transform.rotation = nathanR;
	}

	//Send
	[RPC]
	void GoliathTransform(Vector3 topHalfPos,Quaternion topHalfRot,Vector3 botHalfPos,Quaternion botHalfRot ){
	}

}


