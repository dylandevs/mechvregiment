using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	private float SendInterval = 0.15f;
	private float sendTimer = 0;

	public GameObject goliathTop;
	public GameObject goliathBot;

	public Player[] players;

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
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	if(sendTimer <= 0){
                //Here's where the RPC calls go so they happen once properly joined.

				for(int i = 0; i < players.Length; i++){
					Player player = players[i];
					ControllerScript control = player.playerController;

					if (players[i].gameObject.GetActive()){
						photonView.RPC ("SetPlayerTransform", PhotonTargets.All, i, player.rigidbody.position, player.rigidbody.rotation, player.rigidbody.velocity);
						photonView.RPC ("UpdatePlayerAnim", PhotonTargets.All, i, control.forwardSpeed, control.rightSpeed, control.speed, control.isCrouching, control.isSprinting, control.aimingDownSight, player.GetCurrentWeapon().isFiring);
					}
				}
	        	        
				sendTimer = SendInterval;
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
	void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}

	[RPC]
	void UpdatePlayerAnim(int playerNum, float fwdSpeed, float rgtSpeed, float speed, bool crouching, bool sprinting, bool ads, bool firing){}
	
	[RPC]
	public void PlayerCycleWeapon(int playerNum, int newWeapon){}
	
	[RPC]
	public void PlayerJump(int playerNum){}
	
	[RPC]
	public void PlayerDeath(int playerNum, bool forward){}
	
	[RPC]
	public void PlayerRespawn(int playerNum){}
	
	[RPC]
	public void PlayerReload(int playerNum){}
}
