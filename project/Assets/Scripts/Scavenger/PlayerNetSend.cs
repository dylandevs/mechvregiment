using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	//private PhotonView photonView;
	private float SendInterval = 0.05f;
	private float sendTimer = 0;

	public GameObject goliathTop;
	public GameObject goliathBot;

	public Player[] players;

	float SendInterval2 = 0.02f;
	float sendTimer2 = 0;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		//photonView = PhotonView.Get (this);
	}
	void MakeRoom(){
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
	
	// Update is called once per frame
	void Update () {
		sendTimer -= Time.deltaTime;
		sendTimer2 -= Time.deltaTime;
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	if(sendTimer <= 0){
                //Here's where the RPC calls go so they happen once properly joined.

				for(int i = 0; i < players.Length; i++){
					if (players[i].gameObject.GetActive()){
						ControllerScript control = players[i].playerController;

						//photonView.RPC ("SetPlayerPosition", PhotonTargets.All, i, players[i].transform.position);
						//photonView.RPC ("SetPlayerFacing", PhotonTargets.All, i, control.facing);
						//photonView.RPC ("SyncControllerInput", PhotonTargets.All, i, control.R_XAxis, control.R_YAxis, control.L_XAxis, control.L_YAxis, control.LS_Held, control.TriggersR, control.TriggersL, control.currentlyGrounded);
						photonView.RPC ("SetPlayerTransform", PhotonTargets.All, players[i].rigidbody.position, players[i].rigidbody.rotation, players[i].rigidbody.velocity);
					}
				}
	        	/*photonView.RPC("PositionPlayer1", PhotonTargets.All, player1.transform.position, player1.transform.rotation);
	        	photonView.RPC("PositionPlayer2", PhotonTargets.All, player2.transform.position, player2.transform.rotation);
	        	photonView.RPC("PositionPlayer3", PhotonTargets.All, player3.transform.position, player3.transform.rotation);
	        	photonView.RPC("PositionPlayer4", PhotonTargets.All, player4.transform.position, player4.transform.rotation);*/
        
				sendTimer = SendInterval;
			}

			if (sendTimer2 <= 0){
				for(int i = 0; i < players.Length; i++){
					if (players[i].gameObject.GetActive()){
						ControllerScript control = players[i].playerController;
						
						//photonView.RPC ("SetPlayerPosition", PhotonTargets.All, i, players[i].transform.position);
					}
				}

				sendTimer2 = SendInterval2;
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
	public void ExchangeGoliathPositioning(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
		DecerealizeTransform(goliathTop, topPos, topRot);
		DecerealizeTransform(goliathBot, botPos, botRot);
	}
	[RPC]
	void SetPlayerPosition(int playerNum, Vector3 newPos){}

	[RPC]
	void SyncControllerInput(int playerId, float R_XAxis, float R_YAxis, float L_XAxis, float L_YAxis, bool LS_Held, float TriggersR, float TriggersL, bool isGrounded){}

	[RPC]
	void SetPlayerFacing(int playerId, Vector3 facing){}

	[RPC]
	void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}

	public void TogglePlayerADS (int playerNum, bool setADS){
		if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){

		}
	}
}
