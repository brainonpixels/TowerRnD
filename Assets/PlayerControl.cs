using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {

	Rigidbody rigidBody;
	Animator animator;
	bool onGround = true;

	void Awake() {
		rigidBody = GetComponent<Rigidbody> ();
		animator = GetComponent<Animator> ();
	}

	void Update () {
		if (Input.GetKeyDown (KeyCode.W) && onGround) {
			rigidBody.AddForce (Vector3.up * 60);
			animator.SetBool ("isJumping", true);
			onGround = false;
		}

		float inputx = Input.GetAxis ("Horizontal");
		float move = inputx * 8f;
		rigidBody.MovePosition (transform.position + Vector3.right * move * Time.deltaTime);

		if (inputx > 0.1f) {
			transform.localEulerAngles = new Vector3(0.0f, 270.0f , 0.0f);
		}
		if (inputx < -0.1f) {
			transform.localEulerAngles = new Vector3(0.0f, 90.0f, 0.0f);
		}

		bool isMoving = onGround && (inputx > 0.1f || inputx < -0.1f);
		animator.SetBool ("isMoving", isMoving);
	}

	void OnCollisionEnter(Collision collision) {
		if (collision.collider.tag == "Ground") {
			animator.SetBool ("isJumping", false);
			onGround = true;
		}
	}
}
