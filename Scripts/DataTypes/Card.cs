using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

[Serializable]
public class Card : IComparable<Card> {
	// THIS DATATYPE HOLDS THE BASIC, CONSTANT INFORMATION OF EACH CARD USED DURING THE GAME

	/* TO DO:
	 * Update the variables here when the fields in the table have changed to reflect the new card format (new/edited fields)
	 */

	// Variables that remain constant;
	public int id;
	public string title;
	public string imageName;
	public string modelName;
	public string description;
	
	public int category;
	public int type;
	public int rarity;

	// Base variables most often manipulated via other Scripts; 'Primary Stats'
	public int baseAlignment;
	public int basePower;
	public int baseRange;
	public int baseDuration;

	// Additional variables; cast time, combo chain/clip size, reload time, downtime
	public int baseDelay;
	public int baseCapacity;
	public int baseRecovery;
	public int baseCooldown;

	// Tags used to determine which other Cards this Card can combine with
	public int[] groupIDs;

	// If the Card has a unique special effect, this refers to which Script it calls
	// Possibly, eventually, remove the desc field if text from the Script can be pulled.
	public int scriptID;

	public bool compatible;
	public bool onCooldown = false;
	
	public Card() {
		id = 0;
		title = "";
		imageName = "";
		modelName = "";
		description = "";
		category = 0;
		type = 0;
		rarity = 0;
		baseAlignment = 0;
		basePower = 0;
		baseRange = 0;
		baseDuration = 0;
		baseDelay = 0;
		baseCapacity = 0;
		baseRecovery = 0;
		baseCooldown = 0;
		groupIDs = new int[1] {0};
		scriptID = 0;
	}

	public Card(int _id, string _title, string _imageName, string _modelName, string _description,
	            int _category, int _type, int _rarity,int _baseAlignment,
	            int _basePower, int _baseRange, int _baseDuration,
	            int _baseDelay, int _baseCapacity, int _baseRecovery,int _baseCooldown,
	            string _rawGroupIDs, int _scriptID) {
		id = _id;
		title = _title;
		imageName = _imageName;
		modelName = _modelName;
		description = _description;
		category = _category;
		type = _type;
		rarity = _rarity;
		baseAlignment = _baseAlignment;
		basePower = _basePower;
		baseRange = _baseRange;
		baseDuration = _baseDuration;
		baseDelay = _baseDelay;
		baseCapacity = _baseCapacity;
		baseRecovery = _baseRecovery;
		baseCooldown = _baseCooldown;
		groupIDs = Array.ConvertAll(_rawGroupIDs.Split(','), new Converter<string, int>(int.Parse));
		scriptID = _scriptID;

		//Debug.Log ("Card Initialized >> " + title);
	}

	public int CompareTo(Card other) {
		//For ID sorting
		if(other == null) {
			return 1;
		}
		return this.id.CompareTo(other.id);
	}
}
