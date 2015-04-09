using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Mono.Data.SqliteClient;

[RequireComponent (typeof (NetworkView))]

public class CollectionManager : MonoBehaviour {
	/* THIS CLASS HANDLES THE COLLECTION OF CARDS FOR A PLAYER
	 * AS A SERVER, IT RETRIEVES COLLECTION DATA FROM A LOCAL SQLITE TABLE 'collection' AND SENDS IT TO THE APPROPRIATE PLAYER OVER THE NETWORK
	 * AS A CLIENT, IT RECEIVES ACCOUNT DATA SENT OVER THE SERVER
	 * LOCALLY, EACH PLAYER HAS A COPY OF THE COLLECTION FROM WHICH OTHER SCRIPTS DRAW FROM, BUT NO CHANGES/UPDATES ARE MADE OR SENT
	 */
	
	/* TO DO:
	 * Update PopulateCollection() when the fields in the table have changed to reflect the new card format (new/edited fields)
	 */

	private string dbPath;
	private NetworkView nView;

	public List<Card> localCollection = new List<Card>();
	
	void Start() {
		dbPath = "URI=file:" + Application.dataPath + "/Data/localdata.sqlite";
	}

	void OnServerInitialized() {
		Debug.Log ("CollectionManager >> OnServerInitialized");
		PopulateCollection();
	}

	void OnGUI() {
		GUILayout.BeginArea(new Rect(0, Screen.height/2, Screen.width*0.25f, Screen.height/2));
		GUILayout.Label ("Local Collection Size: " + localCollection.Count);
		GUILayout.EndArea();
	}

	void DebugCollection() {
		foreach (Card printCard in localCollection) {
			Debug.Log("Card " + printCard.id + ": " + printCard.title);
		}
	}
	
	void OnDisconnectedFromServer() {
		if (!Network.isServer) {
			localCollection.Clear();
		}
	}

	[RPC]
	void ClearCollection() {
		Debug.Log ("Local collection cleared");
		localCollection.Clear();
	}

	void OnPlayerConnected(NetworkPlayer player) {
		/* When a player connects to the server
		 * clear that player's local collection data
		 * on the server, serialize and send each card to the player who connected
		 */
		Debug.Log ("CollectionManager >> Player " + player.ToString() + " connected, sending collection from server");
		GetComponent<NetworkView>().RPC ("ClearCollection", player);
		foreach (Card currCard in localCollection) {
			SerializeCard(player, currCard);
		}
		Debug.Log ("Collection serialized and transferred");
	}

	void SerializeCard(NetworkPlayer player, Card cardToSerialize) {
		//Debug.Log ("Serializing card >> " + cardToSerialize.title);
		BinaryFormatter binFormatter = new BinaryFormatter();
		MemoryStream memStream = new MemoryStream();

		binFormatter.Serialize(memStream, cardToSerialize);

		byte[] serializedCard = memStream.ToArray();

		memStream.Close();

		nView.RPC("SetCardData", player, serializedCard);
	}

	[RPC]
	void SetCardData(byte[] serializedCard) {
		BinaryFormatter binFormatter = new BinaryFormatter();
		MemoryStream memStream = new MemoryStream();

		memStream.Write(serializedCard, 0, serializedCard.Length);

		memStream.Seek(0, SeekOrigin.Begin);

		Card sentCard = (Card)binFormatter.Deserialize(memStream);

		localCollection.Add(sentCard);
	}

	void PopulateCollection() {
		Debug.Log ("Creating Collection...");

		//Clear the Collection first
		localCollection.Clear ();

		//Create a connection to the database, open it, and create a command
		IDbConnection dbCon = (IDbConnection) new SqliteConnection(dbPath);
		dbCon.Open();
		IDbCommand dbCmd = dbCon.CreateCommand();
		
		//Set the command's text and execute it
		dbCmd.CommandText = "SELECT * FROM collection;";
		IDataReader reader = dbCmd.ExecuteReader();
		
		Debug.Log ("Card Field Count >> " + reader.FieldCount);
		
		while(reader.Read()) {
			//DEBUGGING, PRINTS VALUES OF FIELDS FROM DATABASE
			/*
			for (int i=0; i<reader.FieldCount; i++) {
				Debug.Log ("Index " + i + " is of Type " + reader.GetFieldType (i) + " called " + reader.GetDataTypeName (i) + " with value " + reader.GetValue (i));
			}
			*/
			
			//Add a new Card object to the collection List using data from the reader
			localCollection.Add(new Card(
				reader.GetInt32(0),
				reader.GetString(1),
				reader.GetString(2),
				reader.GetString(3),
				reader.GetString(4), 
				reader.GetInt32(5),
				reader.GetInt32(6),
				reader.GetInt32(7),
				reader.GetInt32(8), 
				reader.GetInt32(9),
				reader.GetInt32(10),
				reader.GetInt32(11),
				reader.GetInt32(12), 
				reader.GetInt32(13),
				reader.GetInt32(14),
				reader.GetInt32(15),
				reader.GetString(16),
				reader.GetInt32(17)));
		}
		Debug.Log ("Collection Size >> " + localCollection.Count);

		/* Trying this AssetDatabase dealie...
		#if UNITY_EDITOR
		AssetDatabase.CreateAsset(localCollection,"Assets/Resources/Collection/NewCollectionManager.asset");
		AssetDatabase.SaveAssets();
		
		EditorUtility.FocusProjectWindow();
		Selection.activeObject = localCollection;
		#endif*/
		
		
		//Clean up
		reader.Close();
		reader = null;
		dbCmd.Dispose();
		dbCmd = null;
		dbCon.Close();
		dbCon = null;
	}

	public int SortByTitle(Card x, Card y) {
		//For title sorting
		return x.title.CompareTo(y.title);
	}
	
	public int SortByCategory(Card x, Card y) {
		//For category sorting
		return x.category.CompareTo(y.category);
	}
	
	public int SortByType(Card x, Card y) {
		//For type sorting
		return x.type.CompareTo(y.type);
	}
	
	public int SortByRarity(Card x, Card y) {
		//For rarity sorting
		return x.rarity.CompareTo(y.rarity);
	}
}
