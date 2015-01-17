using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";

	//float countdownTimer = 5;

	public GameObject goliathTop;
	public GameObject goliathBot;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
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
        else {
        	//Everything for post joining here
        }
	}
    void OnJoinedRoom(){
        Debug.Log("Joined room!");
    }

	[RPC]
	void UpdateNathanPos(Vector3 pos){
		print ("BNOAEBJIRTF");
	}

	[RPC]
	void GoliathTransform(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
		goliathTop.transform.position = topPos;
		goliathTop.transform.rotation = topRot;
		goliathBot.transform.position = botPos;
		goliathBot.transform.rotation = botRot;
	}

}
