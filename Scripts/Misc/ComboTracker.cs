using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ComboTracker : MonoBehaviour {

	private Card[,] deck = new Card[GlobalVars.MAX_ROWS,GlobalVars.MAX_COLS];

	private Card[] combo = new Card[GlobalVars.MAX_ROWS];

	private int currentRow = 0;

	void Update() {
		if (Input.GetButtonDown("Card Slot 1")) {
			if (deck[currentRow,0].compatible) {
				combo[currentRow] = deck[currentRow,0];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 2")) {
			if (deck[currentRow,1].compatible) {
				combo[currentRow] = deck[currentRow,1];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 3")) {
			if (deck[currentRow,2].compatible) {
				combo[currentRow] = deck[currentRow,2];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 4")) {
			if (deck[currentRow,3].compatible) {
				combo[currentRow] = deck[currentRow,3];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 5")) {
			if (deck[currentRow,4].compatible) {
				combo[currentRow] = deck[currentRow,4];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 6")) {
			if (deck[currentRow,5].compatible) {
				combo[currentRow] = deck[currentRow,5];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 7")) {
			if (deck[currentRow,6].compatible) {
				combo[currentRow] = deck[currentRow,6];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 8")) {
			if (deck[currentRow,7].compatible) {
				combo[currentRow] = deck[currentRow,7];
				ComboCheck();
				currentRow+=1;
			}
		}
		if (Input.GetButtonDown("Card Slot 9")) {
			if (deck[currentRow,8].compatible) {
				combo[currentRow] = deck[currentRow,8];
				ComboCheck();
				currentRow+=1;
			}
		}

		if (combo.Length == GlobalVars.MAX_ROWS) {
			//activeCard = Combine(combo);
		}

		if (Input.GetButtonDown("Clear Hand")) {
			ResetCombo();
		}
	}

	/*
	Card Combine(Card[] reagents) {
		Card baseCard;
		for (int i=0; i<reagents.Length; i++) {
			if (reagents[i].category != 3) {
				baseCard = reagents[i];
			}
		}
		return baseCard;
	}*/

	void ComboCheck() {
		//For each Card in the combo array
		for (int x=0; x<combo.Length; x++) {
			//Create an array of common groupIDs equivalent to the groupIDs of the current combo Card
			int[] commonGIDs = combo[x].groupIDs;
			//If the combo contains more than 1 Card...
			if (combo.Length > 1) {
				//...the common groupIDs are the ones shared between the current common set and the current Card in the combo array
				commonGIDs = GetCommonGroupIDs(commonGIDs, combo[x].groupIDs);
			}
			//If there is at least 1 Non-Upgrade Card in the combo
			if (combo[x].category != 3) {
				//Highlight ONLY Upgrades that have at least 1 matching GroupID
				for (int i=x+1; i<GlobalVars.MAX_ROWS; i++) {
					for (int j=0; j<GlobalVars.MAX_COLS; j++) {
						if (HaveCommonGroupIDs(commonGIDs, deck[i,j].groupIDs) && deck[i,j].category == 3) {
							deck[i,j].compatible = true;
						}
					}
				}
			}
			//Otherwise highlight any Cards that have at least 1 matching GroupID
			else {
				for (int i=x+1; i<GlobalVars.MAX_ROWS; i++) {
					for (int j=0; j<GlobalVars.MAX_COLS; j++) {
						if (HaveCommonGroupIDs(commonGIDs, deck[i,j].groupIDs)) {
							deck[i,j].compatible = true;
						}
					}
				}
			}
		}
	}

	void ComboCheckAlt() {
		//In this alternate method, Upgrades can be combined as long as they have at least 1 groupID in common with each other Card
		for (int x=0; x<combo.Length; x++) {
			int[] compGIDs = combo[x].groupIDs;
			if (combo.Length > 1) {
				compGIDs = GetDistinctGroupIDs(compGIDs, combo[x].groupIDs);
			}
			//If there is at least 1 Non-Upgrade Card in the combo
			if (combo[x].category != 3) {
				//Highlight ONLY Upgrades that have at least 1 matching GroupID
				for (int i=x+1; i<GlobalVars.MAX_ROWS; i++) {
					for (int j=0; j<GlobalVars.MAX_COLS; j++) {
						if (HaveCommonGroupIDs(compGIDs, deck[i,j].groupIDs) && deck[i,j].category == 3) {
							deck[i,j].compatible = true;
						}
					}
				}
			}
			//Otherwise highlight any Cards that have at least 1 matching GroupID
			else {
				for (int i=x+1; i<GlobalVars.MAX_ROWS; i++) {
					for (int j=0; j<GlobalVars.MAX_COLS; j++) {
						if (HaveCommonGroupIDs(compGIDs, deck[i,j].groupIDs)) {
							deck[i,j].compatible = true;
						}
					}
				}
			}
		}
	}

	int[] GetDistinctGroupIDs(int[] group1, int[] group2) {
		IEnumerable<int> concatIDs = group1.Concat(group2);
		concatIDs = concatIDs.Distinct();
		int[] commonIDs = concatIDs.ToArray();
		return commonIDs;
	}

	void ResetCombo() {
		for (int i=0; i<GlobalVars.MAX_ROWS; i++) {
			for (int j=0; j<GlobalVars.MAX_COLS; j++) {
				if (i==0) {
					deck[i,j].compatible = true;
				}
				else {
					deck[i,j].compatible = false;
				}
			}
		}
	}

	int[] GetCommonGroupIDs(int[] group1, int[] group2) {
		return group1.Intersect(group2).ToArray();
	}

	bool HaveCommonGroupIDs(int[] group1, int[] group2) {
		if (group1.Intersect(group2).ToArray().Length != 0) {
			return true;
		}
		else {
			return false;
		}
	}
}