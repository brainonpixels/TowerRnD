using UnityEngine;
using System.Collections;

public class LevelGenerator : MonoBehaviour {

	public Transform player;
	public Transform platorm;
	public Transform platforms;

	float currentTop = 0.0f;
	float pieceHeight = 10.0f;
	float heightToGen = 5.0f;

	float angle = 0.0f;
	float y = 0.0f;

	void Start () {
	
	}

	void Update () {
	
		if (player.position.y > currentTop - heightToGen)
			generateNextPiece ();

	}

	void generateNextPiece() {

		float dy = 0.9f;
		float da = 0.7f;

		int n = (int)(pieceHeight / dy);

		float radius = 5.0f;

		for (int i = 0; i < n; i++) {
			angle += da;
			y += dy;
			Vector3 radPos = new Vector3 (radius, angle, y);
			addPlatform (radPos, angle);
		}

		currentTop += pieceHeight;
	}

	void addPlatform(Vector3 radialPos, float angle) {

		Transform p = Instantiate (platorm);
		p.position = RadialMath.radialToEuqlid (radialPos);
		p.localEulerAngles = new Vector3 (0.0f, - angle * Mathf.Rad2Deg, 0.0f);

		p.parent = platforms;

	}
}
