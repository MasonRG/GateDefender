using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketTower : Tower {

	private GameObject targetLastFrame;
	private float fireTime = 0;


	void Update() {

		if (!towerIsActive || nearbyEnemies.Count == 0) {
			return;
		}

		//1. Acquire - Handled by collider entry and exit

		//2. Select - Pick the nearest enemy by default (for now)
		GameObject target=null;
		{
			//Search for nearest target or keep the same target if we already found one
			float minDist = Mathf.Infinity;
			bool nullFoundFlag = false;
			foreach(GameObject obj in nearbyEnemies) {

				if (obj == null) {
					nullFoundFlag = true;
					continue;
				}

				if (targetLastFrame != null) {
					if (targetLastFrame == obj) { //old target is still in range so maintain it
						target = targetLastFrame;
						break;
					}
				}

				float currdist = Vector3.SqrMagnitude(obj.transform.position - transform.position);
				if (currdist < minDist) {
					target = obj;
					minDist = currdist;
				}
			}
			if (nullFoundFlag)
				nearbyEnemies.Remove(null);
		}

		targetLastFrame = target;

		//3. Engage
		if (target != null) {
			if (TurnTowardsTarget(target.transform.position)) //returns true when facing enemy
				EngageTarget(target);
		}
	}

	void LateUpdate() {
		PointBarrel();
	}

	BallisticData ballistics = new BallisticData();

	//Sets ballistics info
	void AcquireBallisticData(Vector3 position) {
		float displacement;
		float flightTime;
		float shotAngle;
		float initialSpeed;

		displacement = Vector3.Magnitude(Common.RemoveYComp(position - transform.position));
		float distRatio = Common.GetRatio(displacement, 0.5f, range);

		//angle determined by the distance to target
		shotAngle = Mathf.Lerp(Kinematics.min_angle, Kinematics.max_angle, distRatio);
		flightTime = Kinematics.GetFlightTimeFromDist(Kinematics.gravity, displacement, shotAngle, activeMuzzle.position.y-position.y);
		initialSpeed = Kinematics.GetInitialSpeed(displacement, shotAngle, flightTime);


		ballistics.angle = shotAngle;
		ballistics.distance = displacement;
		ballistics.initialSpeed = initialSpeed;
		ballistics.time = flightTime;
	}


	void PointBarrel() {
		Vector3 euler = turretBody.localEulerAngles;
		euler.y = 240f+ballistics.angle;
		turretBody.localEulerAngles = euler;
	}



	public void Detonate(Vector3 position) {

		//Set up explosion fx
		fireFXScript.MoveDamageFX(position);
		fireFXScript.OrientDamageFX(Quaternion.identity);
		fireFXScript.StartDamageFX(towerType);
		AudioManager.RetireFireSound(towerType);
		Invoke("RetireExplosionSFX", 1f);

		//Deal damage
		float radius_AoE_sqr = areaOfEffect*areaOfEffect;

		Collider[] cols = Physics.OverlapSphere(position, areaOfEffect, Common.Layers.enemy);
		foreach(Collider c in cols) {
			
			//Determine damage to deal based on distance from explosion
			float currDistSqr = Vector3.SqrMagnitude(c.transform.position - position);
			float dmgMult = 1f - Common.GetRatio(currDistSqr, 1f, radius_AoE_sqr); //returns 0-1, 1 when closest, 0 when furthest
			dmgMult = Mathf.Clamp(dmgMult, 0.25f, 1f); //minimum of 25% damage

			//Deal damage
			Enemy E = c.gameObject.GetComponentInParent<Enemy>();
			int killValue = E.TakeDamage(Mathf.RoundToInt(damage*dmgMult), towerType);
			if (killValue > 0) {//Enemy was killed
				PlayerStats.money += killValue;
				gc.SpawnKillFX(E.ID, c.transform.position);
			}
		}
	}

	void RetireExplosionSFX() {
		AudioManager.RetireDamageSound(towerType);
	}

	void Fire(Transform target) {
		//Spawn firing fx
		fireFXScript.StartFiring(towerType);

		//Spawn the rocket and set its speed
		GameObject projectile = (GameObject)Instantiate(gc.projectilePrefabs.rocket, activeMuzzle.position, activeMuzzle.rotation);
		projectile.GetComponent<RocketProjectile>().Initialize(projectile.transform.forward*ballistics.initialSpeed, target, this);

		//Animate
		StartCoroutine(FireAnimation());
	}

	IEnumerator FireAnimation() {
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_FIRE);
		yield return null;
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_IDLE);
	}

	void EngageTarget(GameObject target) {

		AcquireBallisticData(target.transform.position);

		if (Time.time > fireTime + fireRate) {
			fireTime = Time.time;
			Fire(target.transform);
		}
	}
}
