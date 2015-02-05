using UnityEngine;
using System.Collections;

public class GoliathNetworking : MonoBehaviour {

	private string roomName = "GoliathConnection_083";
	private PhotonView photonView;
	private float sendTimer = 0.05f;	

	public GameObject GoliathTop;
	public GameObject GoliathBottom;

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
        	//Code here happens when lobby is all set up
        	//Photon RPC example
        	//photonView.RPC("SendInfoToServer", PhotonTargets.All, "hhhhe he he hi");
			if(sendTimer <=0){
        		SendGoliathTransforms(GoliathTop.transform, GoliathBottom.transform);
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

    private Transform DecerealizeTransform(Vector3 position, Quaternion rotation){
    	GameObject outputObject = new GameObject();
    	outputObject.transform.position = position;
    	outputObject.transform.rotation = rotation;
    	return outputObject.transform;
    }

    public void SendGoliathTransforms(Transform Top, Transform Bottom){
    	photonView.RPC("ExchangeGoliathPositioning", PhotonTargets.All, Top.position, Top.rotation, Bottom.position, Bottom.rotation);
    }
    [RPC]
    public void ExchangeGoliathPositioning(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot){
    	print("I'm sending the position over");
    }
}
