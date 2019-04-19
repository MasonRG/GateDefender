using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserTower : Tower {

	private GameObject targetLastFrame;
	private float fireTime = 0;

	void Update() {

		if (!towerIsActive || (nearbyEnemies.Count == 0 && targetLastFrame == null)) {
			fireFXScript.StopFiring(towerType);
			chargingBeam = false;
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

		//3. Engage

		if (target != null) {
			targetLastFrame = target;
			if (TurnTowardsTarget(target.transform.position)) //returns true when facing enemy
				EngageTarget(target);
		}
		else if (targetLastFrame != null && chargingBeam) {
			if (TurnTowardsTarget(targetLastFrame.transform.position))
				EngageTarget(targetLastFrame);
		}
		else
			targetLastFrame = null;
	}


	bool chargingBeam = false;
	float beamTime = 0;
	const float beamRate = 1.5f; //this needs to match up with the particle effect for laser firing

	void EngageTarget(GameObject target) {

		if (Time.time > fireTime + fireRate) {

			if (!chargingBeam) {
				beamTime = Time.time;
				fireFXScript.StopFiring(towerType);
				fireFXScript.StartFiring(towerType);//Spawn firing fx
				chargingBeam = true;
			}
			else {
				if (Time.time > beamTime + beamRate) {
					if (Time.time < beamTime + beamRate + 0.5f) {
						Fire();
					}
					fireTime = Time.time;
					beamTime = Time.time;
					chargingBeam = false;
				}
			}
		}
		else {
			chargingBeam = false;
		}
	}

	const float beamLifeTime = 1f;
	const float beamSpeed = 20f;

	void Fire() {
		//Spawn the projectile and set its velocity and lifetime
		GameObject projectile = (GameObject)Instantiate(gc.projectilePrefabs.laser, activeMuzzle.position, activeMuzzle.rotation);
		projectile.GetComponent<LaserProjectile>().Initialize(Common.RemoveYComp(activeMuzzle.forward)*beamSpeed, beamLifeTime, this);
		AudioManager.RetireFireSound(towerType);

		//Animate
		StartCoroutine(FireAnimation());
	}

	IEnumerator FireAnimation() {
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_FIRE);
		yield return null;
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_IDLE);
	}

	public void ApplyDamage(GameObject target, int enemiesHit) {

		//Deal damage
		//scale the damage we do down as we hit more enemies with the same projectile
		const int max_hits = 8;
		const float hit_decrement = 0.15f;
		float penetrationScale = 1f - (Mathf.Clamp(enemiesHit, 1f, max_hits) * hit_decrement);
		penetrationScale = Mathf.Clamp(penetrationScale, 0.1f, 1f); //cant go below 10 percent

		Enemy E = target.GetComponent<Enemy>();
		int killValue = E.TakeDamage((int)(damage*penetrationScale), towerType);
		if (killValue > 0) {//Enemy was killed
			PlayerStats.money += killValue;
			gc.SpawnKillFX(E.ID, target.transform.position);
		}
	}
}
