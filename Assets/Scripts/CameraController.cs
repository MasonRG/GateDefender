using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

	public Vector3 bottomLeftCorner, topRightCorner;
	public Vector2 zoomLimits;
	public Vector2 fovLimits;
	public float moveSpeed;
	public float zoomSpeed;

	private Camera cam;

	void Start() {
		cam = GetComponent<Camera>();
	}

	void Update () {

		float moveAxisX = Input.GetAxis("Horizontal");
		float moveAxisY = Input.GetAxis("Vertical");
		float zoomAxis = Input.GetAxisRaw("Mouse ScrollWheel");

		Vector3 newPosition = transform.position;

		float timeFactor = (Mathf.Approximately(Time.timeScale, 0f))? 0f : 1f/Time.timeScale;
		float _moveSpeed = moveSpeed * 10f * Time.deltaTime * timeFactor;
		float _zoomSpeed = zoomSpeed * 10f * Time.deltaTime * timeFactor;

		//Panning
		newPosition += Vector3.right * _moveSpeed * moveAxisX;
		newPosition += Vector3.forward * _moveSpeed * moveAxisY;
		newPosition = ConstrainCameraPosition(newPosition);


		//Zooming
		Vector3 preZoomPosition = newPosition;
		if (zoomAxis > 0)		newPosition += transform.forward * _zoomSpeed;
		else if (zoomAxis < 0)	newPosition += -transform.forward * _zoomSpeed;

		if (ZoomOutOfBounds(newPosition.y))
			newPosition = preZoomPosition;


		//Adjust FOV based on height
		float heightRatio = 1f - Common.GetRatio(newPosition.y, zoomLimits.x, zoomLimits.y);
		float additiveFOV = (fovLimits.y - fovLimits.x)*heightRatio;
		cam.fieldOfView = Mathf.Clamp( fovLimits.x + additiveFOV, fovLimits.x, fovLimits.y );


		//Assign new position
		transform.position = newPosition;
	}

	Vector3 ConstrainCameraPosition(Vector3 p) {
		p.x = Mathf.Clamp(p.x, bottomLeftCorner.x, topRightCorner.x);
		p.z = Mathf.Clamp(p.z, bottomLeftCorner.z, topRightCorner.z);
		return p;
	}

	bool ZoomOutOfBounds(float height) {
		return (height < zoomLimits.x || height > zoomLimits.y);
	}
}
