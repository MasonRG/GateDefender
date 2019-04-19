using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserProjectile : MonoBehaviour {

	private Rigidbody rb;
	private LaserTower caller;
	private float lifetime;
	private float spawnTime;

	private int enemiesHit;

	public GameObject hitFX;
	public SphereCollider hitCollider;

	public void Initialize(Vector3 initialVelocity, float lifetime, LaserTower caller) {
		rb = GetComponent<Rigidbody>();
		rb.velocity = initialVelocity;
		hitCollider.radius = caller.areaOfEffect;
		this.lifetime = lifetime;
		this.spawnTime = Time.time;
		this.caller = caller;
	}

	void Update() {
		if (Time.time > spawnTime + lifetime)
			SelfDestruct();
	}

	void SelfDestruct() {
		//Destroy this projectile
		Destroy(gameObject);
	}

	void OnTriggerEnter(Collider other) {

		//If we hit an enemy, tell our laser tower so it can apply damage
		GameObject rootObj = Common.GetRootObject(other.transform).gameObject;
		if (rootObj.tag.Equals("Enemy")) {
			caller.ApplyDamage(rootObj, ++enemiesHit);

			//instantiate fx
			Vector3 targetPos = other.transform.position;
			Vector3 callerPos = caller.transform.position;
			callerPos.y = targetPos.y = transform.position.y;

			Vector3 direction = callerPos - targetPos;
			Quaternion spawnRotation = Quaternion.LookRotation(direction, Vector3.up);
			Vector3 spawnPosition = Vector3.Lerp(transform.position, targetPos, 0.5f);

			Instantiate(hitFX, spawnPosition, spawnRotation);
		}
	}
}
