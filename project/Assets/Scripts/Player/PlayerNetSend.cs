using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	private PhotonView photonView;
            

	public GameObject goliathTop;
	public GameObject goliathBot;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		photonView = PhotonView.Get (this);
	}
	void MakeRoom(){
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
	
	// Update is called once per frame
	void Update () {
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	//Code here happens when lobby is all set up
        	//Photon RPC example
        	//photonView.RPC("SendInfoToServer", PhotonTargets.All, "hhhhe he he hi");
        }
        else {
        	Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        }
	}
	private void OnPhotonJoinRoomFailed (){
        Debug.Log("Photon failed to join room.");
    }

    void OnJoinedRoom(){
    	Room currentRoom = PhotonNetwork.room;
    	Debug.Log("Room \""+ currentRoom.name +"\" has this many joined: " + currentRoom.playerCount);
    }

    private void DecerealizeTransform(GameObject giveMeTransform, Vector3 position, Quaternion rotation){
    	giveMeTransform.transform.position = position;
    	giveMeTransform.transform.rotation = rotation;
    }


	[RPC]
	void UpdateNathanPos(Vector3 pos){
		print ("BNOAEBJIRTF");
	}

	[RPC]
	public void ExchangeGoliathPositioning(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
		print("HOLY SHIT GETTING POSITION");
		DecerealizeTransform(goliathTop, topPos, topRot);
		DecerealizeTransform(goliathBot, botPos, botRot);
	}

	[RPC]
	void GoliathMissileFire(Vector3 targetPosition){

	}

}
