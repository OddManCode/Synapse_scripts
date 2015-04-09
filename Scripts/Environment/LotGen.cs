using UnityEngine;
using System.Collections;

[RequireComponent (typeof (NetworkView))]

public class LotGen : MonoBehaviour {
	/* THIS CLASS POPULATES THE BLOCK IT IS ATTACHED TO WITH RANDOM BUILDINGS OF A RANDOM TYPE (SIZE)
	 * REQUIRES THE BLOCK PREFAB TO HAVE A "block_interior" GAMEOBJECT WITH A COLLIDER
	 * REQUIRES THERE TO BE AT LEAST 1 ASSET IN EACH RESOURCES DIRECTORY
	 */
	
	/* TO DO/TO FIX:	
	 * - improve functionality to determine coordinates of instantiated assets (-not- checking the size of a collider)
	 * - revisit general placement coding/ make it less-reliant 
	 * - is transform.Find().gameObject necessary?
	 * - clean up random rotation code... might require changing actual asset's original rotation
	 */

	void Start() {
		GenerateLots();
	}

	public void GenerateLots() {
		// GENERATES AND PLACES THE BUILDINGS WITHIN A BLOCK
		int lotType = Random.Range(0, 3);
		Object[] lots;
		int numOfLotsX;
		int numOfLotsZ;
		float xOffset = 0;
		float zOffset = 0;

		// Determine which type of buildings are going into the Block (1x1, 2x2, or 4x2)
		switch(lotType) {
		case 0:
			lots = Resources.LoadAll("Test Buildings/1x1");
			//Debug.Log("Lot Size: 1x1");
			//Debug.Log(lots.Length);
			numOfLotsX = 4;
			numOfLotsZ = 2;
			xOffset = transform.Find("block_interior").gameObject.GetComponent<Collider>().bounds.size.x/2.66f;
			zOffset = transform.Find("block_interior").gameObject.GetComponent<Collider>().bounds.size.z/4;
			break;
		case 1:
			lots = Resources.LoadAll("Test Buildings/2x2");
			//Debug.Log("Lot Size: 2x2");
			//Debug.Log(lots.Length);
			numOfLotsX = 2;
			numOfLotsZ = 1;
			xOffset = transform.Find("block_interior").gameObject.GetComponent<Collider>().bounds.size.x/4;
			break;
		case 2:
			lots = Resources.LoadAll("Test Buildings/2x4");
			//Debug.Log("Lot Size: 2x4");
			//Debug.Log(lots.Length);
			numOfLotsX = 1;
			numOfLotsZ = 1;
			break;
		default:
			Debug.Log("Defaulted! This should never happen!");
			lots = Resources.LoadAll("Test Buildings/2x4");
			//Debug.Log("Lot Size: 2x4");
			//Debug.Log(lots.Length);
			numOfLotsX = 1;
			numOfLotsZ = 1;
			break;
		}

		for(int i=0; i<Mathf.Abs(numOfLotsZ); i++) {
			for(int j=0; j<Mathf.Abs(numOfLotsX); j++) {
				// Instantiate a random building of the appropriate size
				GameObject lotPrefab = Instantiate(lots[Random.Range(0,lots.Length)]) as GameObject;
				//Debug Log for size check of Prefab's Collider
				//Debug.Log(lotPrefab.collider.bounds.size.x + " in x");
				//Debug.Log(lotPrefab.collider.bounds.size.z + " in z");
				lotPrefab.transform.position =  new Vector3(this.transform.position.x-xOffset + j*lotPrefab.GetComponent<Collider>().bounds.size.x, 0, this.transform.position.z-zOffset + i*lotPrefab.GetComponent<Collider>().bounds.size.z);
				if(lotType == 0 && i == 0) {
					lotPrefab.transform.rotation = Quaternion.Euler(0,180,0);
				}
				else if(lotType == 1) {
					if(j == 0) {
						lotPrefab.transform.rotation = Quaternion.Euler(0,270,0);
					}
					else {
						lotPrefab.transform.rotation = Quaternion.Euler(0,90,0);
					}
				}
				else if(lotType == 2) {
					lotPrefab.transform.rotation = Quaternion.Euler(0,Random.Range(0,1)*180,0);
				}

			}
		}
	}
}