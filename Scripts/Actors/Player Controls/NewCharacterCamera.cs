using UnityEngine;
using System.Collections;

public class NewCharacterCamera : MonoBehaviour {

	public Transform origin;

	//Rotate variables
	public float cameraRotateSpeed;
	public float rotateDamping = 0.5f;
	
	private float mouseX;
	private float mouseY;
	private float rotSpeed;
	
	//Zoom variables
	//public float cameraZoomSpeed;
	//public float zoomDamping = 0.5f;
	
	private float scrollValue;
	private float zoomSpeed;
	private float zoomValue;

	public float maxCameraZoom;
	public float minCameraZoom;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		Rotate();
		NewZoom();
	}

	void Rotate () {
		if (Input.GetMouseButton(1)) {
			mouseX = Input.GetAxis ("Mouse X");
			mouseY = Input.GetAxis ("Mouse Y");
			rotSpeed = cameraRotateSpeed;
			Screen.lockCursor = true;
		}
		else {
			rotSpeed *= rotateDamping;
			Screen.lockCursor = false;
		}
		transform.RotateAround(origin.position, origin.up, mouseX * rotSpeed);
		transform.RotateAround(origin.position, transform.right, -mouseY * rotSpeed);
	}

	void NewZoom () {
		Ray zoomRay = new Ray (transform.position, origin.position-transform.position);

		if (Input.GetAxis("Mouse ScrollWheel") != 0) {
			scrollValue = Input.GetAxis ("Mouse ScrollWheel");
		}

		Vector3 offset = transform.position - origin.position;
		float zoomLength = offset.sqrMagnitude;
		Debug.Log (scrollValue + " || " + zoomLength + " || " + zoomRay.direction * scrollValue);

		if (zoomLength > minCameraZoom && zoomLength < maxCameraZoom) {
			transform.Translate(zoomRay.direction * scrollValue, Space.World);
		}
		else {
			//Mathf.Clamp (, minCameraZoom, maxCameraZoom);
		}

		transform.Translate(zoomRay.direction * zoomValue, Space.World);
		Debug.DrawRay(transform.position, origin.position-transform.position, Color.cyan);
	}
}
