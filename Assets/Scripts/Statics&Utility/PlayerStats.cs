using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class PlayerStats {

	public static int money;
	public static int health;

	public static int maxWaves;
	public static int wave;
	public static int waveWaitSeconds;

	public static bool gameOver;

	public static void InitializeStats(int startMoney, int startHealth, int startWave) {
		PlayerStats.money = startMoney;
		PlayerStats.health = startHealth;
		PlayerStats.wave = startWave;
		PlayerStats.waveWaitSeconds = 0;
		PlayerStats.gameOver = false;
		PlayerStats.maxWaves = 20;
	}
}
