using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour {

	public float cellSize = 1.5f;
	public float cellSpacing = 0.1f;
	public int size = 16;

	public Transform container;

	[ContextMenu("GenerateGrid")]
	public void GenerateGrid() {

		if (container != null)
			DestroyImmediate(container.gameObject);
		container = new GameObject().transform;
		container.name = "Grid";

		float offset = cellSize+cellSpacing;
		float x_org = -(offset * (size/2));
		float x = x_org;
		float y = 0;
		float z = x_org;

		int i = 0;
		for(int w=0; w<size; w++) {
			for(int h=0; h<size; h++) {
				GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
				DestroyImmediate(cell.GetComponent<MeshCollider>());
				cell.name = "Cell "+(i++).ToString();
				cell.transform.eulerAngles = new Vector3(90f,0f,0f);
				cell.transform.localScale = Vector3.one * cellSize;
				cell.transform.position = new Vector3(x,y,z);
				cell.transform.parent = container;
				x += offset;
			}
			z += offset;
			x = x_org;
		}
	}
}
