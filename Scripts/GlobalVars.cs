using UnityEngine;
using System.Collections;

public class GlobalVars : MonoBehaviour {

	public int EXP_RATE = 1;
	public const int MAX_ROWS = 3;
	public const int MAX_COLS = 9;
	public const int MAX_DECKS = 10;

	public const int MAX_BUFFS = 5;
	public const int MAX_DEBUFFS = 5;

	protected GlobalVars() {
		// guarantee this will be always a singleton only - can't use the constructor!
	}

	void Start() {
	}
}
