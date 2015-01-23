using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";

	PhotonView photonView;
            

	public GameObject goliathTop;
	public GameObject goliathBot;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		photonView = PhotonView.Get(this)
	}
	void MakeRoom(){
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
	
	// Update is called once per frame
	void Update () {
		Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	//Code here happens when lobby is all set up
        	//Photon RPC example
        	//photonView.RPC("SendInfoToServer", PhotonTargets.All, "hhhhe he he hi");
        }
	}
	private void OnPhotonJoinRoomFailed (){
        Debug.Log("Photon failed to join room.");
    }


	[RPC]
	void UpdateNathanPos(Vector3 pos){
		print ("BNOAEBJIRTF");
	}

	[RPC]
	void GoliathTransform(Transform incomingTop, Transform incomingBot){
		goliathTop.transform.position = incomingTop.position;
		goliathTop.transform.rotation = incomingTop.rotation;
		goliathBot.transform.position = incomingBot.position;
		goliathBot.transform.rotation = incomingBot.rotation;
	}

	[RPC]
	void GoliathMissileFire(Vector3 targetPosition){

	}

}
