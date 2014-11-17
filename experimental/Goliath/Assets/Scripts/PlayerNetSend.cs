using UnityEngine;
using System.Collections;

public class PlayerNetSend : MonoBehaviour {

	private string serverIP = "192.168.56.1";
	private int port = 25000;
	private string gameName = "GoliathConnection_083";
	private string gameType = "MechvRegimentMatch";

	float countdownTimer = 5;
	bool stop = false;
	bool startCount = false;

	// Use this for initialization
	void Start () {
		if (Network.peerType == NetworkPeerType.Disconnected){
			bool useNat = !Network.HavePublicAddress();
			if(useNat){
				print("Network has no private address; NAT punching.");
			}
			Network.InitializeServer(10, port, useNat);
			MasterServer.RegisterHost(gameType, gameName, "This is the mech v regiment connection");
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (startCount){
			countdownTimer -= Time.deltaTime;
		}
		if (countdownTimer <= 0){
			countdownTimer = 5;
			print ("called");
			stop = true;
			MasterServer.RequestHostList(gameType);
			HostData[] hostList = MasterServer.PollHostList();
			print (hostList.Length);
			for(int i = 0; i < hostList.Length; i++){
				print (hostList[i].gameName);
				print("IP: " + hostList[i].ip[0] + " Port: "+ hostList[i].port);
			}
		}
	}

	void OnServerInitialized(){
		
	}
	void OnMasterServerEvent(MasterServerEvent mse){
		if (mse == MasterServerEvent.RegistrationSucceeded){
			print ("regSucceed");
			startCount = true;
		}
	}

	[RPC]
	void UpdateNathanPos(Vector3 pos){
		print ("BNOAEBJIRTF");
	}

}
