using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]
[RequireComponent (typeof (AccountManager))]
[RequireComponent (typeof (NetworkManager))]

public class MenuManager: MonoBehaviour {

	const string gameName = "SYNAPSE";
	
	private string serverName = "<Unnamed Server>";
	private string serverMessage = "";
	private string serverPw = "";
	private bool localOnly = false;
	private int numConnections;

	private string acctName = "admin";
	private string pw = "admin";
	private bool createNew = false;

	private NetworkManager network;
	private AccountManager account;

	private NetworkView nView;
	
	private enum Menu {Setup, ServerStart, ServerConnect, ServerDisconnect, Login, Account, DeckEditor, Blank};

	Menu serverMenu = Menu.Setup;
	Menu accountMenu = Menu.Blank;

	void Start() {
		network = GetComponent<NetworkManager>();
		account = GetComponent<AccountManager>();

		nView = GetComponent<NetworkView>();
	}

	void OnGUI() {
		if (Application.loadedLevelName == "Main Menu") {
			switch(serverMenu) {
			case Menu.Setup:
				RenderSetup();
				break;
			case Menu.ServerStart:
				RenderServerStart();
				break;
			case Menu.ServerConnect:
				RenderServerConnect();
				break;
			case Menu.ServerDisconnect:
				RenderServerDisconnect();
				break;
			}
		}

		if (Application.loadedLevelName == "Main Menu" && (Network.isClient || Network.isServer)) {
			switch(accountMenu) {
			case Menu.Login:
				RenderLogin();
				break;
			case Menu.Account:
				RenderAccount();
				break;
			case Menu.DeckEditor:
				RenderDeckEditor();
				break;
			}
		}

		if (Network.isServer || Network.isClient) {
			RenderConnectionInfo();
		}
	}

	void RenderBlank() {

	}

	void RenderSetup() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));
		
		GUILayout.BeginHorizontal();
		if(GUILayout.Button ("Start A Server")) {
			serverMenu = Menu.ServerStart;
		}
		if(GUILayout.Button ("Join A Server")) {
			network.RefreshHostList(gameName);
			serverMenu = Menu.ServerConnect;
		}
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	
	void RenderServerStart() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Server Name: ", GUILayout.MaxWidth(100));
		serverName = GUILayout.TextField(serverName, 40, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		/*
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Server IP: ", GUILayout.MaxWidth(100));
		serverIP = GUILayout.TextField(serverIP, 40, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		 */
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Description: ", GUILayout.MaxWidth(100));
		serverMessage = GUILayout.TextField(serverMessage, 60, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Password: ", GUILayout.MaxWidth(100));
		serverPw = GUILayout.PasswordField(serverPw, "*"[0], 20, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		localOnly = GUILayout.Toggle(localOnly, "Local Only?");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button ("Start Server")) {
			network.StartServer(gameName, serverName, serverMessage, serverPw, localOnly);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Back")) {
			serverMenu = Menu.Setup;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	
	void RenderServerConnect() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(GUILayout.Button ("Refresh Server List")) {
			network.RefreshHostList(gameName);
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if(network.hostList!=null) {
			for(int i=0;i<network.hostList.Length;i++) {
				GUILayout.BeginHorizontal();
				GUILayout.Box(network.hostList[i].gameName + " || " +
				              network.hostList[i].connectedPlayers + " || " +
				              network.hostList[i].comment, GUILayout.MaxWidth(600));
				if(GUILayout.Button ("Join")) {
					network.JoinServer(network.hostList[i]);
				}
				GUILayout.EndHorizontal();
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Back")) {
			serverMenu = Menu.Setup;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}

	void RenderServerDisconnect() {
		if (Network.isServer) {
			if(GUILayout.Button ("Shutdown Server")) {
				network.Shutdown();
				serverMenu = Menu.Setup;
			}
		}
		else {
			if(GUILayout.Button ("Disconnect from Server")) {
				network.Disconnect();
				serverMenu = Menu.Setup;
			}
		}
	}
	
	void RenderConnectionInfo() {
		if (Network.isServer) {
			GUILayout.Label ("Server: " + serverName);
			GUILayout.Label ("# Connections: " + Network.connections.Length);
			GUILayout.Label ("Connected as: " + nView.owner.ToString() + " || " + nView.viewID);
		}
		if (Network.isClient) {
			GUILayout.Label ("Client: " + serverName);
			GUILayout.Label ("# Connections: " + Network.connections.Length);
			GUILayout.Label ("Connected as: " + nView.owner.ToString() + " || " + nView.viewID);
		}
	}

	void RenderLogin() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Account Name: ", GUILayout.MaxWidth(100));
		acctName = GUILayout.TextField(acctName, 40, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Box("Password: ", GUILayout.MaxWidth(100));
		pw = GUILayout.PasswordField(pw, "*"[0], 20, GUILayout.MaxWidth(200));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		createNew = GUILayout.Toggle(createNew, "New Account?");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (createNew) {
			if (GUILayout.Button ("Create Account")) {
				createNew = false;
				account.ProcessAccountInput(acctName, pw, createNew, Network.player);
			}
		}
		else {
			if(GUILayout.Button ("Login")) {
				account.ProcessAccountInput(acctName, pw, createNew, Network.player);
			}
		}
		if (account.id > 0) {
			accountMenu = Menu.Account;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (Network.isServer) {
			if(GUILayout.Button ("Delete Account")) {
				account.DeleteAccount(acctName, pw);
			}
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Back")) {
			//currentMenu = Menu.Main;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	
	void RenderAccount() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Build Decks")) {
			accountMenu = Menu.DeckEditor;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Logout")) {
			LogoutAccount();
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}

	void RenderDeckEditor() {
		GUILayout.BeginArea(new Rect(Screen.width/2 - 300, Screen.height/2 - 100, 600, 400));

		GUILayout.BeginHorizontal();

		GUILayout.EndHorizontal();

		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button ("Back")) {
			accountMenu = Menu.Account;
		}
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		GUILayout.EndArea();
	}

	void LogoutAccount() {
		Destroy(account);
		account = gameObject.AddComponent<AccountManager>();
		accountMenu = Menu.Login;
	}

	void OnConnectedToServer() {
		serverMenu = Menu.ServerDisconnect;
		accountMenu = Menu.Login;
	}

	void OnDisconnectedFromServer() {
		if (!Network.isServer) {
			serverMenu = Menu.Setup;
			accountMenu = Menu.Blank;
		}
	}

	void OnServerInitialized() {
		serverMenu = Menu.ServerDisconnect;
		accountMenu = Menu.Login;
	}
}
