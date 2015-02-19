using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	private PhotonView photonView;
	private float sendTimer = 0.05f;

	public GameObject goliathTop;
	public GameObject goliathBot;

	public GameObject player1;
	public GameObject player2;
	public GameObject player3;
	public GameObject player4;

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
		sendTimer -= Time.deltaTime;
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	if(sendTimer <= 0){
                //Here's where the RPC calls go so they happen once properly joined.
                print("Sent RPC calls for frame.");
	        	photonView.RPC("PositionPlayer1", PhotonTargets.All, player1.transform.position, player1.transform.rotation);
	        	photonView.RPC("PositionPlayer2", PhotonTargets.All, player2.transform.position, player2.transform.rotation);
	        	photonView.RPC("PositionPlayer3", PhotonTargets.All, player3.transform.position, player3.transform.rotation);
	        	photonView.RPC("PositionPlayer4", PhotonTargets.All, player4.transform.position, player4.transform.rotation);
        
                sendTimer = 0.05f;
			}
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

//RPC CALLS
	[RPC]
	void UpdateNathanPos(Vector3 pos){print ("BNOAEBJIRTF");}
	[RPC]
	public void ExchangeGoliathPositioning(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
		DecerealizeTransform(goliathTop, topPos, topRot);
		DecerealizeTransform(goliathBot, botPos, botRot);
	}
	[RPC]
	void PositionPlayer1(Vector3 newPos, Quaternion newRot){}
	[RPC]
	void PositionPlayer2(Vector3 newPos, Quaternion newRot){}
	[RPC]
	void PositionPlayer3(Vector3 newPos, Quaternion newRot){}
	[RPC]
	void PositionPlayer4(Vector3 newPos, Quaternion newRot){}

	public void TogglePlayerADS (int playerNum, bool setADS){
		if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
			switch(playerNum){
				case 1:
				default:
					if(setADS) photonView.RPC("AimPlayer1", PhotonTargets.All);
					else photonView.RPC("UnaimPlayer1", PhotonTargets.All);
					break;
				case 2:
				if(setADS) photonView.RPC("AimPlayer2", PhotonTargets.All);
					else photonView.RPC("UnaimPlayer2", PhotonTargets.All);
					break;
				case 3:
					if(setADS) photonView.RPC("AimPlayer3", PhotonTargets.All);
					else photonView.RPC("UnaimPlayer3", PhotonTargets.All);
					break;
				case 4:
					if(setADS) photonView.RPC("AimPlayer4", PhotonTargets.All);
					else photonView.RPC("UnaimPlayer4", PhotonTargets.All);
					break;
			}
		}
	}
	[RPC]
	void AimPlayer1(){}
	[RPC]
	void UnaimPlayer1(){}
	[RPC]
	void AimPlayer2(){}
	[RPC]
	void UnaimPlayer2(){}
	[RPC]
	void AimPlayer3(){}
	[RPC]
	void UnaimPlayer3(){}
	[RPC]
	void AimPlayer4(){}
	[RPC]
	void UnaimPlayer4(){}
}
