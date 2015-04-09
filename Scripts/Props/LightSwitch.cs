using UnityEngine;
using System.Collections;

public class LightSwitch : MonoBehaviour {

	bool on = true;
	public Vector2 lightOnOffset;
	public Vector2 lightOffOffset;

	public Transform[] allChildren;

	// Use this for initialization
	void Start() {

	}

	void Update() {
		if (Input.GetButtonDown("Jump")){
			Switch();
		}
	}

	void TraverseHierarchy(Transform root) {
		foreach (Transform child in root) {
			TraverseHierarchy(child);
		}
	}
	
	void Switch () {
		allChildren = gameObject.GetComponentsInChildren<Transform>();
		if (on) {
			foreach (Transform child in allChildren) {
				if (child.gameObject.tag == "Light Mesh") {
					Material childMaterial = child.gameObject.GetComponent<Renderer>().material;
					childMaterial.mainTextureOffset = lightOffOffset;
					childMaterial.shader = Shader.Find("CustomToons/ToonAltLighting");
				}
				if (child.gameObject.tag == "Light Toggle") {
					if (child.GetComponent<Light>() != null) {
						child.GetComponent<Light>().enabled = false;
					}
					else if (child.GetComponent<MeshRenderer>() != null) {
						child.GetComponent<MeshRenderer>().enabled = false;
					}
					else if (child.GetComponent<SpriteRenderer>() != null) {
						child.GetComponent<SpriteRenderer>().enabled = false;
					}
				}
			}
			on = false;
		}
		else {
			foreach (Transform child in allChildren) {
				if (child.gameObject.tag == "Light Mesh") {
					Material childMaterial = child.gameObject.GetComponent<Renderer>().material;
					childMaterial.mainTextureOffset = lightOnOffset;
					childMaterial.shader = Shader.Find("Unlit/Texture");
				}
				if (child.gameObject.tag == "Light Toggle") {
					if (child.GetComponent<Light>() != null) {
						child.GetComponent<Light>().enabled = true;
					}
					else if (child.GetComponent<MeshRenderer>() != null) {
						child.GetComponent<MeshRenderer>().enabled = true;
					}
					else if (child.GetComponent<SpriteRenderer>() != null) {
						child.GetComponent<SpriteRenderer>().enabled = true;
					}
				}
			}
			on = true;
		}
	}
}
