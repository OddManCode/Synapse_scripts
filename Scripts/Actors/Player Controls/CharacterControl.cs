using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]

public class CharacterControl : MonoBehaviour {

	public float walkSpeed;// = 4;
	public float runSpeed;// = 50;

	//The current lateral speed
	public float horizontalSpeed;
	//The speed that acceleration will adjust to
	private float targetSpeed;
	//The rate of acceleration/deceleration
	private float accel;

	//Initial speed applied vertically when jumping, "thrust"
	public float jumpSpeed;// = 7;
	//How long, in milliseconds, jumpSpeed thrust is applied
	public float maxJumpTime;// = 15;
	//How quickly the character is pulled back towards the ground
	public float gravity;// = 120;
	//How responsive lateral controls are while in the air
	public float airControl;// = 0.8f;
	//How much Gliding reduces descent speed by
	public float fallSpeedReduction;// = 5;
	//Number of times the player can jump before touching the ground again, includes the initial jump
	public int maxJumps;// = 3;
	//How fast the player must be descending before Gliding will work again
	public float glideThreshold;// = 50;
	//Number of seconds between
	public float dashDelay;// = 3;
	//The max range a target can be acquired from
	public float maxTargetDistance;

	//Base value applied to other objects in a rigidbody collision
	public float pushForce = 2.0f;
	//How massive the player is, only affects rigidbody collision, not gravity
	public float mass = 6.0f;
	
	private short jumpCount = 0;
	private float jumpTimer = 0;
	private float fallModifyFactor = 1;
	private float dashTimer;
	
	public GameObject player;
	private HeadAim headAimScript;
	private Animator playerAnimator;

	private CharacterController controller;
	private Vector3 moveDirection;

	public GameObject cameraGO;
	private CharacterCamera camera;

	private GameObject target;
	
	float lastSyncTime = 0.0f;
	float syncDelay = 0.0f;
	float syncTime = 0.0f;
	Vector3 syncStartPos = Vector3.zero;
	Vector3 syncEndPos = Vector3.zero;

	void Start() {
		dashTimer = dashDelay;
		controller = GetComponent<CharacterController>();
		camera = cameraGO.GetComponent<CharacterCamera>();
		headAimScript = player.GetComponent<HeadAim>();
		playerAnimator = player.GetComponent<Animator>();
	}
	
	void Update() {
		Movement();
		Actions();
		ParseAnimations();

		/*
		if(networkView.isMine) {
			GetInput();
		} else {
			SyncedMovement();
		}*/
		/*
		//Use for falling calculations
		RaycastHit hit;
		if(Physics.Raycast(transform.position, -Vector3.up, out hit, 100.0F)) {
			float distanceToGround = hit.distance;
			Debug.Log(distanceToGround);
		}
		*/
	}

	void LateUpdate() {
		OrientController();
		OrientPlayer();
	}

	void ParseAnimations() {
		playerAnimator.SetFloat ("hSpeed", horizontalSpeed);
		playerAnimator.SetFloat ("vSpeed", controller.velocity.y);
		if (target) {
			playerAnimator.SetBool ("hasTarget", true);
		}
		else {
			playerAnimator.SetBool ("hasTarget", false);
		}
		//Debug.Log (controller.velocity.y);
		playerAnimator.speed = Mathf.Max(controller.velocity.magnitude/25,1);
	}

	void Movement() {
		Vector3 moveDirection = CalculateHorizontalSpeed();
		moveDirection = new Vector3 (moveDirection.x, CalculateVerticalSpeed(controller.velocity.y), moveDirection.z);
		moveDirection = transform.TransformDirection(moveDirection);
		controller.Move(moveDirection * Time.deltaTime);
		//headAimScript.maxCorrectionSpeed = controller.velocity.magnitude;
	}

	Vector3 CalculateHorizontalSpeed() {
		//If a movement key is being held
		if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0) {
			//If the walk button is being held
			if (Input.GetButton("Walk")) {
				//Set target speed to walking speed
				targetSpeed = walkSpeed;
			}
			else {
				//Otherwise set target speed to running speed
				targetSpeed = runSpeed;
			}
		}
		//Otherwise set target speed to 0
		else {
			targetSpeed = 0;
		}

		Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
		horizontalSpeed = horizontalVelocity.magnitude;

		//accel = Mathf.Lerp(horizontalSpeed, targetSpeed, Time.deltaTime);
		//Debug.Log (accel);

		Vector3 lateralMovement = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

