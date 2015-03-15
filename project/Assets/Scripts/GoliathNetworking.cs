using UnityEngine;
using System.Collections;

public class GoliathNetworking : Photon.MonoBehaviour {

	private string roomName = "GoliathConnection_083";
	//private PhotonView photonView;
	private float sendTimer = 0;
	private float SendInterval = 0.15f;

	public GameObject goliathTop;
	public GameObject goliathBot;
	public GameObject goliathSpine;
	public GameObject goliathShoulderR;
	public GameObject goliathShoulderL;

	public mechMovement mechHealth;

	public GameObject playerAvatarWrapper;
	private PlayerAvatar[] playerAvatars;

	public PoolManager playerMineManager;
	public PoolManager playerBulletManager;
	
	public PoolManager minionManager;
	public PoolManager minionBulletManager;

	public PoolManager bulletHoleManager;
	public PoolManager sparkManager;

	public LayerMask shootableLayer;

//INITIALIZATION

	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		//photonView = PhotonView.Get (this);

		playerAvatars = new PlayerAvatar[playerAvatarWrapper.transform.childCount];
		for (int i = 0; i < playerAvatarWrapper.transform.childCount; i++){
			playerAvatars[i] = playerAvatarWrapper.transform.GetChild(i).GetComponent<PlayerAvatar>();
		}
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
				photonView.RPC ("SetGoliathJoints", PhotonTargets.All, goliathTop.transform.position, goliathTop.transform.rotation,
				                goliathBot.transform.position, goliathBot.transform.rotation, goliathBot.rigidbody.velocity,
				                goliathSpine.transform.rotation, goliathShoulderR.transform.rotation, goliathShoulderL.transform.rotation);
	
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

	// Goliath RPC
	[RPC]
	void SetGoliathJoints(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot, Vector3 botVel, Quaternion spineRot, Quaternion shoulderRRot, Quaternion shoulderLRot){}

	[RPC]
	void DamageGoliath(float damage, Vector3 direction){
		// Apply damage to Goliath
		mechHealth.takeDamage(damage,direction);
	}

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
		Transform minionWrapper = minionManager.transform;

		if (minionNum >= 0 && minionNum < minionWrapper.childCount){
			MinionAvatar avatar = minionManager.transform.GetChild(minionNum).GetComponent<MinionAvatar>();
			// TODO: minion props
		}
	}
	
	[RPC]
	public void ApplyMinionDamage(int minionNum, float damage){}
	
	// Projectile RPC
	[RPC]
	public void CreateMine(){

	}
	
	[RPC]
	public void CreateMinionBullet(){

	}
	
	[RPC]
	public void CreatePlayerBullet(float damage, Vector3 position, int speed, Vector3 direction){
		GameObject projectile = playerBulletManager.Retrieve(position);

		Bullet bulletScript;
		Mine mineScript;
		if (bulletScript = projectile.GetComponent<Bullet>()){
			// Setting bullet properties
			bulletScript.setProperties(damage, direction, speed, bulletHoleManager);
			bulletScript.shootableLayer = shootableLayer;
		}
	}
}
