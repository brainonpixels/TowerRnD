using UnityEngine;
using System.Collections;

public class RadialPlayerControl : MonoBehaviour {

	public Transform cam;
	public Transform platforms;
	public Transform debugMarker;
	public Material debugMat1;
	public Material debugMat2;
	public Material debugMat3;

	public float dt = 1.0f;
	public float acceleration = 10.0f;
	public float maxSpeed = 1.0f;
	public float jumpSpeed = 10.0f;
	public float cameraSmoothing = 5.0f;
	public float cameraHeight = 3.0f;
	public float gravity = -9.89f;



	float maxJumpHeight = 0.0f;
	float vertSpeed = 0.0f;

	Vector3 radialPos;
	float direction;
	Vector3 radialCameraPos;

	Vector3 position;
	Vector3 cameraPosition;
	Vector3 cameraOffset;
	float cameraTilt;

	float currentPlatformPosY = 0.0f;
	float speedY = 0.0f;

	bool onGround = false;
	bool testJump = false;

	Collider playerCollider;
	Animator animator;
	public Transform playerMesh;

	Transform debugMarker1 = null;
	Transform debugMarker2 = null;
	Transform debugMarker3 = null;
	Transform debugMarker4 = null;

	Platform lastPlatform = null;
	Platform nextPlatform = null;
	Platform overPlatform = null;

	float logTime;
	Transform[] lineMarkers;

	void Awake() {
		playerCollider = GetComponent<Collider> ();
		animator = playerMesh.gameObject.GetComponent<Animator> ();

		float vs = jumpSpeed;
		maxJumpHeight = 0.0f;
		while (vs > 0 && gravity != 0.0f) {
			float dt = 0.01666f;
			maxJumpHeight += vs * dt;
			vs = vs + gravity * dt;
		}
	}

	void Start () {
		radialPos = RadialMath.euqlidToRadial (transform.position);
		radialCameraPos = RadialMath.euqlidToRadial (cam.transform.position);
		radialCameraPos.y = radialPos.y;

		cameraOffset = cam.transform.position - transform.position;
		cameraTilt = cam.transform.localEulerAngles.x;

		debugMarker1 = Instantiate (debugMarker);
		debugMarker1.GetComponent<Renderer> ().material = debugMat1;

		debugMarker2 = Instantiate (debugMarker);
		debugMarker2.GetComponent<Renderer> ().material = debugMat2;

		debugMarker3 = Instantiate (debugMarker);
		debugMarker3.GetComponent<Renderer> ().material = debugMat3;

		debugMarker4 = Instantiate (debugMarker);
		debugMarker4.GetComponent<Renderer> ().material = debugMat3;
		debugMarker4.localScale = new Vector3 (0.3f, 0.3f, 0.3f);
	}

	void Update() {
		if ( (Input.GetKeyDown (KeyCode.W) || Input.touchCount>0) && onGround) {
			speedY = jumpSpeed;
			onGround = false;
			animator.SetBool ("onGround", false);
			animator.SetTrigger ("jump");
		}

		bool isMoving = onGround && (vertSpeed > 0.1f || vertSpeed < -0.1f);
		animator.SetBool ("isMoving", isMoving);
	}