		if (controller.isGrounded) {
			lateralMovement = Vector3.Normalize(lateralMovement) * Mathf.Lerp (horizontalSpeed, targetSpeed, Time.deltaTime);
		}
		else {
			lateralMovement = Vector3.Normalize(lateralMovement) * Mathf.Lerp (horizontalSpeed, targetSpeed * airControl, Time.deltaTime);
		}

		
		//Debug.Log ("Current hspeed: " + horizontalSpeed + " || target speed: " + targetSpeed + " || accel: " + accel);
		return lateralMovement;
	}

	float CalculateVerticalSpeed(float currentVelocity) {
		if(Input.GetButtonDown("Jump")) {
			if(jumpCount < maxJumps) {
				jumpCount++;
				jumpTimer = 0;
				if(currentVelocity < 0) {
					currentVelocity = jumpSpeed;
				}
			}
			else if(currentVelocity < -glideThreshold){
				currentVelocity /= fallSpeedReduction;
			}
		}
		if(Input.GetButton("Jump")) {
			if(jumpTimer < maxJumpTime) {
				jumpTimer++;
				currentVelocity += jumpSpeed;
			}
			
			if(currentVelocity < -glideThreshold) {
				fallModifyFactor = fallSpeedReduction;
			}
		}
		if(Input.GetButtonUp("Jump")) {
			fallModifyFactor = 1;
		}

		//If character is on the ground, 0 out y Movement and reset jumping stats
		if(controller.isGrounded && !Input.GetButton("Jump")) {
			jumpCount = 0;
			jumpTimer = 0;
			fallModifyFactor = 1;
			//currentVelocity = 0;
		}
		//Otherwise, apply gravity, adjusted by the falling factor
		else {
			currentVelocity -= gravity / fallModifyFactor * Time.deltaTime;
		}
		return currentVelocity;
	}

	void OrientController() {
		//Debug.Log(controller.velocity.x);
		if(target) {
			//Find the x and z position of the target, but along the character's horizontal plane and look at that point
			Vector3 targetPosition = new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z);
			transform.LookAt(targetPosition);
			if (CompareVectors(targetPosition, transform.position)) {
				ClearTarget();
			}
		}
		
		//If the character is moving, rotate in x and z to face the direction of the camera
		else if((controller.velocity.x != 0 || controller.velocity.z != 0)) {
			Vector3 direction = new Vector3(transform.position.x - cameraGO.transform.position.x, 0, transform.position.z - cameraGO.transform.position.z);
			Vector3 newDir = Vector3.RotateTowards(transform.forward, direction, Time.time, 0.0f);
			transform.rotation = Quaternion.LookRotation(newDir);
		}
		
		//Otherwise, continue facing forward; camera can rotate around the character freely
		else {
			transform.rotation = Quaternion.LookRotation(transform.forward);
		}
	}

	void OrientPlayer() {
		Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
		player.transform.forward = Vector3.RotateTowards(player.transform.forward, horizontalVelocity, Time.deltaTime * horizontalSpeed/5, 0.0f);
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

	void Actions() {
		UseSkill();
		if (Input.GetButtonDown ("Select Target")) {
			CheckTarget();
		}
		if (target) {
			if (Vector3.Distance (controller.transform.position, target.transform.position) > maxTargetDistance) {
				ClearTarget();
			}
		}
		if(Input.GetButtonDown("Attack")) {
			Attack();
		}
		if(Input.GetButtonDown("Dash")) {
			Dash();
		}
		if (dashTimer > 0) {
			dashTimer -= Time.deltaTime;
		}
		else {
			dashTimer = dashDelay;
		}
	}

	void UseSkill() {
		for (int i=0; i<10; i++) {
			if (Input.GetKeyDown(""+i)) {
				Debug.Log (i);
			}
		}
	}

	void Attack() {
		//Debug.Log ("pew");
	}
	
	void Dash() {
		//Debug.Log ("dashing goes here");
	}

	void OnControllerColliderHit (ControllerColliderHit hit)
	{
		Rigidbody body = hit.collider.attachedRigidbody;
		Vector3 force;

		if (body == null || body.isKinematic) {
			return;
		}

		if (hit.moveDirection.y < -0.3) {
			force = new Vector3 (0.0f,-0.5f,0.0f) * gravity * mass;
		}
		else {
			force = hit.controller.velocity * pushForce;
		}
		body.AddForceAtPosition(force, hit.point);
	}

	void SyncedMovement() { 
		syncTime += Time.deltaTime;
		transform.position = Vector3.Lerp(syncStartPos, syncEndPos, syncTime / syncDelay);
	}
	/*
	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
		Vector3 sync = Vector3.zero;
		if(stream.isWriting) {
			sync = transform.position;
			stream.Serialize(ref sync);
		}
		else {
			stream.Serialize(ref sync);
			
			syncTime = 0.0f;
			syncDelay = Time.time - lastSyncTime;
			lastSyncTime = Time.time;
			
			syncStartPos = transform.position;
			syncEndPos = sync;
		}
	}*/

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