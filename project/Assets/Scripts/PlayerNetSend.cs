using UnityEngine;
using System.Collections;

public class PlayerNetSend : Photon.MonoBehaviour {

    private string roomName = "GoliathConnection_083";
	private float SendInterval = 0.15f;
	private float sendTimer = 0;

	public ScavGame game;

	public GameObject playerWrapper;
	private Player[] players;
	public PoolManager playerMineManager;
	//public PoolManager playerBulletManager;

	public PoolManager minionManager;
	private BotAI[] minions;
	private bool minionScriptsRetrieved = false;
	//public PoolManager minionBulletManager;

	public GoliathAvatar goliath;
	public PoolManager goliathTracerManager;
	public PoolManager goliathSparksManager;
	public PoolManager goliathMuzzleFlashManager;
	public PoolManager goliathPlasmaManager;
	public PoolManager goliathPlasmaExplosionManager;
	public PoolManager goliathMeteorManager;
	public PoolManager goliathMeteorExplosionManager;

	public GameObject goliathShield;
	public GameObject templeShield;
	public GameObject minionWaypoint;
	public GameObject crystal;

	public bool connectionReceived = false;

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
        if(PhotonNetwork.connectionStateDetailed.ToString() == "JoinedLobby"){
            Debug.Log(PhotonNetwork.connectionStateDetailed.ToString());
            MakeRoom();
        }
        else if(PhotonNetwork.connectionStateDetailed.ToString() == "Joined"){
			sendTimer -= Time.deltaTime;

			if (connectionReceived){

				if (!minionScriptsRetrieved){
					minions = new BotAI[minionManager.transform.childCount];
					for (int i = 0; i < minionManager.transform.childCount; i++){
						minions[i] = minionManager.transform.GetChild(i).GetComponent<BotAI>();
						if (minions[i].gameObject.GetActive()){
							photonView.RPC ("FirstTimeMinionActivation", PhotonTargets.All, minions[i].pooled.index, minions[i].transform.position);
						}
					}
					
					minionScriptsRetrieved = true;
				}


	        	if(sendTimer <= 0){
	                //Here's where the RPC calls go so they happen once properly joined.

					// Update player positions
					for(int i = 0; i < players.Length; i++){
						Player player = players[i];
						ControllerScript control = player.playerController;

						if (players[i].gameObject.GetActive() && !players[i].isDead){
							photonView.RPC ("SetPlayerTransform", PhotonTargets.All, i, player.GetComponent<Rigidbody>().position, player.GetComponent<Rigidbody>().rotation, player.GetComponent<Rigidbody>().velocity);
							photonView.RPC ("UpdatePlayerAnim", PhotonTargets.All, i, control.forwardSpeed, control.rightSpeed, control.speed, control.isCrouching,
							                control.isSprinting, control.aimingDownSight, player.GetCurrentWeapon().isFiring, control.spineJoint.transform.localRotation);
						}
					}

					// Update minion positions
					for(int i = 0; i < minions.Length; i++){
						if (minions[i].remoteId != -1){
							Transform minionTransform = minions[i].transform;
							NavMeshAgent minionAgent = minions[i].GetComponent<NavMeshAgent>();
							photonView.RPC ("SetMinionTransform", PhotonTargets.All, minions[i].remoteId, minionTransform.position, minionTransform.rotation, minionAgent.velocity);
						}
					}
		        	        
					sendTimer = SendInterval;
				}
			}
			else{
				if(sendTimer <= 0){
					photonView.RPC("ScavengerConnected", PhotonTargets.All, game.StartMatchTime);
					sendTimer = SendInterval;
				}
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

	// Game RPC
	[RPC]
	public void ScavengerWin(){}
	
	[RPC]
	public void GoliathWin(){}

	// Connection RPC
	[RPC]
	public void ScavengerConnected(float startTime){}

	[RPC]
	public void GoliathConnected(){
		print ("Goliath online");
		connectionReceived = true;
		game.GoliathOnline();
	}

	// Flag RPC
	[RPC]
	public void ScavengerDroppedFlag(Vector3 flagPos, int Player){}
	
	[RPC]
	public void ScavengerPickedUpFlag(int Player){}

	[RPC]
	public void GoliathDroppedFlag(Vector3 flagPos){
		game.FlagDropped(flagPos);
		goliath.carryEmitters.SetActive(false);
	}
	
	[RPC] 
	public void GoliathPickedUpFlag(){
		game.flag.SetActive(false);
		game.FlagRetrieved(goliath.botJoint.gameObject);
		goliath.carryEmitters.SetActive(true);
	}

	// Goliath RPC
	[RPC]
	void SetGoliathJoints(Vector3 topPos, Quaternion topRot, Vector3 botPos, Quaternion botRot, Vector3 botVel, Quaternion spineRot, Quaternion shoulderRRot, Quaternion shoulderLRot){
		goliath.SetNextTargetTransform(topPos, topRot, botPos, botRot, botVel, spineRot, shoulderRRot, shoulderLRot);
	}

	[RPC]
	void DamageGoliath(float damage, Vector3 direction){}

	[RPC]
	public void CreateGoliathTracer(Vector3 position, Vector3 direction){
		GameObject tracer = goliathTracerManager.Retrieve(position);
		tracer.transform.up = direction;

		TracerRoundScript tracerRound = tracer.GetComponent<TracerRoundScript>();
		tracerRound.sparkPool = goliathSparksManager;
		tracerRound.isAvatar = true;

		GameObject flash = goliathMuzzleFlashManager.Retrieve(position);
		flash.transform.up = direction;
		goliath.FireMinigun();
	}

	[RPC]
	public void CreateGoliathPlasma(Vector3 position, Quaternion rotation){
		GameObject plasma = goliathPlasmaManager.Retrieve(position, rotation);

		cannonShot plasmaScript = plasma.GetComponent<cannonShot>();
		plasmaScript.plasmaExplodePool = goliathPlasmaExplosionManager;
		plasmaScript.isAvatar = true;
		goliath.FireCannon();
	}

	[RPC]
	public void CreateGoliathMeteor(Vector3 position, Vector3 target){
		GameObject meteor = goliathMeteorManager.Retrieve(position);
		
		RocketScript meteorScript = meteor.GetComponent<RocketScript>();
		meteorScript.SetTarget(target);
		meteorScript.explosionPool = goliathMeteorExplosionManager;
		meteorScript.isAvatar = true;
	}

	[RPC]
	public void DamageGoliathShielded(float damage, Vector3 direction,Vector3 pos){}
	
	[RPC]
	public void BrokenShield(){
		templeShield.SetActive(false);
		goliathShield.SetActive(false);
		foreach (Player player in players){
			player.display.UpdateBrokenShield();
		}
	}

	[RPC]
	public void GoliathDashingStart(){
		goliath.StartDash();
	}
	
	[RPC]
	public void GoliathDashingEnd(){
		goliath.EndDash();
	}

	[RPC]
	public void GoliathDisabled(){
		goliath.SetDisabled();
	}

	[RPC]
	public void GoliathEnabled(){
		goliath.SetEnabled();
	}

	// Player RPC	
	[RPC]
	void SetPlayerTransform(int playerNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}
	
	[RPC]
	void UpdatePlayerAnim(int playerNum, float fwdSpeed, float rgtSpeed, float speed, bool crouching, bool sprinting, bool ads, bool firing, Quaternion spineRot){}

	[RPC]
	public void PlayerCycleWeapon(int playerNum, int newWeapon){}
	
	[RPC]
	public void PlayerJump(int playerNum){}
	
	[RPC]
	public void PlayerDeath(int playerNum, bool forward){}
	
	[RPC]
	public void PlayerRespawn(int playerNum, Vector3 respawnPos){}
	
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

	[RPC]
	public void LaunchPlayer(int playerNum){
		if (playerNum >= 0 && playerNum < players.Length){
			players[playerNum].Launch();
		}
	}

	// Minion RPC
	[RPC]
	void FirstTimeMinionActivation(int networkId, Vector3 position){}

	[RPC]
	void SetMinionTransform(int minionNum, Vector3 newPos, Quaternion newRot, Vector3 currVelocity){}

	[RPC]
	public void ApplyMinionDamage(int minionNum, float damage){
		if (minionNum >= 0 && minionNum < minions.Length){
			minions[minionNum].Damage (damage);
		}
	}

	[RPC]
	public void DestroyMinion(int networkId){}

	[RPC]
	public void SpawnNetworkedMinion(int networkId, Vector3 startPos){}

	[RPC]
	public void LinkMinions(int masterNum, int avatarNum){
		if (masterNum >= 0 && masterNum < minions.Length){
			minions[masterNum].remoteId = avatarNum;
		}
	}

	// Projectile RPC
	[RPC]
	public void CreateMine(int creatorId, Vector3 position, Vector3 direction){}

	[RPC]
	public void AffixMine(int networkId, Vector3 position){}

	[RPC]
	public void DetonateMine(int networkId){}

	[RPC]
	public void SetMineID(int creatorId, int networkId){
		if (creatorId >= 0 && creatorId < playerMineManager.transform.childCount){
			Mine mineScript = playerMineManager.transform.GetChild(networkId).GetComponent<Mine>();
			mineScript.remoteId = networkId;

			// Fix position
			if (!mineScript.gameObject.GetActive() || mineScript.isDetonated){
				photonView.RPC ("DetonateMine", PhotonTargets.All, networkId);
			}
		}
	}

	[RPC]
	public void CreateMinionBullet(Vector3 position, Vector3 facing, int minionNum){}

	[RPC]
	public void PlaceMinionWaypoint(Vector3 position){
		minionWaypoint.transform.position = position;

		foreach(BotAI minion in minions){
			minion.SetNewWaypoint();
		}
	}

	[RPC]
	public void CreatePlayerBullet(float damage, Vector3 position, int speed, Vector3 direction){}

	[RPC]
	public void CreatePlayerTracer(Vector3 tracerPos, Vector3 tracerDir){}

	[RPC]
	public void CreatePlayerBulletHit(Vector3 position, Vector3 normal, Quaternion rotation){}
}
