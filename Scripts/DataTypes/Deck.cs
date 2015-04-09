using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/*
 * The Deck class handles adding, subtracting, updating, and re-organizing a List of Card objects
 */

[Serializable]
public class Deck : IComparable<Deck> {
	// THIS DATATYPE HOLDS THE BASIC, CONSTANT INFORMATION OF EACH CARD USED DURING THE GAME
	
	/* TO DO:
	 * the constructor can have paramters that do not match the MAX_ROWS variable.
	 * 	Consider potential work-arounds, or make the number of rows exact and unchangeable (3 is the standard)
	 */

	public string deckName;

	public int[,] cardIDs = new int[GlobalVars.MAX_ROWS, GlobalVars.MAX_COLS];

	public Deck() {
		deckName = "[empty deck]";

		Array.Clear(cardIDs,0,cardIDs.Length);
	}

	public Deck(string _deckName, string _firstRow, string _secondRow, string _thirdRow) {
		deckName = _deckName;

		int[] firstRowIDs = Array.ConvertAll(_firstRow.Split(','), new Converter<string, int>(int.Parse));
		int[] secondRowIDs = Array.ConvertAll(_secondRow.Split(','), new Converter<string, int>(int.Parse));
		int[] thirdRowIDs = Array.ConvertAll(_thirdRow.Split(','), new Converter<string, int>(int.Parse));

		for (int i=0; i<GlobalVars.MAX_ROWS; i++) {
			for (int j=0; j<GlobalVars.MAX_COLS; j++) {
				switch(i) {
				case 0:
					cardIDs[i,j] = firstRowIDs[j];
					break;
				case 1:
					cardIDs[i,j] = secondRowIDs[j];
					break;
				case 2:
					cardIDs[i,j] = thirdRowIDs[j];
					break;
				}
			}
		}
	}

	public int CompareTo(Deck other)
	{
		if(other == null) {
			return 1;
		}
		//Return alphabetical sort
		return String.Compare(this.deckName, other.deckName);
	}

	public string RowToString(int rowNum) {
		string convert = "";
		for (int j=0; j<GlobalVars.MAX_COLS; j++) {
			if (j == GlobalVars.MAX_COLS-1) {
				convert += cardIDs[rowNum,j].ToString();
			}
			else {
				convert += cardIDs[rowNum,j].ToString() + ",";
			}

		}
		Debug.Log(convert);
		return convert;
	}
}
