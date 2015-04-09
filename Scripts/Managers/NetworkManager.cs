using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]

public class NetworkManager : MonoBehaviour {

	public HostData[] hostList;

	public int expRate;
	public int mapSeed;

	void Awake () {
		DontDestroyOnLoad(this);

		if (Application.loadedLevelName == "Setup") {
			Debug.Log("Setup Complete, loading Main Menu");
			Application.LoadLevel ("Main Menu");
		}
	}

	public void StartServer(string gameName, string serverName, string serverMessage, string serverPw, bool localOnly){
		int numConnections;
		
		if(localOnly) {
			numConnections = 0;
		}
		else {
			numConnections = 15;
		}
		
		Network.incomingPassword = serverPw;
		Network.InitializeServer(numConnections, 25000, !Network.HavePublicAddress());
		MasterServer.RegisterHost(gameName, serverName, serverMessage);
		Application.runInBackground = true;
		Debug.Log ("IP: " + Network.connectionTesterIP);
		Debug.Log ("Port: " + Network.connectionTesterPort);
	}
	
	public void RefreshHostList(string gameName) {
		MasterServer.ClearHostList();
		MasterServer.RequestHostList(gameName);
	}
	
	void OnMasterServerEvent(MasterServerEvent msEvent) {
		if(msEvent==MasterServerEvent.HostListReceived) {
			hostList = MasterServer.PollHostList();
		}
	}
	
	public void JoinServer(HostData hostList) {
		Network.Connect (hostList);
	}
	
	void OnConnectedToServer() {
		Debug.Log ("Network Manager >> Joined Server");

	}
	
	void OnServerInitialized() {
		Debug.Log ("Network Manager >> Server Initialized");
	}

	void OnPlayerConnected(NetworkPlayer player) {
		Debug.Log("Network Manager >> Player " + player.ToString() + " connected from " + player.ipAddress + ":" + player.port);
	}
	
	void OnPlayerDisconnected (NetworkPlayer player) {
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
	}
	
	public void Shutdown() {
		Network.Disconnect();
		MasterServer.UnregisterHost();
		Debug.Log ("Shutting Down...");
	}

	public void Disconnect() {
		if (Network.connections.Length == 1) {
			Debug.Log("Disconnecting: " + Network.connections[0].ipAddress + ":" + Network.connections[0].port);
			Network.CloseConnection(Network.connections[0], true);
		}
		else if (Network.connections.Length == 0) {
			Debug.Log("No one is connected");
		}
		else if (Network.connections.Length > 1) {
			Debug.Log("Too many connections. Are we running a server?");
		}
	}
}
