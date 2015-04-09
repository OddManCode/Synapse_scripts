using UnityEngine;
using System.Collections;
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class CharacterControl1 : MonoBehaviour
{
	private Vector3 groundVelocity;
	private CapsuleCollider capsule;
	
	//Ground variables
	public float walkSpeed = 4.0f;
	public float runSpeed = 50.0f;
	public float acceleration = 2.0f;

	//Jump state variables
	private bool grounded = false;
	private bool jumpFlag = false;
	private float jumpTimer = 0.0f;
	private int jumpCount = 0;
	private float controlDamping = 1.0f;

	//Jump variables
	public int gravity = 2;
	public int additionalJumps = 2;
	public float jumpForce = 10.0f;
	public float maxJumpTime = 15.0f;
	public float airControlDamping = 0.8f;
	public int glideThreshold = 10;
	public float glideReduction = 2.0f;

	//Animation variables
	public GameObject player;
	private HeadAim headAimScript;
	private Animator playerAnimator;

	//Camera access
	public GameObject cameraGO;
	private CharacterCamera camera;

	//Target access
	private GameObject target;
	public float maxTargetDistance;

	void Awake()
	{
		capsule = GetComponent<CapsuleCollider>();
		GetComponent<Rigidbody>().freezeRotation = true;
	}

	void Start()
	{
		camera = cameraGO.GetComponent<CharacterCamera>();
		//headAimScript = player.GetComponent<HeadAim>();
		playerAnimator = player.GetComponent<Animator>();
	}

	void Update()
	{
		//headAimScript.maxCorrectionSpeed = rigidbody.velocity.magnitude;

		//Debug.Log ("velocity: " + rigidbody.velocity.magnitude);
		//Debug.Log ("jumpCount: " + jumpCount + " || maxJumps: " + additionalJumps + " || jumpTimer: " + jumpTimer + " || velocity.y: " + rigidbody.velocity.y);

		if (Input.GetButtonDown("Select Target")) {
			CheckTarget();
		}

		if (Input.GetButtonDown("Jump") && jumpCount < additionalJumps) {
			jumpTimer = 0;
			jumpCount++;
			GetComponent<Rigidbody>().velocity = new Vector3 (GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.z);
		}

		if (Input.GetButton ("Jump")) {
			if (jumpTimer < maxJumpTime) {
				jumpTimer++;
				jumpFlag = true;
			}
			else {
				jumpFlag = false;
			}
			//The player is falling
			if (GetComponent<Rigidbody>().velocity.y < -glideThreshold) {
				GetComponent<Rigidbody>().drag = glideReduction;
			}
		}
		else {
			jumpFlag = false;
			GetComponent<Rigidbody>().drag = 1;
		}

		if (!grounded) {
			controlDamping = airControlDamping;
		}
		else {
			controlDamping = 1;
			jumpCount = 0;
		}

		ParseAnimations();
		OrientCapsule();
		OrientPlayer();
	}

	void FixedUpdate()
	{
		// Cache lateral movement
		Vector3 inputVector = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
		inputVector.Normalize();

		// On the ground
		if (grounded)
		{
			// By setting the grounded to false in every FixedUpdate we avoid
			// checking if the character is not grounded on OnCollisionExit()
			grounded = false;
		}
		// In mid-air
		else
		{
			GetComponent<Rigidbody>().velocity -= (Vector3.up * gravity) / Mathf.Max(1, GetComponent<Rigidbody>().drag);
		}

		if (jumpFlag) {
			GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
		}

		Vector3 velocityChange = CalculateVelocityChange(inputVector) * controlDamping;
		GetComponent<Rigidbody>().AddForce(velocityChange, ForceMode.VelocityChange);

	}

	void ParseAnimations() {
		/*
		float lateralSpeed = new Vector3(rigidbody.velocity.x, 0, rigidbody.velocity.z).magnitude;
		playerAnimator.SetFloat ("hSpeed", lateralSpeed);
		playerAnimator.SetFloat ("vSpeed", rigidbody.velocity.y);
		playerAnimator.SetBool ("isGrounded", grounded);
		if (target) {
			playerAnimator.SetBool ("hasTarget", true);
		}
		else {
			playerAnimator.SetBool ("hasTarget", false);
		}
		*/
		//Debug.Log (controller.velocity.y);
		//playerAnimator.speed = Mathf.Max(rigidbody.velocity.magnitude/25,1);
	}

	//Orient the bounding capsule towards the target (if there is one) or face forwar
	void OrientCapsule() {

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
		
		//If the character is moving, rotate in x and z to face the direction of the camera
		else if(lateralSpeed != 0) {
			Vector3 direction = new Vector3(transform.position.x - cameraGO.transform.position.x, 0, transform.position.z - cameraGO.transform.position.z);
			Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, Time.deltaTime * lateralSpeed , 0.0f);
			transform.rotation = Quaternion.LookRotation(newDir);
		}
		
		//Otherwise, continue facing forward; camera can rotate around the character freely
		else {
			transform.rotation = Quaternion.LookRotation(transform.forward);
		}
	}
	
	void OrientPlayer() {
		Vector3 horizontalVelocity = new Vector3(GetComponent<Rigidbody>().velocity.x, 0, GetComponent<Rigidbody>().velocity.z);
		player.transform.forward = Vector3.RotateTowards(player.transform.forward, horizontalVelocity, Time.deltaTime * (1 + GetComponent<Rigidbody>().velocity.magnitude), 0.0f);
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
	
	// Unparent if we are no longer standing on our parent
	void OnCollisionExit(Collision collision)
	{
		if (collision.transform == transform.parent)
			transform.parent = null;
	}
	// If there are collisions check if the character is grounded
	void OnCollisionStay(Collision col)
	{
		TrackGrounded(col);
	}
	void OnCollisionEnter(Collision col)
	{
		TrackGrounded(col);
	}

	// From the user input calculate using the set up speeds the velocity change
	private Vector3 CalculateVelocityChange(Vector3 inputVector)
	{
		// Calculate how fast we should be moving
		Vector3 relativeVelocity = transform.TransformDirection(inputVector);
		relativeVelocity.z *= (Input.GetButton("Walk")) ? walkSpeed : runSpeed;
		relativeVelocity.x *= (Input.GetButton("Walk")) ? walkSpeed : runSpeed;
		// Calcualte the delta velocity
		Vector3 currRelativeVelocity = GetComponent<Rigidbody>().velocity - groundVelocity;
		Vector3 velocityChange = relativeVelocity - currRelativeVelocity;
		velocityChange.x = Mathf.Clamp(velocityChange.x, -acceleration, acceleration);
		velocityChange.z = Mathf.Clamp(velocityChange.z, -acceleration, acceleration);
		velocityChange.y = 0;
		return velocityChange;
	}

	// Check if the base of the capsule is colliding to track if it's grounded
	private void TrackGrounded(Collision collision)
	{
		float maxHeight = capsule.bounds.min.y + capsule.radius * .9f;
		foreach (var contact in collision.contacts)
		{
			if (contact.point.y < maxHeight)
			{
				if (isKinematic(collision))
				{
					// Get the ground velocity and we parent to it
					groundVelocity = collision.rigidbody.velocity;
					transform.parent = collision.transform;
				}
				else if (isStatic(collision))
				{
					// Just parent to it since it's static
					transform.parent = collision.transform;
				}
				else
				{
					// We are standing over a dynamic object,
					// set the groundVelocity to Zero to avoid jiggers and extreme accelerations
					groundVelocity = Vector3.zero;
				}
				// We are on a ground surface
				grounded = true;
			}
			break;
		}
	}
	private bool isKinematic(Collision collision)
	{
		return isKinematic(GetComponent<Collider>().transform);
	}
	private bool isKinematic(Transform transform)
	{
		return transform.GetComponent<Rigidbody>() && transform.GetComponent<Rigidbody>().isKinematic;
	}
	private bool isStatic(Collision collision)
	{
		return isStatic(collision.transform);
	}
	private bool isStatic(Transform transform)
	{
		return transform.gameObject.isStatic;
	}

	private void CheckTarget() {
		//If the ray cast to the mouse's position hits a valid target, set it to the camera's target
		Camera camComp = cameraGO.GetComponent<Camera>();
		Ray targetRay = camComp.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		//If the target is within the max distance, is an Actor, and is neither the current target nor the player character
		if(Physics.Raycast(targetRay, out hit, maxTargetDistance) && hit.transform.gameObject.tag == "Actor" && hit.transform != this.transform) { //|| hit.transform != target.transform)) {
			SetTarget(hit.transform);
		}
		//Otherwise, clear the target & reset the camera;
		else {
			ClearTarget();
		}
	}
	
	public void SetTarget(Transform passTarget) {
		Debug.Log("Camera target is: " + passTarget.gameObject.name);
		target = passTarget.gameObject;
		camera.SetTarget(passTarget);
		headAimScript.SetTarget(passTarget);
	}
	
	public void ClearTarget() {
		target = null;
		camera.SetTarget(null);
		camera.ResetPosition();
		headAimScript.SetTarget(null);
	}
}