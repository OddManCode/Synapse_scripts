using UnityEngine;
using System.Collections;

public class CharacterCamera : MonoBehaviour {

	//The owning player's transform
	public Transform player;
	//The origin around which the camera rotates and follows
	public Transform origin;
	//The target
	public Transform target;
	//The home position
	public Transform resetNode;

	public float trackingSpeed;

	//Rotate variables
	public float cameraRotateSpeed;
	public float rotateDamping = 0.5f;
	
	private float mouseX;
	private float mouseY;
	private float rotSpeed;
	
	//Zoom variables
	public float cameraZoomSpeed;
	public float zoomDamping = 0.5f;
	
	private float scrollValue;
	private float zoomSpeed;
	private float zoomValue;

	void Start() {
		//Set up culling masks for the cameras
		float[] cullDistances = new float[32];
		cullDistances[LayerMask.NameToLayer ("Interiors")] = 100;
		//cullDistances[LayerMask.NameToLayer ("Actors")] = maxTargetDistance;
		GetComponent<Camera>().layerCullDistances = cullDistances;

		Mathf.Clamp (rotateDamping, 0, 1);
		Mathf.Clamp (zoomDamping, 0, 1);

	}

	void Update() {
		//Debug.Log ("Parent Local: " + player.localPosition);
		//Debug.Log ("Camera Local: " + transform.localPosition);
		if (target != null) {
			Ray targettingRay = new Ray(origin.position, target.position-origin.position);
			float ratio = Vector3.Distance(origin.position, target.position)/100;
			transform.position = targettingRay.GetPoint (-6*(1+ratio));

			Vector3 newDir = Vector3.RotateTowards(transform.forward, target.position-transform.position, trackingSpeed * Time.deltaTime, 0.0f);
			transform.rotation = Quaternion.LookRotation (newDir);

			//transform.LookAt(target.position);
			Debug.DrawRay(transform.position, target.position-transform.position, Color.yellow);
			//Debug.Log (Vector3.Distance(origin.position, target.position));
		}
		else {
			Rotate();
			Zoom();
		}

		Debug.DrawRay(transform.position, player.position-transform.position, Color.green);
	}

	public void ResetPosition() {
		transform.position = resetNode.position;
		transform.LookAt (origin.position);
	}

	public void SetTarget(Transform pass) {
		target = pass;
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
	
	void Zoom () {
		Ray zoomRay = new Ray(transform.position, origin.position-transform.position);
		
		if (Input.GetAxis("Mouse ScrollWheel") != 0) {
			scrollValue = Input.GetAxis ("Mouse ScrollWheel");
			zoomSpeed = cameraZoomSpeed;
		}
		else {
			zoomSpeed *= zoomDamping;
		}
		zoomValue = zoomSpeed * scrollValue;
		
		float zoomDistance = Vector3.Distance (origin.position, transform.position);
		
		if (zoomDistance < 2) {
			transform.Translate(-transform.forward, Space.World);
		}
		else if (zoomDistance > 12) {
			transform.Translate(transform.forward, Space.World);
		}
		else {
			transform.Translate(zoomRay.direction * zoomValue, Space.World);
		}
		Debug.DrawRay(transform.position, origin.position-transform.position, Color.green);
	}
}
