using UnityEngine;
using System.Collections;

public class LevelFactory : MonoBehaviour {

	public Transform platformPrefab;
	public float radius = 5.0f;

	void Start () {



		for (int i = 0; i < 100; i++) {
			Transform t = Instantiate (platformPrefab);

			float randomAngle = Random.value * Mathf.PI * 2;
			t.position = new Vector3 ( Mathf.Sin(randomAngle) * radius, 1.5f + i*0.5f, Mathf.Cos(randomAngle) * radius );
			t.localEulerAngles = new Vector3( 0.0f, randomAngle * Mathf.Rad2Deg, 0.0f  );
		}

	}

	void Update () {
	
	}
}
