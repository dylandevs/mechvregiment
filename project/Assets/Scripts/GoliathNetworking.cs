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
	public GameObject flag;

	public mechMovement mechHealth;
	public GoliathGameScript goliathGame;
	public cockpitUI cockpit;

	public GameObject playerAvatarWrapper;
	private PlayerAvatar[] playerAvatars;

	public PoolManager playerTracerManager;
	public PoolManager playerMineManager;
	public PoolManager playerBulletManager;
	public PoolManager playerMineExplosionManager;
	
	public PoolManager minionManager;
	private MinionAvatar[] minionAvatars;
	private bool minionScriptsRetrieved = false;
	public PoolManager minionBulletManager;

	public PoolManager scorchMarkManager;
	public PoolManager bulletHoleManager;
	public PoolManager sparkManager;
	public PoolManager shieldHitPool;

	public LayerMask shootableLayer;

//INITIALIZATION

	void Start () {
		PhotonNetwork.ConnectUsingSettings("v4.2");
		//photonView = PhotonView.Get (this);

		playerAvatars = new PlayerAvatar[playerAvatarWrapper.transform.childCount];
		for (int i = 0; i < playerAvatarWrapper.transform.childCount; i++){
			playerAvatars[i] = playerAvatarWrapper.transform.GetChild(i).GetComponent<PlayerAvatar>();
			//playerAvatars[i].PlayerNum = i + 1;
		}
	}
	
