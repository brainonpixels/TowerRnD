using UnityEngine;
using System.Collections;

public class PlatformTester : MonoBehaviour {

	public Transform debugMarker;
	public Transform platforms;

	public Material material1;
	public Material material2;

	float angle = 0.0f;
	float speed = 1.0f;

	Transform marker = null;
	Transform[] lineMarkers;

	void Start () {
		marker = Instantiate (debugMarker);
		marker.transform.position = new Vector3 (0.0f, 0.0f, 0.0f);

		lineMarkers = new Transform[100];
		for (int i = 0; i < 100; i++) {
			lineMarkers [i] = Instantiate (debugMarker);
			lineMarkers [i].transform.position = new Vector3 (0.0f, 0.0f, 0.0f);
			lineMarkers [i].transform.localScale = new Vector3 (0.2f, 0.2f, 0.2f);
		}
	}

	void Update () {
	
		if (Input.GetKey (KeyCode.Z))
			angle -= speed * Time.deltaTime;
		if (Input.GetKey (KeyCode.X))
			angle += speed * Time.deltaTime;

		angle = RadialMath.normalize (angle);

		transform.localEulerAngles = new Vector3 (0.0f, - angle * Mathf.Rad2Deg, 0.0f);

		Platform platform = findNearestPlatform ();
		Vector3 pos = platform.getPosOnPlatform (angle);

		marker.transform.position = pos;

		bool onPlatform = RadialMath.angleInRange (angle, platform.getStartAngle (), platform.getEndAngle ());
		if (onPlatform)
			marker.gameObject.GetComponent<Renderer> ().material = material1;
		else
			marker.gameObject.GetComponent<Renderer> ().material = material2;
	}

	private Platform findNearestPlatform() {

		float distMin = 10000.0f;
		Platform minP = null;
		foreach (Transform t in platforms) {

			Platform p = t.GetComponent<Platform> ();
			Vector3 radial = p.getRadialPosition ();

			float dist = RadialMath.dist (angle, radial.y);
			if (dist < distMin) {
				distMin = dist;
				minP = p;
			}


		}

		return minP;
	}
}
