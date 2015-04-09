using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.SqliteClient;

[RequireComponent(typeof(NetworkView))]

public class LibraryManager : MonoBehaviour {
	/* THIS CLASS HANDLES THE LIBRARY-RELATED FIELDS FOR A PLAYER
	 * AS A SERVER, IT RETRIEVES LIBRARY DATA FOR A GIVEN ACCOUNT FROM A LOCAL SQLITE TABLE 'libraries' AND SENDS IT TO THE APPROPRIATE PLAYER OVER THE NETWORK
	 * AS A CLIENT, IT RECEIVES DECKLIST DATA SENT OVER THE SERVER AND SENDS UPDATED DATA BACK
	 * LOCALLY, EACH PLAYER UPDATES THEIR OWN IN-GAME DATA, BUT UPDATES ARE SENT TO THE SERVER
	 */
	
	/* TO DO:
	 * Add methods that update the localLibrary accessible from other scripts (used when the player acquires or loses a Card through gameplay)
	 * Consider changing the localDeckList from a List to an Array
	 */

	private string dbPath;
	private NetworkView nView;

	public List<int[]> localLibrary = new List<int[]>();

	void Start() {
		dbPath = "URI=file:" + Application.dataPath + "/Data/localdata.sqlite";
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, Screen.height/2+32, Screen.width*0.25f, Screen.height/2));
		GUILayout.Label ("Local Library Size: " + localLibrary.Count);
		GUILayout.EndArea();
	}

	[RPC]
	public void GetLibraryData(int acctID, NetworkPlayer player) {
		//Retrieves library data of a given account ID
		Debug.Log ("Getting library data for Account ID >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();

		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT cardID, qty FROM libraries WHERE accountID='" + acctID + "';";
		Debug.Log (dbCmd.CommandText);
		IDataReader reader = dbCmd.ExecuteReader();
		Debug.Log ("Library data retrieved for Account >> " + acctID);

		while(reader.Read()) {
			if (player != Network.player) {
				GetComponent<NetworkView>().RPC ("AddToLocalLibrary", player, reader.GetInt32(0), reader.GetInt32(1));
			}
			else {
				AddToLocalLibrary(reader.GetInt32(0), reader.GetInt32(1));
			}
		}

		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	[RPC]
	public void AddToLocalLibrary(int cardID, int qty) {
		localLibrary.Add(new int[] {cardID, qty});
	}

	public void CreateNewAccountLibrary(int acctID) {
		//Inserts a default library records for the given account ID into the libraries table
		Debug.Log ("Creating new Library for Account ID >> " + acctID);

		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		for (int i=1; i<=5; i++) {
			//Set the command's text and execute it
			dbCmd.CommandText = "INSERT INTO libraries (accountID, cardID, qty) VALUES ('" + acctID + "', '" + i + "', '" + UnityEngine.Random.Range(3, 9) + "');";
			Debug.Log (dbCmd.CommandText);
			dbCmd.ExecuteNonQuery();
			Debug.Log ("New Library entry created for Account >> " + acctID);
		}
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}
	
	public void UpdateLibraryData(int acctID) {
		//Update the library information of a given accountID
		Debug.Log ("Updating library data for Account ID >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();

		foreach (int[] entry in localLibrary) {
			//Set the command's text and execute it
			dbCmd.CommandText = "SELECT cardID FROM libraries WHERE cardID='" + entry.GetValue(0) + "';";
			Debug.Log (dbCmd.CommandText);
			IDataReader reader = dbCmd.ExecuteReader();
			if (!reader.Read()) {
				Debug.Log("Account >> " + acctID + " does not have a record for this card, adding record.");
				
				//Set the command's text and execute it
				dbCmd.CommandText = "INSERT INTO libraries (accountID, cardID, qty) VALUES ('" + acctID + "', '"+ entry.GetValue(0) + "', '" + entry.GetValue(1) + "');";
				Debug.Log (dbCmd.CommandText);
				dbCmd.ExecuteNonQuery();
			}
			else {
				//Set the command's text and execute it
				dbCmd.CommandText = "UPDATE libraries SET cardID = '" + entry.GetValue(0) + "', qty = '" + entry.GetValue(1) + "' WHERE id='" + acctID + "';";
				Debug.Log (dbCmd.CommandText);
				dbCmd.ExecuteNonQuery();
			}
		}

		Debug.Log ("Library data updated on >> " + acctID);
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}
}
