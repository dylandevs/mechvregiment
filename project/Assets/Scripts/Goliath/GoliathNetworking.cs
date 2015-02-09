using UnityEngine;
using System.Collections;

public class GoliathNetworking : MonoBehaviour {

	private string roomName = "GoliathConnection_083";
	private PhotonView photonView;
	private float sendTimer = 0.05f;	

	public GameObject GoliathTop;
	public GameObject GoliathBottom;

    public GameObject Player1Avatar;
    public GameObject Player2Avatar;
    public GameObject Player3Avatar;
    public GameObject Player4Avatar;

//INITIALIZATION

	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		photonView = PhotonView.Get (this);
	}
	
//UPDATE

	void Update () {
		sendTimer -= Time.deltaTime;

		if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            //Once connected to Photon, join the room or make the room.
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
        	if(sendTimer <= 0){
                //Here's where the RPC calls go so they happen once properly joined.

        		SendGoliathTransforms(GoliathTop.transform, GoliathBottom.transform);

                sendTimer = 0.05f;
			}
        }
        else {
        	Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
        }
	}

//CONNECTING / NETWORKING

    void MakeRoom(){
        RoomOptions roomOptions = new RoomOptions() { isVisible = false, maxPlayers = 2 };
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
    }
	private void OnPhotonJoinRoomFailed (){
        Debug.Log("Photon failed to join room.");
    }
    private void OnJoinedRoom(){
    	Room currentRoom = PhotonNetwork.room;
    	Debug.Log("Room \""+ currentRoom.name +"\" has this many joined: " + currentRoom.playerCount);
    }


//RPC CALLS

    public void SendGoliathTransforms(Transform Top, Transform Bottom){
    	photonView.RPC("ExchangeGoliathPositioning", PhotonTargets.All, Top.position, Top.rotation, Bottom.position, Bottom.rotation);
    }
    [RPC]
    public void ExchangeGoliathPositioning(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
    	print("I'm sending the position over");
    }

    [RPC]
    public void PositionPlayer1(Vector3 newPos, Quaternion newRot){
        Player1Avatar.transform.position = newPos;
        Player1Avatar.transform.rotation = newRot;
    }
    [RPC]
    public void PositionPlayer2(Vector3 newPos, Quaternion newRot){
        Player2Avatar.transform.position = newPos;
        Player2Avatar.transform.rotation = newRot;
    }
    [RPC]
    public void PositionPlayer3(Vector3 newPos, Quaternion newRot){
        Player3Avatar.transform.position = newPos;
        Player3Avatar.transform.rotation = newRot;
    }
    [RPC]
    public void PositionPlayer4(Vector3 newPos, Quaternion newRot){
        Player4Avatar.transform.position = newPos;
        Player4Avatar.transform.rotation = newRot;
    }
}
