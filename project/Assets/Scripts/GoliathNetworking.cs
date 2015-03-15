﻿using UnityEngine;
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

	public PoolManager playerTracerManager;
	public PoolManager playerMineManager;
	public PoolManager playerBulletManager;
	public PoolManager playerMineExplosionManager;
	
	public PoolManager minionManager;
	private MinionAvatar[] minionAvatars;
	private bool minionScriptsRetrieved = false;
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
	public void CreateMinionBullet(){

	}
	
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
}
