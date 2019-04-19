using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Waypoint : MonoBehaviour {

	[SerializeField]public Waypoint next;
	public Transform end;
	public static readonly float trim_percent = 0.1f;


	//just gives the location of the waypoint
	public Vector3 FindWay(float offset){

		if (offset > 1 || offset < 0)
			offset = 0.5f;
		
		return (offset * (end.position - transform.position) + transform.position);
	}

	//Tells the AI how to get to the waypoint from the AI's location
	public Vector3 FindWay(Vector3 location, float offset){
		
		if (offset > 1 || offset < 0)
			offset = 0.5f;

		Vector3 way = ((offset * (end.position-transform.position) + transform.position)-location);
		way.y = 0;
		return way;


	}

	//Returns the vector that represents the line between the waypoint and it's endpoint
	public Vector3 GetLineStartToEnd() {
		return end.position - transform.position;
	}
		

	[ContextMenu("GenerateSequence")]
	public void GenerateSequence() {
		GameObject[] wpObjs = GameObject.FindGameObjectsWithTag("Waypoint");
		Waypoint[] wps = new Waypoint[wpObjs.Length];

		foreach(GameObject obj in wpObjs) { //waypoint names are: Waypoint (x) ; x some number
			string number = obj.name.Split(new char[]{' '}, 2)[1]; //extract the (x)
			number = number.Trim(new char[]{'(', ')'}); //Trim the parentheses

			//Use the number to index into wps
			int index;
			if (int.TryParse(number, out index)) //convert string to int, if successful use as index
				wps[index] = obj.GetComponent<Waypoint>();
		}

		//Assign the next waypoint.
		for(int i = 0; i < wps.Length-1; i++) {
			wps[i].next = wps[i+1];
		}
	}
}