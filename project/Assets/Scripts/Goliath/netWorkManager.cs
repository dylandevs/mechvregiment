using UnityEngine;
using System.Collections;

public class netWorkManager : Photon.MonoBehaviour {
	private string roomName = "goliathBuildJoinForGoodTimez";
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
		if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
			MakeRoom();
		}
		else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
			PhotonView photonView = PhotonView.Get(this);
			photonView.RPC("sendHigh", PhotonTargets.All, "Me High");
			photonView.RPC("getPosP1", PhotonTargets.All);
		}
	}

	[RPC]
	void sendHigh (string hi){}

	[RPC]
	void getPosP1 (Transform positionP1, Quaternion rotationP1){}
}
