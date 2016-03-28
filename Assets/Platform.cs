using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {

	Vector3 radialPos = new Vector3(0.0f, 0.0f, 0.0f);
	Collider coll;
	Vector3 start;
	Vector3 end;
	Vector3 startRad;
	Vector3 endRad;
	void Awake () {
		coll = GetComponent<Collider> ();
	}

	void Start() {
		radialPos = RadialMath.euqlidToRadial (transform.position);

		BoxCollider box = (BoxCollider)coll;
		Vector3 size = box.size;
		Vector3 startX = new Vector3 (-size.x / 2.0f, 0.0f, 0.0f);
		startX = transform.TransformPoint (startX);
		startRad = RadialMath.euqlidToRadial (startX);

		Vector3 endX = new Vector3 (size.x / 2.0f, 0.0f, 0.0f);
		endX = transform.TransformPoint (endX);
		endRad = RadialMath.euqlidToRadial (endX);

		start = getPosOnPlatform (startRad.y);
		end = getPosOnPlatform (endRad.y);
	}

	public Transform getTransform() {
		return gameObject.transform;
	}

	public Vector3 getRadialPosition() {
		return radialPos;
	}

	public Vector3 getStartPoint() {
		return start;
	}

	public Vector3 getEndPoint() {
		return end;
	}

	public Vector3 getStartRadial() {
		return startRad;
	}

	public Vector3 getEndRadial() {
		return endRad;
	}

	public Vector3 getPosOnPlatform(float angle) {

		angle = RadialMath.normalize (angle);

		float platformRot = - transform.localEulerAngles.y * Mathf.Deg2Rad;
		float a1 = Mathf.Tan (platformRot);
		float b1 = transform.position.z - a1 * transform.position.x;
		float a2 = Mathf.Tan (angle - Mathf.PI/2.0f);

		float x =  b1 / (a2 - a1);
		float y = a2 * x;

		return new Vector3 (x, transform.position.y, y);
	}

	public Vector3 getNearestEndRadial(Vector3 radialPos) {
		return new Vector3 (0.0f, 0.0f, 0.0f);
	}

	public float getStartAngle() {
		return startRad.y;
	}

	public float getEndAngle() {
		return endRad.y;
	}

	public Vector3 getNearestEndRadial(float angle) {
		if (RadialMath.dist (angle, startRad.y) <= RadialMath.dist (angle, endRad.y))
			return startRad;
		else
			return endRad;
	}

	public Vector3 getNearestEnd(float angle) {
		if (RadialMath.dist (angle, startRad.y) <= RadialMath.dist (angle, endRad.y))
			return start;
		else
			return end;
	}
}
