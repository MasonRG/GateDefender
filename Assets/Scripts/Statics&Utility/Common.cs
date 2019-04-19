using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Common {

	public static class Layers {
		public static readonly int ground = (1<<8);
		public static readonly int tower = (1<<9);
		public static readonly int path = (1<<10);
		public static readonly int enemy = (1<<11);
		public static readonly int all = ground | tower | path | enemy;
	}



	public static int LEFT_MOUSE = 0;
	public static int RIGHT_MOUSE = 1;

	#region Utility Functions
	public static Vector3 RemoveYComp(Vector3 v) {
		v.y = 0f;
		return v;
	}

	public static float GetRatio(float value, float lower, float upper) {
		value = Mathf.Clamp(value, lower, upper);
		value = value - lower;
		upper = upper - lower;
		return Mathf.Clamp01(value/upper);
	}

	public static Transform GetRootObject(Transform current) {
		if (current == null)
			return null;
		while (current.parent != null) {
			current = current.parent;
		}
		return current;
	}
	#endregion

	#region Debug
	public static void DrawCube(Vector3 center, float length, Color color) {

		float halfL = length/2f;
		Vector3 upLeftBack = center + Vector3.left*halfL + Vector3.up*halfL - Vector3.forward*halfL;
		Vector3 upRightBack = center + Vector3.right*halfL + Vector3.up*halfL - Vector3.forward*halfL;
		Vector3 downLeftBack = center + Vector3.left*halfL + Vector3.down*halfL - Vector3.forward*halfL;
		Vector3 downRightBack = center + Vector3.right*halfL + Vector3.down*halfL - Vector3.forward*halfL;

		Vector3 upLeftFore = upLeftBack+Vector3.forward*length;
		Vector3 upRightFore = upRightBack+Vector3.forward*length;
		Vector3 downLeftFore = downLeftBack+Vector3.forward*length;
		Vector3 downRightFore = downRightBack+Vector3.forward*length;


		Debug.DrawLine(upLeftBack, upLeftFore, color);
		Debug.DrawLine(upRightBack, upRightFore, color);
		Debug.DrawLine(downLeftBack, downLeftFore, color);
		Debug.DrawLine(downRightBack, downRightFore, color);

		Debug.DrawLine(upLeftBack, upRightBack, color);
		Debug.DrawLine(upLeftFore, upRightFore, color);
		Debug.DrawLine(downLeftBack, downRightBack, color);
		Debug.DrawLine(downLeftFore, downRightFore, color);

		Debug.DrawLine(upLeftBack, downLeftBack, color);
		Debug.DrawLine(upLeftFore, downLeftFore, color);
		Debug.DrawLine(upRightBack, downRightBack, color);
		Debug.DrawLine(upRightFore, downRightFore, color);
	}
	#endregion


	public static void SetCursor(bool confine, bool visible) {
		Cursor.lockState = (confine)? CursorLockMode.Confined : CursorLockMode.None;
		Cursor.visible = visible;
	}
}
	



