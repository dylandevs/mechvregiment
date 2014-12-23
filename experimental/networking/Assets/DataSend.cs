using UnityEngine;
using System.Collections;

public class DataSend : MonoBehaviour {
 
    private string serverIP = "172.17.50.225";
    private int port = 25000;
    private string _messageLog = "";
    string someInfo = "";
    private NetworkPlayer _myNetworkPlayer;
    private string gameName = "GoliathConnection_083";

    void Start(){
    }
 
    void OnGUI() {
    	GUI.Label(new Rect(100, 100, 150, 25), ""+Network.peerType);
        if (Network.peerType == NetworkPeerType.Disconnected) {
            if (GUI.Button(new Rect(100, 125, 150, 25), "connect")) {
                AddMessage("Connecting...");
                MasterServer.RequestHostList("MechvRegimentMatch");
                HostData[] hostList = MasterServer.PollHostList();
                AddMessage("Host list is thiiiiis long-> " + hostList.Length);
                for(int i = 0; i < hostList.Length; i++){
                    AddMessage(hostList[i].gameName);
                    if(hostList[i].gameName == gameName){
                        serverIP = hostList[i].ip[0];
                        port = hostList[i].port;
                    }
                }
                AddMessage("Starting client to connect to " + serverIP + ":" + port + ".");
                Network.Connect(serverIP, port);
            }
        } else {
            if (Network.peerType == NetworkPeerType.Client) {
 
                if (GUI.Button(new Rect(100, 125, 150, 25), "disconnect"))
                    Network.Disconnect();
 
                if (GUI.Button(new Rect(100, 150, 150, 25), "say hi to server"))
                    SendInfoToServer();
            }
        }
        GUI.TextArea(new Rect(250, 100, 300, 300), _messageLog);
    }
    [RPC]
    void OnConnectedToServer(){
    	AddMessage("Connected to the server.");
    }
    void OnDisconnectedToServer(){
    	AddMessage("Disconnected from the server.");
    }
    void OnFailedToConnect(NetworkConnectionError error){
    	AddMessage("Didn't connect because: "+error);
    }
    [RPC]
    void StartLogin(NetworkPlayer player){
        AddMessage("hahahahahahahahah yay login");
    }
    [RPC]
    void SendInfoToServer(){
        AddMessage("Sending stuff to server!");
        someInfo = "Client " + _myNetworkPlayer.guid + ": hello server";
        networkView.RPC("ReceiveInfoFromClient", RPCMode.All, someInfo);
        AddMessage ("SENT: "+someInfo);
    }
    [RPC]
    void SetPlayerInfo(NetworkPlayer player) {
        _myNetworkPlayer = player;
        someInfo = "Player setted";
        networkView.RPC("ReceiveInfoFromClient", RPCMode.All, someInfo);
        AddMessage ("SENT: "+someInfo);
    }
    [RPC]
    void ReceiveInfoFromServer(string someInfo) {
        AddMessage("GOT: "+someInfo);
    }
    void AddMessage(string message){
        _messageLog += message + "\n";
    }
 
    // fix RPC errors
    [RPC]
    void ReceiveInfoFromClient(string someInfo) {
	}
    [RPC]
    void SendInfoToClient(string someInfo) {
    }
}