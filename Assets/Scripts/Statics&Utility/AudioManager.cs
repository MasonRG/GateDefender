using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AudioManager {

	#region Firing
	const int MAX_GUN_FIRE = 3;
	const int MAX_LASER_FIRE = 3;
	const int MAX_ROCKET_FIRE = 3;

	public static int numGunFireSounds=0;
	public static int numLaserFireSounds=0;
	public static int numRocketFireSounds=0;

	public static bool RequestFireSound(int towerType) {

		switch(towerType) {
		case Tower.TYPE_GUN:
			if (numGunFireSounds < MAX_GUN_FIRE) {
				numGunFireSounds++;
				return true;
			}
			else return false;
		case Tower.TYPE_LASER:
			if (numLaserFireSounds < MAX_LASER_FIRE) {
				numLaserFireSounds++;
				return true;
			}
			else return false;
		case Tower.TYPE_ROCKET:
			if (numRocketFireSounds < MAX_ROCKET_FIRE) {
				numRocketFireSounds++;
				return true;
			}
			else return false;
		}

		return false;
	}

	public static void RetireFireSound(int towerType) {
		switch(towerType) {
		case Tower.TYPE_GUN:	numGunFireSounds--;		break;
		case Tower.TYPE_LASER:	numLaserFireSounds--;	break;
		case Tower.TYPE_ROCKET:	numRocketFireSounds--;	break;
		}
	}
	#endregion

	#region Damage
	const int MAX_ROCKET_DMG = 2;

	public static int numRocketDmgSounds=0;

	public static bool RequestDamageSound(int towerType) {

		switch(towerType) {
		/*case Tower.TYPE_GUN:
			if (numGunFireSounds < MAX_GUN_FIRE) {
				numGunFireSounds++;
				return true;
			}
			else return false;
		case Tower.TYPE_LASER:
			if (numLaserFireSounds < MAX_LASER_FIRE) {
				numLaserFireSounds++;
				return true;
			}
			else return false;*/
		case Tower.TYPE_ROCKET:
			if (numRocketDmgSounds < MAX_ROCKET_DMG) {
				numRocketDmgSounds++;
				return true;
			}
			else return false;
		}

		return false;
	}

	public static void RetireDamageSound(int towerType) {
		switch(towerType) {
		/*case Tower.TYPE_GUN:	numGunFireSounds--;		break;
		case Tower.TYPE_LASER:	numLaserFireSounds--;	break;*/
		case Tower.TYPE_ROCKET:	numRocketDmgSounds--;	break;
		}
	}
	#endregion

	#region Enemy Kill
	const int MAX_KILL = 4;

	public static int numKillSounds = 0;

	public static bool RequestKillSound() {
		if (numKillSounds < MAX_KILL) {
			numKillSounds++;
			return true;
		}
		else
			return false;
	}

	public static void RetireKillSound() {
		numKillSounds--;
	}
	#endregion
}
