using System.Collections;
using UnityEngine;

public class DestroyByTime : MonoBehaviour {

	public float lifetime = 4f;

	void Start () {
		Destroy(gameObject, lifetime);
	}

}
