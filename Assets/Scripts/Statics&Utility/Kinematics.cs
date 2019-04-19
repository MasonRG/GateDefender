using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Kinematics {

//	public static readonly float max_time = 2.0f;// s
//	public static readonly float min_time = 0.5f;// s
//	public static readonly float max_speed = 60f;// m/s
//	public static readonly float min_speed = 10f;// m/s
//	public static readonly float min_range = 20f;// m
//	public static readonly float max_range = 60f;// m
	public static readonly float max_angle = 40f;// deg
	public static readonly float min_angle = 20f;// deg
	public static readonly float gravity = -9.81f;// m/s*s

	public static float GetFlightTimeFromDist(float g, float x, float deg, float y0) {
		float tmp1 = -2f/g;
		float tan = Mathf.Tan(deg*Mathf.Deg2Rad);
		float tmp2 = (x*tan) + y0;
		float discrim = tmp1 * tmp2;
		return Mathf.Sqrt(discrim);
	}

	public static float GetInitialSpeed(float x, float deg, float time) {
		float cos = Mathf.Cos(deg*Mathf.Deg2Rad);
		float tmp1 = cos*time;
		return x/tmp1;
	}

}

public class BallisticData {
	public float time;
	public float distance;
	public float initialSpeed;
	public float angle;
	public BallisticData() {
		this.time = 1f;
		this.distance = 1f;
		this.initialSpeed = 1f;
		this.angle = 1f;
	}
	public BallisticData(float time, float distance, float initialSpeed, float angle) {
		this.time = time;
		this.distance = distance;
		this.initialSpeed = initialSpeed;
		this.angle = angle;
	}
}