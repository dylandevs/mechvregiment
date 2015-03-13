using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	private float SendInterval = 0.15f;
	private float sendTimer = 0;

	public GameObject playerWrapper;
	private Player[] players;
	public PoolManager playerMineManager;
	public PoolManager playerBulletManager;

	public PoolManager minionManager;
	public PoolManager minionBulletManager;

	public GoliathAvatar goliath;

	// Use this for initialization
	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		//photonView = PhotonView.Get (this);

		players = new Player[playerWrapper.transform.childCount];
		for (int i = 0; i < playerWrapper.transform.childCount; i++){
			players[i] = playerWrapper.transform.GetChild(i).GetComponent<Player>();
		}
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

				// Update player positions
				for(int i = 0; i < players.Length; i++){
					Player player = players[i];
					ControllerScript control = player.playerController;

					if (players[i].gameObject.GetActive()){
						photonView.RPC ("SetPlayerTransform", PhotonTargets.All, i, player.rigidbody.position, player.rigidbody.rotation, player.rigidbody.velocity);
						photonView.RPC ("UpdatePlayerAnim", PhotonTargets.All, i, control.forwardSpeed, control.rightSpeed, control.speed, control.isCrouching, control.isSprinting, control.aimingDownSight, player.GetCurrentWeapon().isFiring);
					}
				}

				// Update minion positions
				Transform minionWrapper = minionManager.transform;
				for(int i = 0; i < minionWrapper.childCount; i++){
					Transform minionTransform = minionWrapper.GetChild(i);


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

	[RPC]
	void SetGoliathJoints(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot, Vector3 botVel, Quaternion spineRot, Quaternion shoulderRRot, Quaternion shoulderLRot){
		goliath.SetNextTargetTransform(topPos, topRot, botPos, botRot, botVel, spineRot, shoulderLRot, shoulderLRot);
	}

	[RPC]
	void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}

	// Player RPC
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

	[RPC]
	public void PlayerForceFire(int playerNum){}

	[RPC]
	public void ApplyPlayerDamage(int playerNum, float damage, Vector3 direction){
		if (playerNum >= 0 && playerNum < players.Length){
			players[playerNum].Damage(damage, direction);
		}
	}

	// Minion RPC
	[RPC]
	void SetMinionTransform(int minionNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}

	[RPC]
	public void ApplyMinionDamage(int minionNum, float damage){
		if (minionNum >= 0 && minionNum < minionManager.transform.childCount){
			BotAI minion = minionManager.transform.GetChild(minionNum).GetComponent<BotAI>();
			minion.Damage (damage);
		}
	}

	// Projectile RPC
	[RPC]
	public void CreateMine(){}

	[RPC]
	public void CreateMinionBullet(){}

	[RPC]
	public void DestroyMinionBullet(){

	}

	[RPC]
	public void CreatePlayerBullet(){}

	[RPC]
	public void DestroyPlayerBullet(){

	}
}
