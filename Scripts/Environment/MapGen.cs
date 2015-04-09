using UnityEngine;
using System.Collections;

public class MapGen : MonoBehaviour {
	/* THIS CLASS RANDOMLY GENERATES AND PLACES BLOCK PREFABS TO SERVE AS THE GAME MAP
	 * REQUIRES THE BLOCK PREFAB TO BE SET AND HAVE A COLLIDER THAT MATCHES ITS X AND Z DIMENSIONS
	 */
	
	/* TO DO/TO FIX:	
	 * - create a Resources directory with multiple block prefabs to instantiate: remove "public GameObject block"
	 * - instantiate extra "out of bounds" blocks so the playing field doesn't abruptly end
	 * - ^determine how to always place the "in bounds" blocks in the center of the tiled grid
	 */
	public GameObject block;

	public int length;
	public int width;

	public int seed;
	
	void Start() {
		//If the seed isn't set manually, get a random one from System
		if (seed == 0) {
			seed = (int) System.DateTime.Now.Ticks;
		}
		Random.seed = seed;
		Debug.Log ("Map Seed is: " + seed);

		GenerateMap();
	}
	
	private void GenerateMap() {
		for(int i=0; i<length; i++) {
			for(int j=0; j<width; j++) {
				GameObject blockPrefab = Instantiate(block) as GameObject;
				//Debug Log for size check of Prefab's Collider
				//Debug.Log(blockPrefab.collider.bounds.size.x + " in x");
				//Debug.Log(blockPrefab.collider.bounds.size.z + " in z");
				Vector3 pos = new Vector3(j*blockPrefab.GetComponent<Collider>().bounds.size.x, 0, i*blockPrefab.GetComponent<Collider>().bounds.size.z);
				blockPrefab.transform.position = pos;
			}
		}
	}
}