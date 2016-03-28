using UnityEngine;
using System.Collections;

public class PlayerOnCircle : MonoBehaviour {

	public Transform cam;

	public float speed = 5.0f;
	public float jumpSpeed = 10.0f;
	public float cameraSmoothing = 5.0f;
	public float cameraHeight = 3.0f;

	float angle = 0.0f;
	float radius = 0.0f;
	float cameraAngle = 0.0f;
	float cameraRadius = 0.0f;
	float cameraPosY = 0.0f;
	Vector3 position;
	Vector3 cameraPosition;

	float currentPlatformPosY = 0.0f;

	Rigidbody rigidBody;

	void Awake() {
		rigidBody = GetComponent<Rigidbody> ();
	}

	void Start () {
		radius = Mathf.Sqrt (  transform.position.x*transform.position.x + transform.position.z*transform.position.z  );
		cameraRadius = Mathf.Sqrt (  cam.transform.position.x*cam.transform.position.x + cam.transform.position.z*cam.transform.position.z  );
	}

	void Update() {
		//if (Input.GetKeyDown (KeyCode.W) && onGround) {			
		//	onGround = false;
		//	rigidBody.AddForce (Vector3.up * 400);
		//}
	}

	void FixedUpdate () {
		
		float angleChange = Input.GetAxis ("Horizontal") * speed * Time.deltaTime;
		angle += angleChange;

		position.Set ( Mathf.Sin(angle)*radius , transform.position.y, -Mathf.Cos(angle)*radius);
		//transform.position = position;
		rigidBody.MovePosition( position );

		cameraAngle = Mathf.Lerp ( cameraAngle, angle, cameraSmoothing * Time.deltaTime );
		cameraPosY = Mathf.Lerp (cameraPosY, currentPlatformPosY + cameraHeight, cameraSmoothing * Time.deltaTime);
			
		cameraPosition.Set ( Mathf.Sin(cameraAngle)*cameraRadius , cameraPosY, -Mathf.Cos(cameraAngle)*cameraRadius);
		cam.transform.position = cameraPosition;
		cam.transform.localEulerAngles = new Vector3( 0.0f, -cameraAngle*Mathf.Rad2Deg, 0.0f);

		transform.localEulerAngles = new Vector3( 0.0f, -cameraAngle*Mathf.Rad2Deg, 0.0f);
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "Ground") {
			if (transform.position.y > collision.collider.transform.position.y) {
				currentPlatformPosY = collision.collider.transform.position.y;
				rigidBody.AddForce (Vector3.up * 400);
			}
		}
	}

	void OnCollisionExit(Collision collision) {
	}
}
