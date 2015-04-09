using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (CollectionManager))]
[RequireComponent (typeof (DeckListManager))]
[RequireComponent (typeof (LibraryManager))]

public class DeckBuilder : MonoBehaviour {

	public Canvas deckBuilder;
	public GameObject cardTemplate;
	public List<Card> collection = new List<Card>();
	public int maxCardsPerRow = 3;
	public int maxCardsPerColumn = 2;

	public RectTransform rowUIPrefab;

	void Start() {
		deckBuilder = GetComponent<Canvas>();
		collection = GetComponent<CollectionManager>().localCollection;
		//GameCard newCard = new GameCard();
	}
	
	void PrintCollection() {
		int maxCardsPerPage = maxCardsPerColumn * maxCardsPerRow;
		int numOfPages = Mathf.CeilToInt((float)collection.Count / (float)maxCardsPerPage);
		for (int x=0; x<numOfPages; x++) {
			//Create a new canvas-label-page prefab
			//GridLayoutGroup newPage = 
		}

	}

	void CreateNewCollectionPage() {
		//GameObject newPage = Instantiate(pageTemplate);

	}

	void WriteDeckToDeckList() {

	}

	public RectTransform newRow;

	void SomethingSomething() {
		for (int i=0; i<GlobalVars.MAX_ROWS; i++) {

			RectTransform newRow = Instantiate (rowUIPrefab);
			if (i != 0) {
				RectTransform previousRowRect = transform.parent.GetComponent<RectTransform>();
				newRow.offsetMin = new Vector2 (previousRowRect.offsetMin.x+10, previousRowRect.offsetMin.y+10);
				newRow.offsetMax = new Vector2 (previousRowRect.offsetMax.x-10, previousRowRect.offsetMax.y+10);
			}
			for (int j=0; j<GlobalVars.MAX_COLS; j++) {

			}
		}
	}

}
