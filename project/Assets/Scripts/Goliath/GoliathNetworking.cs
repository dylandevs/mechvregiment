using UnityEngine;
using System.Collections;

public class GoliathNetworking : Photon.MonoBehaviour {

	private string roomName = "GoliathConnection_083";
	//private PhotonView photonView;
	private float sendTimer = 0;
	private float SendInterval = 0.05f;

	public GameObject GoliathTop;
	public GameObject GoliathBottom;

	public PlayerAvatar[] playerAvatars;

	public PoolManager playerMineManager;
	public PoolManager playerBulletManager;
	
	public PoolManager minionManager;
	public PoolManager minionBulletManager;

//INITIALIZATION

	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		//photonView = PhotonView.Get (this);
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

	
				sendTimer = SendInterval;
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

	[RPC]
	void SetGoliathJoints(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot, Quaternion spineRot, Quaternion shoulderRRot, Quaternion shoulderLRot){}

	// Player RPC
	[RPC]
	public void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].SetNextTargetTransform(newPos, newRot, currVelocity);
		}
	}

	[RPC]
	public void UpdatePlayerAnim(int playerNum, float fwdSpeed, float rgtSpeed, float speed, bool crouching, bool sprinting, bool ads, bool firing){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].UpdateAnimValues(fwdSpeed, rgtSpeed, speed, crouching, sprinting, ads, firing);
		}
	}
	
	[RPC]
	public void PlayerCycleWeapon(int playerNum, int newWeapon){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].CycleNewWeapon(newWeapon);
		}
	}
	
	[RPC]
	public void PlayerJump(int playerNum){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].TriggerJump();
		}
	}
	
	[RPC]
	public void PlayerDeath(int playerNum, bool forward){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].DeathAnim(forward);
		}
	}
	
	[RPC]
	public void PlayerRespawn(int playerNum){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].TriggerRespawn();
		}
	}
	
	[RPC]
	public void PlayerReload(int playerNum){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].TriggerReload();
		}
	}

	[RPC]
	public void PlayerForceFire(int playerNum){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].TriggerFire();
		}
	}

	[RPC]
	public void ApplyPlayerDamage(int playerNum, float damage, Vector3 direction){}

	// Minion RPC
	[RPC]
	void SetMinionTransform(int minionNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){

	}
	
	[RPC]
	public void ApplyMinionDamage(int minionNum, float damage){}
	
	// Projectile RPC
	[RPC]
	public void CreateMine(){}
	
	[RPC]
	public void CreateMinionBullet(){}
	
	[RPC]
	public void CreatePlayerBullet(){}
}