//UPDATE

	void Update () {
		if (!minionScriptsRetrieved){
			minionAvatars = new MinionAvatar[minionManager.transform.childCount];
			for (int i = 0; i < minionManager.transform.childCount; i++){
				minionAvatars[i] = minionManager.transform.GetChild(i).GetComponent<MinionAvatar>();
				if (minionAvatars[i].gameObject.GetActive()){
					minionAvatars[i].remoteId = i;
				}
			}

			minionScriptsRetrieved = true;
		}

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

	// Game RPC
	[RPC]
	void ScavengerWin(){
		goliathGame.goliathLost();
	}

	[RPC]
	void GoliathWin(){
		goliathGame.goliathWon();
	}

	// Goliath RPC
	[RPC]
	void SetGoliathJoints(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot, Vector3 botVel, Quaternion spineRot, Quaternion shoulderRRot, Quaternion shoulderLRot){}

	[RPC]
	void GoliathConnected(){}

	[RPC]
	void DamageGoliath(float damage, Vector3 direction){
		// Apply damage to Goliath
		mechHealth.takeDamage(damage,direction);
	}

	[RPC]
	public void GoliathDashingStart(){}

	[RPC]
	public void GoliathDashingEnd(){}

	[RPC]
	public void ScavengerConnected(float startTime){
		goliathGame.netWorkReady = true;
		goliathGame.remainingTime = startTime;
	}

	[RPC]
	public void ScavengerDroppedFlag(Vector3 flagPos,int Player){
		flag.transform.position = flagPos;
		flag.SetActive(true);
		cockpit.droppedFlag(Player);
		playerAvatars [Player].DropFlag ();
	}

	[RPC]
	public void ScavengerPickedUpFlag(int Player){
		flag.SetActive(false);
		cockpit.switchToFlag(Player);
		cockpit.updateObjectiveDiamond(playerAvatars[Player].gameObject);
		playerAvatars [Player].CarryFlag ();
	}

	[RPC]
	void DamageGoliathShielded(float damage, Vector3 direction,Vector3 pos){
		// Apply damage to Goliath
		mechHealth.takeDamage(damage,direction);
		GameObject shieldHit = shieldHitPool.Retrieve(pos);
		shieldHit.transform.up = -direction;
	}


	[RPC]
	public void BrokenShield(){}

	[RPC]
	public void GoliathDroppedFlag(Vector3 flagPos){}

	[RPC] 
	public void GoliathPickedUpFlag(){}

	[RPC]
	public void CreateGoliathTracer(Vector3 position, Vector3 direction){}

	[RPC]
	public void CreateGoliathPlasma(Vector3 position, Quaternion rotation){}

	[RPC]
	public void CreateGoliathMeteor(Vector3 position, Vector3 target){}

	// Player RPC
	[RPC]
	public void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].SetNextTargetTransform(newPos, newRot, currVelocity);
		}
	}

	[RPC]
	public void UpdatePlayerAnim(int playerNum, float fwdSpeed, float rgtSpeed, float speed, bool crouching, bool sprinting, bool ads, bool firing, Quaternion spineRot){
		if (playerNum >= 0 && playerNum < playerAvatars.Length){
			playerAvatars[playerNum].UpdateAnimValues(fwdSpeed, rgtSpeed, speed, crouching, sprinting, ads, firing, spineRot);
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

	[RPC]
	public void LaunchPlayer(int playerNum){}

	// Minion RPC
	[RPC]
	void SetMinionTransform(int minionNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){
		if (minionNum >= 0 && minionNum < minionAvatars.Length){
			minionAvatars[minionNum].SetNextTargetTransform(newPos, newRot, currVelocity);
		}
	}
	
	[RPC]
	public void ApplyMinionDamage(int minionNum, float damage){}

	[RPC]
	public void DestroyMinion(int networkId){
		if (networkId >= 0 && networkId < minionAvatars.Length){
			if (minionAvatars[networkId].gameObject.GetActive()){
				minionAvatars[networkId].Kill();
			}
		}
	}

	[RPC]
	public void SpawnNetworkedMinion(int networkId, Vector3 startPos){
		GameObject spawnedMinion = minionManager.Retrieve(startPos);
		MinionAvatar avatarScript = spawnedMinion.GetComponent<MinionAvatar>();
		photonView.RPC ("LinkMinions", PhotonTargets.All, networkId, avatarScript.pooled.index);
	}

	[RPC]
	public void LinkMinions(int masterNum, int avatarNum){}
	
	// Projectile RPC
	[RPC]
	public void CreateMine(int creatorId, Vector3 position, Vector3 direction){
		GameObject projectile = playerMineManager.Retrieve(position);
		
		Mine mineScript;
		if (mineScript = projectile.GetComponent<Mine>()){
			mineScript.explosionPool = playerMineExplosionManager;
			mineScript.goliathNetworker = this;
			mineScript.remoteId = creatorId;
			projectile.transform.position += direction * 2;
			projectile.rigidbody.AddForce(direction * 1000);

			mineScript.isAvatar = true;
			photonView.RPC ("SetMineID", PhotonTargets.All, creatorId, mineScript.pooled.index);
		}
	}

	[RPC]
	public void AffixMine(int networkId, Vector3 position){
		if (networkId >= 0 && networkId < playerMineManager.transform.childCount){
			Mine mineScript = playerMineManager.transform.GetChild(networkId).GetComponent<Mine>();
			mineScript.SetAffixedPosition(position);
		}
	}

	[RPC]
	public void DetonateMine(int networkId){
		if (networkId >= 0 && networkId < playerMineManager.transform.childCount){
			Mine mineScript = playerMineManager.transform.GetChild(networkId).GetComponent<Mine>();
			mineScript.Detonate();
		}
	}

	[RPC]
	public void SetMineID(int creatorId, int networkId){}
	
	[RPC]
	public void CreateMinionBullet(Vector3 position, Vector3 facing, int minionNum){
		GameObject bullet = minionBulletManager.Retrieve(position);
		bullet.transform.forward = facing;
		MinionBullet bulletScript = bullet.GetComponent<MinionBullet>();
		bulletScript.shootableLayer = shootableLayer;
		bulletScript.bulletMarkPool = scorchMarkManager;
		bulletScript.isAvatar = true;

		if (minionNum >= 0 && minionNum < minionAvatars.Length){
			minionAvatars[minionNum].TriggerFlash();
		}
	}

	[RPC]
	public void PlaceMinionWaypoint(Vector3 position){}
	
	[RPC]
	public void CreatePlayerBullet(float damage, Vector3 position, int speed, Vector3 direction){
		GameObject projectile = playerBulletManager.Retrieve(position);

		Bullet bulletScript;
		if (bulletScript = projectile.GetComponent<Bullet>()){
			// Setting bullet properties
			bulletScript.setProperties(damage, direction, speed, bulletHoleManager);
			bulletScript.shootableLayer = shootableLayer;
			bulletScript.isAvatar = true;
		}
	}

	[RPC]
	public void CreatePlayerTracer(Vector3 tracerPos, Vector3 tracerDir){
		GameObject tracer = playerTracerManager.Retrieve (tracerPos);
		tracer.transform.forward = tracerDir;
	}

	[RPC]
	public void CreatePlayerBulletHit(Vector3 position, Vector3 normal, Quaternion rotation){
		sparkManager.Retrieve(position, rotation);
		bulletHoleManager.Retrieve(position + normal * 0.01f, rotation);
	}
}
