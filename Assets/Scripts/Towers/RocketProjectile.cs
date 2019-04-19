using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketProjectile : MonoBehaviour {

	private Rigidbody rb;
	private Transform target;
	private RocketTower caller;
	private bool detonateFlag = false;

	public void Initialize(Vector3 initialVelocity, Transform target, RocketTower caller) {
		rb = GetComponent<Rigidbody>();
		rb.velocity = initialVelocity;
		this.target = target;
		this.caller = caller;
	}

	void FixedUpdate() {
		
			//If we are below the map, we can stop and delete
		if (transform.position.y < 0f) {
			if (!detonateFlag) {
				rb.velocity = Vector3.zero;
				rb.useGravity = false;
				caller.Detonate(Common.RemoveYComp(transform.position));
				Destroy(gameObject, 0.5f);
				detonateFlag = true;
			}
		}
		else {
			
			if (rb.velocity.y < 0f && target != null)
				rb.velocity = Vector3.RotateTowards(rb.velocity, target.position - transform.position, Mathf.Deg2Rad*4f, 0.0f);

			transform.LookAt(transform.position + rb.velocity);
		}
	}
}