	void FixedUpdate () {

		float inputX = Input.GetAxisRaw ("Horizontal");
		if (Input.acceleration.x != 0.0f)
			inputX = Mathf.Sign (Input.acceleration.x);

		float targetSpeed = inputX * maxSpeed;
		vertSpeed = Mathf.Lerp( vertSpeed,  targetSpeed, acceleration * Time.deltaTime * dt);

		if (targetSpeed > 0.0f) {
			playerMesh.localEulerAngles = new Vector3 (0.0f, 90.0f, 0.0f);
		}

		if (targetSpeed < 0.0f) {
			playerMesh.localEulerAngles = new Vector3 (0.0f, -90.0f, 0.0f);
		}

		animator.SetBool ("isFalling", vertSpeed < 0);

		float angleChange = vertSpeed * Time.deltaTime * dt;
		radialPos.y = radialPos.y + angleChange;
		radialPos.y = RadialMath.normalize (radialPos.y);

		transform.position = RadialMath.radialToEuqlid (radialPos);
		transform.localEulerAngles = new Vector3 (0.0f, direction * Mathf.Rad2Deg, 0.0f);

		radialCameraPos.z = Mathf.Lerp (radialCameraPos.z, currentPlatformPosY + cameraOffset.y, cameraSmoothing * Time.deltaTime * dt);		
		radialCameraPos.y = RadialMath.Lerp (radialCameraPos.y, radialPos.y, cameraSmoothing * Time.deltaTime * dt);

		cam.transform.position = RadialMath.radialToEuqlid (radialCameraPos);
		cam.transform.localEulerAngles = new Vector3 (cameraTilt, -radialCameraPos.y * Mathf.Rad2Deg, 0.0f);

		if (!onGround && !testJump) {
			speedY = speedY + gravity * Time.deltaTime * dt;
			radialPos.z = radialPos.z + speedY * Time.deltaTime * dt;
		}


		Platform newNextPlatform = nextPlatform;
		if (lastPlatform != null) {
			if (angleChange > 0.0f && radialPos.y > lastPlatform.getEndAngle ())
				newNextPlatform = findNextPlatform (angleChange);
			else if (angleChange < 0.0f && radialPos.y < lastPlatform.getStartAngle ())
				newNextPlatform = findNextPlatform (angleChange);
		}

		if (nextPlatform != null && RadialMath.angleInRange (radialPos.y, nextPlatform.getStartAngle (), nextPlatform.getEndAngle ()) && radialPos.z > nextPlatform.transform.position.y)
			overPlatform = nextPlatform;

		if (newNextPlatform != nextPlatform) {
			nextPlatform = newNextPlatform;
		}

		debugMarker4.position = new Vector3 (0.0f, 0.0f, 0.0f);

		if (lastPlatform != null) {

			if (onGround) {
				Vector3 pos = lastPlatform.getPosOnPlatform (radialPos.y);
				radialPos.x = RadialMath.radius (pos);
				direction = lastPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad;
			} else if (nextPlatform != null) {
				
				if (RadialMath.angleInRange (radialPos.y, lastPlatform.getStartAngle (), lastPlatform.getEndAngle ())) {
					Vector3 pos = lastPlatform.getPosOnPlatform (radialPos.y);
					radialPos.x = RadialMath.radius (pos);
					direction = lastPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad;
				} else {
					
					Vector3 lastNearest = lastPlatform.getNearestEndRadial (radialPos.y);
					Vector3 nextNearest = nextPlatform.getNearestEndRadial (radialPos.y);
					Vector3 overNearest = Vector3.zero;
					if (overPlatform != null)
						overNearest = overPlatform.getNearestEndRadial (radialPos.y);

					if (overPlatform != null && overPlatform != nextPlatform && RadialMath.angleInRange( radialPos.y, overPlatform.getStartAngle(), overPlatform.getEndAngle() )) {
						Vector3 pos = overPlatform.getPosOnPlatform (radialPos.y);
						radialPos.x = RadialMath.radius (pos);
						direction = overPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad;
					} else if (overPlatform != null && overPlatform != nextPlatform && RadialMath.angleInRange( radialPos.y, overNearest.y, nextNearest.y )) {
						float t = (radialPos.y - overNearest.y) / (nextNearest.y - overNearest.y);
						radialPos.x = Mathf.Lerp (overNearest.x, nextNearest.x, t);
						direction = RadialMath.Lerp (overPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad, nextPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad, t);
					} else if (RadialMath.angleInRange (radialPos.y, lastNearest.y, nextNearest.y)) {
						float t = (radialPos.y - lastNearest.y) / (nextNearest.y - lastNearest.y);
						radialPos.x = Mathf.Lerp (lastNearest.x, nextNearest.x, t);
						direction = RadialMath.Lerp (lastPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad, nextPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad, t);

					} else if (RadialMath.angleInRange (radialPos.y, nextPlatform.getStartAngle (), nextPlatform.getEndAngle ())) {
						Vector3 pos = nextPlatform.getPosOnPlatform (radialPos.y);
						radialPos.x = RadialMath.radius (pos);
						direction = nextPlatform.transform.localEulerAngles.y * Mathf.Deg2Rad;
					}

				}
			}

		}

//		if (nextPlatform != null)
//			debugMarker1.transform.position = nextPlatform.getTransform ().position;
//		else 
//			debugMarker1.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
//
//		if (lastPlatform != null)
//			debugMarker2.transform.position = lastPlatform.getTransform ().position;
//		else 
//			debugMarker2.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
//
//		if (overPlatform != null)
//			debugMarker3.transform.position = overPlatform.getTransform ().position;
//		else 
//			debugMarker3.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);

	}

	void OnTriggerEnter(Collider other) {
		
		if (other.tag=="platform" && other.bounds.max.y < playerCollider.bounds.min.y + 0.4f) {
			Debug.Log ("HIT PLATFORM "+transform.position.y);
			onGround = true;
			radialPos.z = transform.position.y + (other.bounds.max.y - playerCollider.bounds.min.y - 0.1f);
			transform.position = RadialMath.radialToEuqlid (radialPos);
			currentPlatformPosY = other.transform.position.y;
			lastPlatform = other.GetComponent<Platform> ();
			overPlatform = null;
			animator.SetBool ("onGround", true);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.tag == "platform") {
			Debug.Log ("EXIT PLATFORM "+transform.position.y);
			onGround = false;
		}
	}

	private Platform findNextPlatform(float angleChange) {

		RadialMath.normalize (radialPos);

		if (angleChange == 0.0f)
			return null;

		float angleMin = 0.0f;
		float angleMax = 0.0f;

		if (angleChange > 0.0f) {
			angleMin = radialPos.y;
			angleMax = radialPos.y + Mathf.PI / 1.5f;
		} else {
			angleMin = radialPos.y - Mathf.PI / 1.5f;
			angleMax = radialPos.y;
		}

		Platform minT = null;
		float minDist = 10000000.0f;

		foreach (Transform t in platforms) {

				Platform p = t.GetComponent<Platform>();

				if (p == null)
					continue;

				Vector3 pos = t.position;
				Vector3 radial = p.getRadialPosition ();

				if ( RadialMath.angleInRange(radial.y, angleMin, angleMax) ) {
				float dist = (pos - transform.position).magnitude;

					if (dist < minDist) {
						minDist = dist;
						minT = p;
					}
				}
			}

		return minT;
	}

	private void log(object message) {
		logTime += Time.deltaTime;
		if (logTime > 1.0f) {
			logTime = 0;
			Debug.Log (message);
		}
	}
}
