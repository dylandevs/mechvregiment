using UnityEngine;
using System.Collections;

public class DataGet : MonoBehaviour {

	private int port = 25000;
    private string _messageLog = "Bootering server...";
    private string gameName = "GoliathConnection_083";

	// Use this for initialization
	void Start () {
	
	}
	
	public void Awake(){
		if (Network.peerType == NetworkPeerType.Disconnected){
            bool useNat = !Network.HavePublicAddress();
            if(useNat){
                AddMessage("Network has no private address; NAT punching.");
            }/*
            MasterServer.ipAddress = "83.221.146.11";
            MasterServer.port = 23457;
            port = 23457;*/
            Network.InitializeServer(10, port, useNat);
            MasterServer.RegisterHost("MechvRegimentMatch", gameName, "This is the mech v regiment connection");
        }
	}
    void OnServerInitialized(){
        AddMessage("Server initialized.");
    }
    void OnMasterServerEvent(MasterServerEvent mse){
        if (mse == MasterServerEvent.RegistrationSucceeded){
            AddMessage("Registration succeeded.");
            MasterServer.RequestHostList("MechvRegimentMatch");
            HostData[] hostList = MasterServer.PollHostList();
            AddMessage("Host list is thiiiiis long-> " + hostList.Length);
            for(int i = 0; i < hostList.Length; i++){
                AddMessage(hostList[i].gameName);
                AddMessage("IP: " + hostList[i].ip[0] + " Port: "+ hostList[i].port);
            }
        }
    }
	// Update is called once per frame
	void Update () {
	
	}

	 public void OnGUI() {
        GUI.Label(new Rect(100, 100, 150, 25), ""+Network.peerType);
        if (Network.peerType == NetworkPeerType.Server) {
            GUI.Label(new Rect(100, 125, 150, 25), "Clients attached: " + Network.connections.Length);
 
            if (GUI.Button(new Rect(100, 150, 150, 25), "Quit server")) {
                AddMessage("Quitting...");
                Network.Disconnect();
                Application.Quit();
            }
            if (GUI.Button(new Rect(100, 175, 150, 25), "Send hi to client"))
                SendInfoToClient();
            if (GUI.Button(new Rect(100, 200, 150, 25), "Get host list")){
                MasterServer.RequestHostList("MechvRegimentMatch");
                HostData[] hostList = MasterServer.PollHostList();
                AddMessage("Host list is thiiiiis long-> " + hostList.Length);
                for(int i = 0; i < hostList.Length; i++){
                    AddMessage(hostList[i].gameName);
                    AddMessage("IP: " + hostList[i].ip[0] + " Port: "+ hostList[i].port);
                }
            }
        }
        GUI.TextArea(new Rect(275, 100, 300, 300), _messageLog);

    }
    /*
    [RPC]
    void OnPlayerConnected(NetworkPlayer player) {
        AskClientForInfo(player);
        AddMessage("Player connected. Now at "+Network.connections.Length);
    }*/
    void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player connected");
    }
    [RPC]
    void StartLogin() {
    }
    [RPC]
    void AskClientForInfo(NetworkPlayer player) {
        networkView.RPC("SetPlayerInfo", player, player);
    }
    void AddMessage(string message){
        _messageLog += message + "\n";
    }
    [RPC]
    void ReceiveInfoFromClient(string someInfo) {
        AddMessage(someInfo);
    }
 
    string someInfo = "Server: hello client";
    [RPC]
    void SendInfoToClient() {
        networkView.RPC("ReceiveInfoFromServer", RPCMode.All, someInfo);
    }
 
    // fix RPC errors
    [RPC]
    void SendInfoToServer() { }
    [RPC]
    void SetPlayerInfo(NetworkPlayer player) { }
    [RPC]
    void ReceiveInfoFromServer(string someInfo) { }
}
