using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class NewCharacterController : MonoBehaviour
{
	public GameObject target;

	public GameObject cameraGO;
	private CharacterCamera cameraScript;

	void Start () {
		cameraScript = cameraGO.GetComponent<CharacterCamera>();
	}

	void Update () {
		if (Input.GetButtonDown("Select Target")) {
			CheckTarget();
		}
	}

	void FixedUpdate () {
		Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		inputVector.Normalize();
	}

	void OrientCapsule () {
		float lateralSpeed = Mathf.Round (new Vector3(GetComponent<Rigidbody>().velocity.x,0,GetComponent<Rigidbody>().velocity.z).magnitude);
		
		if(target) {
			//Find the x and z position of the target, but along the character's horizontal plane and look at that point
			Vector3 targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
			transform.LookAt(targetPosition);
			//If the player and target are vertically aligned (one is directly over/under the other), clear the target
			if (CompareVectors(targetPosition, transform.position)) {
				ClearTarget();
			}
		}
		
		// Otherwise if the character is moving laterally, rotate in x and z to face the direction of the camera
		else if(Mathf.Floor (lateralSpeed) != 0) {
			Vector3 cameraDirection = new Vector3(transform.position.x - cameraGO.transform.position.x, 0, transform.position.z - cameraGO.transform.position.z);
			Vector3 newDir = Vector3.RotateTowards(transform.forward, cameraDirection, Time.deltaTime * lateralSpeed , 0.0f);
			transform.rotation = Quaternion.LookRotation(newDir);
		}
		
		//Otherwise, continue facing forward; camera can rotate around the character freely
		else {
			transform.rotation = Quaternion.LookRotation(transform.forward);
		}
	}

	void OrientPlayer () {
		//Vector3 horizontalDirection = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z);
		//player.transform.forward = Vector3.RotateTowards(player.transform.forward, horizontalDirection, Time.deltaTime * (1 + rigidbody.velocity.magnitude), 0.0f);
	}

	void CheckTarget () {

	}

	void ClearTarget () {

	}

	void Controller () {

	}

	private bool CompareVectors(Vector3 first, Vector3 second) {
		Vector3 comp1 = new Vector3(Mathf.Round (first.x), Mathf.Round (first.y), Mathf.Round (first.z));
		Vector3 comp2 = new Vector3(Mathf.Round (second.x), Mathf.Round (second.y), Mathf.Round (second.z));
		if (comp1 == comp2) {
			return true;
		}
		else {
			return false;
		}
	}
}