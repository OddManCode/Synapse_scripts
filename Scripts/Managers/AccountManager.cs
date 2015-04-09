using UnityEngine;
using System.Collections;
using System.Data;
using Mono.Data.SqliteClient;

[RequireComponent (typeof (NetworkView))]
[RequireComponent (typeof (LibraryManager))]
[RequireComponent (typeof (DeckListManager))]

public class AccountManager : MonoBehaviour {
	/* THIS CLASS HANDLES THE ACCOUNT-RELATED FIELDS FOR A PLAYER
	 * AS A SERVER, IT RETRIEVES ACCOUNT DATA FROM A LOCAL SQLITE TABLE 'accounts' AND SENDS IT TO THE APPROPRIATE PLAYER OVER THE NETWORK
	 * AS A CLIENT, IT RECEIVES ACCOUNT DATA SENT OVER THE SERVER AND SENDS UPDATED DATA BACK
	 * LOCALLY, EACH PLAYER UPDATES THEIR OWN IN-GAME DATA, BUT UPDATES ARE SENT TO THE SERVER
	 */

	/* TO DO:
	 * Add functionality to prevent 2 of the same account from logging in at the same time
	 * Add functionality to delete an account as a client if you enter the name and password
	 */
	
	private string dbPath;

	private NetworkView nView;
	private LibraryManager libraryManager;
	private DeckListManager deckListManager;
	
	public int id;
	public string name;
	public int lvl;
	public int exp;
	public int cen;

