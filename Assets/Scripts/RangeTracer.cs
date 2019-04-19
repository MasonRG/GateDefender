using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeTracer : MonoBehaviour {

	private GameController gc;
	private LineRenderer lineRenderer;
	private Vector3[] localVertexPositions;
	public int numVertices = 12;
	public float circleHeight = 0.5f;
	public float turnDegPerSecond = 180f;


	//Called by the game controller upon instantiation
	public void Initialize(GameController gc) {
		this.gc = gc;
		localVertexPositions = new Vector3[numVertices+1];
		lineRenderer = GetComponent<LineRenderer>();
		lineRenderer.numPositions = numVertices+1;
		SetVertices(0f);
	}
		
	public void SetVertices(float range) {

		float arc = 360f / numVertices;
		Vector3 pointer = new Vector3(0f,0f,range);
		Vector3 toDir = Vector3.left;

		//Get each vertex by rotating the pointer
		for(int i = 0; i < numVertices; i++) {
			localVertexPositions[i] = pointer+Vector3.up*circleHeight;
			pointer = Vector3.RotateTowards(pointer, toDir, arc*Mathf.Deg2Rad, 0.0f);

			if (i == numVertices/4 - 1)			toDir = Vector3.back;
			else if(i == numVertices/2 - 1)		toDir = Vector3.right;
			else if (i == 3*numVertices/4 - 1)	toDir = Vector3.forward;
		}

		//Add the final (duplicate of the first vertex to enclose the circle)
		localVertexPositions[numVertices] = localVertexPositions[0];
	
		lineRenderer.SetPositions(localVertexPositions);
	}

	void Update() {
		//Rotation
		if (lineRenderer.enabled)
			transform.Rotate(Vector3.up, turnDegPerSecond*Time.deltaTime);
	}

	void LateUpdate() {
		//Check if the rangetracer should be active or not
		lineRenderer.enabled = gc.DrawRangeActive;
	}

}
