using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	public Transform target;
	public float smoothing = 5f;
	Vector3 offset;

	void Start () {
		offset = transform.position - target.position;
	}

	void FixedUpdate () {
		transform.position = Vector3.Lerp (transform.position, target.position + offset, smoothing * Time.deltaTime);
	}
}