	void Awake() {
		dbPath = "URI=file:" + Application.dataPath + "/Data/localdata.sqlite";
		//Reset pre-existing related components
		ResetComponents();
		//assign the NetworkView component
		nView = GetComponent<NetworkView>();
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(Screen.width*0.75f,0, Screen.width*0.25f, Screen.height));
		GUILayout.Label (dbPath);
		if (id > 0) {
			GUILayout.Label ("accountID >> " + id + " || exp >> " + exp);
		}
		GUILayout.EndArea();
	}

	void Update() {
		if (Input.GetButtonDown("Jump")) {
			exp += 100;
		}
	}

	public void ProcessAccountInput(string acctName, string pw, bool createNew, NetworkPlayer netPlayer) {
		//Determine whether to call account creation or account log-in functions

		//If the Server performed the call, create or log into the account locally
		if (Network.isServer) {
			Debug.Log("The server is...");
			if (createNew) {
				Debug.Log ("...creating a new account >> " + acctName);
				CreateNewAccount(acctName, pw);
			}
			else {
				Debug.Log ("...logging into account >> " + acctName);
				LoginAccount(acctName, pw, Network.player);
			}
		}
		//Otherwise, perform the RPC calls on the Server
		else {
			Debug.Log ("Client >> " + netPlayer.ToString() + " is...");
			if (createNew) {
				Debug.Log ("...creating a new account >> " + acctName);
				nView.RPC ("CreateNewAccount", RPCMode.Server, acctName, pw);
				nView.RPC ("GetAccountID", RPCMode.Server, acctName, pw);
			}
			else {
				Debug.Log ("...logging into account >> " + acctName);
				nView.RPC ("LoginAccount", RPCMode.Server, acctName, pw, netPlayer);
			}
		}
	}
	
	[RPC]
	public void LoginAccount (string acctName, string pw, NetworkPlayer netPlayer) {
		//SERVER-SIDE ONLY | Log a player into an account, retrieve its related data, and send the data to the appropriate scripts
		Debug.Log ("Player >> " + netPlayer.ToString()+ " is logging in");
		//Ensure the account input provided has a record
		if (AccountNameExists (acctName)) {
			if (GetAccountID(acctName, pw) > 0) {
				//Retrieve the account's ID, used in most other account-based function calls
				int getID = GetAccountID(acctName, pw);
				Debug.Log ("Player >> " + netPlayer.ToString() + " account ID >> " + getID + " retrieved");
				//If this function was called via RPC, 
				if (netPlayer != Network.player) {
					GetAccountData(getID, netPlayer);
					libraryManager.GetLibraryData(getID, netPlayer);
					deckListManager.GetDeckListData(getID, netPlayer);
				}
				else {
					GetAccountData(getID, Network.player);
					libraryManager.GetLibraryData(getID, Network.player);
					deckListManager.GetDeckListData(getID, Network.player);
				}
			}
		}
	}

	bool AccountNameExists (string acctName) {
		//Return true if the passed string matches an account name in the accounts table
		bool result = false;
		Debug.Log ("Checking If Account Name Exists >> " + acctName);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT name FROM accounts WHERE name='" + acctName + "';";
		Debug.Log (dbCmd.CommandText);
		IDataReader reader = dbCmd.ExecuteReader();
		//If the reader didn't retrieve any data...
		if (!reader.Read()) {
			//...then the result of false is returned
			Debug.Log("Account Name Does Not Exist >> " + acctName);
		}
		//Otherwise there is a record with the given name
		else {
			//So return true
			Debug.Log ("Account Name Exists >> " + acctName);
			result = true;
		}
		
		//Clean up
		reader.Close();
		reader = null;
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
		
		return result;
	}

	[RPC]
	public int GetAccountID (string acctName, string pw) {
		//SERVER-SIDE ONLY | Return the accountID of the record with the given name and password
		int id = 0;
		Debug.Log ("Getting Account ID >> " + acctName);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT id FROM accounts WHERE name='" + acctName + "' AND password='" + pw + "';";
		Debug.Log (dbCmd.CommandText);
		IDataReader reader = dbCmd.ExecuteReader();

		//If the reader retrieved data, then the name and password were correct
		if (reader.Read()) {
			//So set the retrieved ID to the return variable
			id = reader.GetInt32 (0);
			Debug.Log("ID Retrieved >> " + id);
		}
		else {
			Debug.Log ("Incorrect Password");
		}
		
		//Clean up
		reader.Close();
		reader = null;
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
		
		return id;
	}

	[RPC]
	public void GetAccountData(int acctID, NetworkPlayer netPlayer) {
		//SERVER-SIDE ONLY | Retrieve the data of a given accountID and send it to the player who called this method
		Debug.Log ("Getting data for Account ID >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT name, level, exp, cen FROM accounts WHERE id='" + acctID + "';";
		Debug.Log (dbCmd.CommandText);
		IDataReader reader = dbCmd.ExecuteReader();
		//If the reader retrieved account data
		if (reader.Read()) {
			//...assign it to local-scope variables
			string tempName = reader.GetString (0);
			int tempLvl = reader.GetInt32 (1);
			int tempExp = reader.GetInt32 (2);
			int tempCen = reader.GetInt32 (3);
			
			Debug.Log("Data Retrieved for ID >> " + acctID);
			
			//If this function was called via RPC, make sure the retrieved data is sent back to the player who called it
			if (netPlayer != Network.player) {
				nView.RPC ("SetLocalAccountData", netPlayer, acctID, tempName, tempLvl, tempExp, tempCen);
			}
			//Otherwise the server called this function locally, so assign the retrieved data to the local variables
			else {
				SetLocalAccountData(acctID, tempName, tempLvl, tempExp, tempCen);
			}
		}
		//Otherwise the accountID was incorrect
		else {
			Debug.Log ("Incorrect Account ID");
		}
		
		//Clean up
		reader.Close();
		reader = null;
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	[RPC]
	public void SetLocalAccountData(int acctID, string acctName, int acctLvl, int acctExp, int acctCen) {
		//Sets the local account data's variables
		id = acctID;
		name = acctName;
		lvl = acctLvl;
		exp = acctExp;
		cen = acctCen;
	}
	
	[RPC]
	public void UpdateAccountData(int acctID, int lvl, int exp, int cen) {
		//SERVER-SIDE ONLY | Update the account data of a given accountID
		Debug.Log ("Updating data for Account ID >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "UPDATE accounts SET level='" + lvl + "', exp='" + exp + "', cen='" + cen + "' WHERE id='" + acctID + "';";
		Debug.Log (dbCmd.CommandText);
		dbCmd.ExecuteNonQuery();
		Debug.Log ("Account updated >> " + acctID);
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	[RPC]
	public void CreateNewAccount(string acctName, string pw) {
		//SERVER-SIDE ONLY | Insert a new record into the 'accounts' table
		Debug.Log ("Creating New Account >> " + acctName);
		//If the account name doesn't already exist...
		if (!AccountNameExists(acctName)) {
			//Create a connection to the database, open it, and create a command
			IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
			dbCon.Open();
			IDbCommand dbCmd = dbCon.CreateCommand();
			
			//Set the command's text and execute it
			dbCmd.CommandText = "INSERT INTO accounts (name, password) VALUES ('" + acctName + "', '" + pw + "');";
			Debug.Log (dbCmd.CommandText);
			dbCmd.ExecuteNonQuery();
			Debug.Log ("New Account Created >> " + acctName);
			
			//Retrieve the newly-created account's ID
			int newAcctID = GetAccountID(acctName, pw);
			//Create the new account's library
			libraryManager.CreateNewAccountLibrary(newAcctID);
			//Create blank deck templates for the new account
			deckListManager.CreateNewAccountDeckList(newAcctID);
			
			//Clean up
			dbCmd.Dispose();
			dbCmd = null;
			dbCon.Close();
			dbCon = null;
		}
		//Otherwise the name was taken and cannot be used
		else {
			Debug.Log ("Account Name >> " + acctName + " Already Exists || Cannot Create New Account");
		}
	}
	
	[RPC]
	public void DeleteAccount(string acctName, string pw) {
		//SERVER-SIDE ONLY | Delete an account's record from the 'accounts', 'decks', and 'libraries' tables
		Debug.Log ("Deleting Account >> " + acctName);
		//Retrieve the account's ID
		int deleteID = GetAccountID(acctName, pw);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "DELETE FROM accounts WHERE id = '" + deleteID + "'; " +
			"DELETE FROM decks WHERE accountID = '" + deleteID + "'; " +
				"DELETE FROM libraries WHERE accountID = '" + deleteID + "';";
		Debug.Log (dbCmd.CommandText);
		dbCmd.ExecuteNonQuery();
		
		Debug.Log ("Account >> " + acctName + " deleted");
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	void OnDestroy() {
		if (Network.isServer) {
			UpdateAccountData(id, lvl, exp, cen);
			libraryManager.UpdateLibraryData(id);
			deckListManager.UpdateDeckListData(id);
		}

		if (Network.isClient) {
			nView.RPC("UpdateAccountData", RPCMode.Server, id, lvl, exp, cen);
			nView.RPC("UpdateLibraryData", RPCMode.Server, id);
			nView.RPC("UpdateDeckListData", RPCMode.Server, id);
		}
	}

	void OnDisconnectedFromServer() {
		if (Network.isServer) {
			UpdateAccountData(id, lvl, exp, cen);
			libraryManager.UpdateLibraryData(id);
			deckListManager.UpdateDeckListData(id);
		}
		
		if (Network.isClient) {
			nView.RPC("UpdateAccountData", RPCMode.Server, id, lvl, exp, cen);
			nView.RPC("UpdateLibraryData", RPCMode.Server, id);
			nView.RPC("UpdateDeckListData", RPCMode.Server, id);
			ResetComponents();
		}
	}

	void ResetComponents() {
		//Finds other account-related Manager scripts, destroys them, then adds them again
		libraryManager = GetComponent<LibraryManager>();
		Destroy(libraryManager);
		libraryManager = gameObject.AddComponent<LibraryManager>();
		
		deckListManager = GetComponent<DeckListManager>();
		Destroy(deckListManager);
		deckListManager = gameObject.AddComponent<DeckListManager>();
	}
}
