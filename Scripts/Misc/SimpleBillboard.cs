using UnityEngine;
using System.Collections;

public class SimpleBillboard : MonoBehaviour
{
	public Camera camera;

	void Start() {
		if (camera == null) {
			camera = Camera.main;
		}
	}

	void Update() {
		if (camera != null) {
			transform.LookAt(transform.position + camera.transform.rotation * Vector3.back, camera.transform.rotation * Vector3.up);
		}
	}
}