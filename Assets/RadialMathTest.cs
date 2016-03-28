using UnityEngine;
using System.Collections;

public class RadialMathTest : MonoBehaviour {


	void Start () {
	

		float a1 = 0.0f;
		float a2 = 0.0f;
		float a3 = 0.0f;

		a1 = 0.1f;
		a2 = 0.6f;
		Debug.Assert (RadialMath.onRight (a1, a2) == true);
		Debug.Assert (RadialMath.onLeft (a1, a2) == false);
		Debug.Assert (RadialMath.onRight (a2, a1) == false);
		Debug.Assert (RadialMath.onLeft (a2, a1) == true);

		a1 = 6.1f;
		a2 = 0.6f;
		Debug.Assert (RadialMath.onRight (a1, a2) == true);
		Debug.Assert (RadialMath.onLeft (a1, a2) == false);
		Debug.Assert (RadialMath.onRight (a2, a1) == false);
		Debug.Assert (RadialMath.onLeft (a2, a1) == true);


		a1 = 0.3f;
		a2 = 0.4f;
		a3 = 0.5f;
		Debug.Assert (RadialMath.angleInRange ( a2, a1, a3 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a2, a3, a1 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a3, a1, a2 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a3, a2, a1 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a2, a3 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a3, a2 ) == false);

		a1 = 6.1f;
		a2 = 6.2f;
		a3 = 0.3f;
		Debug.Assert (RadialMath.angleInRange ( a2, a1, a3 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a2, a3, a1 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a3, a1, a2 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a3, a2, a1 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a2, a3 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a3, a2 ) == false);

		a1 = 6.1f;
		a2 = 0.1f;
		a3 = 0.3f;
		Debug.Assert (RadialMath.angleInRange ( a2, a1, a3 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a2, a3, a1 ) == true);
		Debug.Assert (RadialMath.angleInRange ( a3, a1, a2 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a3, a2, a1 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a2, a3 ) == false);
		Debug.Assert (RadialMath.angleInRange ( a1, a3, a2 ) == false);

	}

}
