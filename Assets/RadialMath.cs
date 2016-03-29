using UnityEngine;
using System.Collections;

public class RadialMath : MonoBehaviour {

	// Radial pos is Vector3 where
	// x = radius
	// y = angle
	// z = height

	public static Vector3 radialToEuqlid(Vector3 radial) {
		return new Vector3 (
			Mathf.Sin(radial.y)*radial.x,
			radial.z,
			-Mathf.Cos(radial.y)*radial.x
		);
	}

	public static Vector3 euqlidToRadial(Vector3 euqlid) {

		float radius = Mathf.Sqrt ( euqlid.x*euqlid.x + euqlid.z*euqlid.z );
		float y = euqlid.y;
		float alpha = 0.0f;

		if (euqlid.x >= 0 && euqlid.z < 0)
			alpha = Mathf.Asin (euqlid.x / radius);
		else if (euqlid.x >= 0 && euqlid.z >= 0)
			alpha = Mathf.PI - Mathf.Asin (euqlid.x / radius);
		else if (euqlid.x < 0 && euqlid.z >= 0)
			alpha = Mathf.PI + Mathf.Asin (-euqlid.x / radius);
		else
			alpha = 2*Mathf.PI - Mathf.Asin (-euqlid.x / radius);
		

		return new Vector3 (radius, alpha, y);
	}

	public static Vector2 euqlidToRadial2(Vector2 euqlid) {
		float radius = Mathf.Sqrt ( euqlid.x*euqlid.x + euqlid.y*euqlid.y );
		float y = euqlid.y;
		float alpha = 0.0f;

		if (euqlid.x >= 0 && euqlid.y < 0)
			alpha = Mathf.Asin (euqlid.x / radius);
		else if (euqlid.x >= 0 && euqlid.y >= 0)
			alpha = Mathf.PI - Mathf.Asin (euqlid.x / radius);
		else if (euqlid.x < 0 && euqlid.y >= 0)
			alpha = Mathf.PI + Mathf.Asin (-euqlid.x / radius);
		else
			alpha = 2*Mathf.PI - Mathf.Asin (-euqlid.x / radius);
		
		
		return new Vector2 (radius, alpha);
	}

	public static float normalize(Vector3 radialPos) {
		float angle = radialPos.y;
		while (angle > Mathf.PI*2)
			angle = angle - Mathf.PI*2;
		while (angle < 0)
			angle = angle + Mathf.PI*2;
		radialPos.Set (radialPos.x, angle, radialPos.z);
		return angle;
	}

	public static float normalize(float angle) {
		while (angle > Mathf.PI*2)
			angle = angle - Mathf.PI*2;
		while (angle < 0)
			angle = angle + Mathf.PI*2;
		return angle;
	}

	public static bool onRight(float start, float angle) {
		while (angle < start)
			angle += 2*Mathf.PI;
		return angle < start + Mathf.PI;
	}

	public static bool onLeft(float start, float angle) {
		while (angle > start)
			angle -= 2*Mathf.PI;
		return angle > start - Mathf.PI;
	}

	public static bool angleInRange(float angle, float min, float max) {
		
		angle = normalize (angle);
		min = normalize (min);
		max = normalize (max);

		if (onLeft (min, max)) {
			float x = min;
			min = max;
			max = x;
		}

		if (angle >= min && angle <= max)
			return true;

		if (min > max) {
			if (angle < Mathf.PI)
				return angle < max;
			else
				return angle > min;
		}

		return false;
	}

	public static float Lerp(float start, float target, float t) {
		start = normalize (start);
		target = normalize (target);

		if (start > target + Mathf.PI)
			target += 2 * Mathf.PI;
		if (start < target - Mathf.PI)
			start += 2 * Mathf.PI;

		return normalize(Mathf.Lerp(start, target, t));
	}

	public static float Lerp(float start, float target, float t, float direction) {

		float dir = target - start;
		if (Mathf.Sign (dir) != Mathf.Sign (direction)) {
			float x = start;
			start = target;
			target = x;
		}

		return Lerp (start, target, t);

	}

	public static float dist(float angle1, float angle2) {

		if (angle1 > angle2 + Mathf.PI)
			angle2 += 2 * Mathf.PI;
		if (angle1 < angle2 - Mathf.PI)
			angle1 += 2 * Mathf.PI;

		return Mathf.Abs(angle2 - angle1);
	}

	public static float radius(Vector3 euqlidPos) {
		return Mathf.Sqrt ( euqlidPos.x*euqlidPos.x + euqlidPos.z*euqlidPos.z );
	}

	public static float getDirectionAngle(Vector3 p1, Vector3 p2) {
		float a = euqlidToRadial (p2 - p1).y;
		return 5.0f/2.0f*Mathf.PI - a;
	}

	public static float radialSize(float radius, float width) {
		return Mathf.Atan (width / (radius * 2.0f)) * 2.0f;
	}

	public static float radialSize(float radius, float width, float angle) {

		float A = Mathf.Sqrt ( width*width/4.0f + radius*radius - width*radius*Mathf.Cos(Mathf.PI / 2.0f - angle) );
		float B = Mathf.Sqrt ( width*width/4.0f + radius*radius - width*radius*Mathf.Cos(Mathf.PI / 2.0f + angle) );

		return
			Mathf.Asin (width / 2.0f * Mathf.Sin (Mathf.PI / 2.0f - angle) / A) +
			Mathf.Asin (width / 2.0f * Mathf.Sin (Mathf.PI / 2.0f + angle) / B);
	}
}
