using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunTower : Tower {

	private GameObject targetLastFrame;
	private float fireTime = 0;

	void Update() {

		if (!towerIsActive) {
			fireFXScript.StopFiring(towerType);
			fireFXScript.StopDamageFX(towerType);
			return;
		}

		//Default to the idle state; override later if firing
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_IDLE);

		if (nearbyEnemies.Count == 0) {
			fireFXScript.StopFiring(towerType);
			fireFXScript.StopDamageFX(towerType);
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
		if (target == null) {
			fireFXScript.StopFiring(towerType);
			fireFXScript.StopDamageFX(towerType);
		}
		else {
			if (TurnTowardsTarget(target.transform.position)) //returns true when facing enemy
				EngageTarget(target);
			else
				fireFXScript.StopDamageFX(towerType);
		}
	}

	void EngageTarget(GameObject target) {
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_FIRE);
		fireFXScript.StartFiring(towerType);
		//For damage fx
		Vector3 direction = target.transform.position - activeMuzzle.position;
		fireFXScript.MoveDamageFX(target.transform.position-direction.normalized*0.1f+Vector3.up*0.2f);
		fireFXScript.OrientDamageFX(activeMuzzle.position);
		fireFXScript.StartDamageFX(towerType);

		if (Time.time > fireTime + fireRate) {
			fireTime = Time.time;
			//Apply damage to enemy
			Enemy E = target.GetComponent<Enemy>();
			int killValue = E.TakeDamage(damage, towerType);
			if (killValue > 0) {//Enemy was killed
				PlayerStats.money += killValue;
				gc.SpawnKillFX(E.ID, target.transform.position);
			}
		}
	}
}
