using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.SqliteClient;

[RequireComponent (typeof (CollectionManager))]

public class DeckListManager : MonoBehaviour {
	/* THIS CLASS HANDLES THE DECKLIST-RELATED FIELDS FOR A PLAYER
	 * AS A SERVER, IT RETRIEVES DECKLIST DATA FOR A GIVEN ACCOUNT FROM A LOCAL SQLITE TABLE 'decks' AND SENDS IT TO THE APPROPRIATE PLAYER OVER THE NETWORK
	 * AS A CLIENT, IT RECEIVES DECKLIST DATA SENT OVER THE SERVER AND SENDS UPDATED DATA BACK
	 * LOCALLY, EACH PLAYER UPDATES THEIR OWN IN-GAME DATA, BUT UPDATES ARE SENT TO THE SERVER
	 */
	
	/* TO DO:
	 * Add methods that update the localDeckList accessible from other scripts (used in Deck-building)
	 * Consider changing the localDeckList from a List to an Array.
	 * 	The number of Decks each account can have should be a set amount, so a List leaves this open to having more local/server decks when retrieving/sending
	 */

	private string dbPath;
	private NetworkView nView;
	public List<Deck> localDeckList = new List<Deck>();

	void Start() {
		dbPath = "URI=file:" + Application.dataPath + "/Data/localdata.sqlite";
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, Screen.height/2+64, Screen.width*0.25f, Screen.height/2));
		GUILayout.Label ("Local DeckList Size: " + localDeckList.Count);
		GUILayout.EndArea();
	}

	[RPC]
	public void GetDeckListData(int acctID, NetworkPlayer player) {

		Debug.Log ("Retrieveing all Decks for Account >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT deckName, firstRow, secondRow, thirdRow FROM decks WHERE accountID='" + acctID + "';";
		Debug.Log (dbCmd.CommandText);
		IDataReader reader = dbCmd.ExecuteReader();

		while(reader.Read()) {
			if (player != Network.player) {
				GetComponent<NetworkView>().RPC ("AddToLocalDeckList", player, reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
			}
			else {
				AddToLocalDeckList(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
			}
		}
		Debug.Log (localDeckList.Count + " Decks retrieved for Account >> " + acctID);

		//Clean up
		reader.Close();
		reader = null;
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	[RPC]
	public void AddToLocalDeckList(string deckName, string firstRow, string secondRow, string thirdRow) {
		localDeckList.Add(new Deck(deckName, firstRow, secondRow, thirdRow));
	}

	public void CreateNewAccountDeckList(int acctID) {
		//Inserts new, empty deck records for the given account ID into the decks table
		Debug.Log ("Creating empty deckLists for new Account ID >> " + acctID);

		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();

		for (int i=0; i<GlobalVars.MAX_DECKS; i++) {
			//Set the command's text and execute it
			dbCmd.CommandText = "INSERT INTO decks (accountID, deckName, firstRow, secondRow, thirdRow) VALUES ('" + acctID + "', '[empty deck]', '0,0,0,0,0,0,0,0,0', '0,0,0,0,0,0,0,0,0', '0,0,0,0,0,0,0,0,0');";
			Debug.Log (dbCmd.CommandText);
			dbCmd.ExecuteNonQuery();
			Debug.Log ("New deckList entry created for Account >> " + acctID);
		}
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	[RPC]
	public void UpdateDeckListData(int acctID) {
		//Update the deckList information of a given accountID
		Debug.Log ("Updating deckList data for Account ID >> " + acctID);
		
		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		foreach (Deck currDeck in localDeckList) {
			//Set the command's text and execute it
			dbCmd.CommandText = "UPDATE decks SET deckName = '" + currDeck.deckName + "', " +
				"firstRow = '" + currDeck.RowToString(0) + "', " +
				"secondRow = '" + currDeck.RowToString(1) + "', " +
				"thirdRow = '" + currDeck.RowToString(2) + "' WHERE id='" + acctID + "';";
			Debug.Log (dbCmd.CommandText);
			dbCmd.ExecuteNonQuery();
		}

		Debug.Log ("deckList data updated on >> " + acctID);
		
		//Clean up
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}
}
