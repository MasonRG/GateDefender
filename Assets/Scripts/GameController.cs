using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameController : MonoBehaviour {

	[System.Serializable]
	public class TowerPrefabs {
		public GameObject gun;
		public GameObject laser;
		public GameObject rocket;
	}
	[System.Serializable]
	public class VisualEffectsPrefabs {
		public GameObject generalGun, generalLaser, generalRocket, enemyKill;
		public AudioClip buildSound, sellSound, upgradeSound;
	}
	[System.Serializable]
	public class ProjectilePrefabs {
		public GameObject rocket, laser;
	}
		
	public TowerPrefabs towerPrefabs;
	public VisualEffectsPrefabs VFXPrefabs;
	public ProjectilePrefabs projectilePrefabs;
	public EnemyManager[] enemies;
	public GhostManager ghostManager;

	public GameObject rangeTracer;
	private RangeTracer rangeTracerScript;
	public bool DrawRangeActive {
		get {
			return 
				(ghostManager.isActive && ghostManager.isValid) ||
				selectedObject != null;
		}
	}

	[HideInInspector]
	public GameObject selectedObject;
	public int playerStartMoney = 100;
	public int playerStartHealth = 100;

	[Range(0f,1f), Tooltip("Percentage of total cost we get for selling.")] public float sellReturnPercent = 0.5f;
	[Range(0f,1f), Tooltip("Factor to multiply enemy threat level by to determine damage.")] public float enemyDamageFactor = 0.5f;
	[Range(0f,1f), Tooltip("Factor to multiply enemy threat level by to determine value.")] public float enemyValueFactor = 0.5f;


	//Assigning enemy health and speed from constants in EnemyManager class (purely for convenience)
	//and computing enemy damage and value and providing ID number for death VFX
	[ContextMenu("AssignAndComputeEnemyVariables")]
	public void AssignAndComputeEnemyVariables() {
		EnemyManager.AssignAndComputeVariables(enemies, enemyDamageFactor, enemyValueFactor);
	}


	void Awake () {
		InitializeRangeTracer();
		InitializeGhosts();
		ResetGhosts();

		PlayerStats.InitializeStats(playerStartMoney, playerStartHealth, 1);
	}

	#region Changing game speed & pause menu
	void Update() {
		if (Input.GetKey(KeyCode.Escape))		ChangeGameSpeed(0);
		else if (Input.GetKey(KeyCode.Alpha1))	ChangeGameSpeed(1);
		else if (Input.GetKey(KeyCode.Alpha2))	ChangeGameSpeed(2);
		else if (Input.GetKey(KeyCode.Alpha3))	ChangeGameSpeed(3);
	}
	public void ChangeGameSpeed(int newSpd) {
		if (newSpd > 3 || newSpd < 0)
			newSpd = 1;
		float spd = (float)newSpd;
		Time.timeScale = Mathf.Round(spd);
	}
	public void LoadMenu() {
		ChangeGameSpeed(1);
		UnityEngine.SceneManagement.SceneManager.LoadScene(0);
	}
	public void ReloadLevel() {
		ChangeGameSpeed(1);
		UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
	}
	#endregion

	#region Drawing Range
	void InitializeRangeTracer() {
		rangeTracer = (GameObject)Instantiate(rangeTracer, Vector3.zero, Quaternion.identity);
		rangeTracer.name = "Range Tracer";
		rangeTracerScript = rangeTracer.GetComponent<RangeTracer>();
		rangeTracerScript.Initialize(this);
	}

	public void DrawRange(Vector3 position, float range) {
		rangeTracer.transform.position = position;
		rangeTracerScript.SetVertices(range);
	}
	public void DrawRange(Vector3 position, int type, int tier) {
		Mathf.Clamp(tier, Tower.TIER_0, Tower.TIER_2);
		float range = 0f;
		switch(type) {
		case Tower.TYPE_GUN:
			switch(tier) {
			case Tower.TIER_0: range = Tower.Ranges.GUN_TIER_0;	break;
			case Tower.TIER_1: range = Tower.Ranges.GUN_TIER_1;	break;
			case Tower.TIER_2: range = Tower.Ranges.GUN_TIER_2;	break;
			}
			break;

		case Tower.TYPE_LASER:
			switch(tier) {
			case Tower.TIER_0: range = Tower.Ranges.LASER_TIER_0; break;
			case Tower.TIER_1: range = Tower.Ranges.LASER_TIER_1; break;
			case Tower.TIER_2: range = Tower.Ranges.LASER_TIER_2; break;
			}
			break;

		case Tower.TYPE_ROCKET:
			switch(tier) {
			case Tower.TIER_0: range = Tower.Ranges.ROCKET_TIER_0; break;
			case Tower.TIER_1: range = Tower.Ranges.ROCKET_TIER_1; break;
			case Tower.TIER_2: range = Tower.Ranges.ROCKET_TIER_2; break;
			}
			break;

		default: 
			Debug.LogWarning("Couldn't draw tower range: invalid tower type.");	
			return;
		}

		rangeTracer.transform.position = position;
		rangeTracerScript.SetVertices(range);
	}
	#endregion

	#region FX Spawning
	public GameObject SpawnFX(GameObject fxPrefab, Vector3 spawnPosition) {
		return (GameObject)Instantiate(fxPrefab, spawnPosition, Quaternion.identity);
	}
	public GameObject SpawnKillFX(int enemyID, Vector3 spawnPosition) {
		GameObject obj = (GameObject)Instantiate(VFXPrefabs.enemyKill, spawnPosition, Quaternion.identity);
		obj.GetComponent<EnemyFX>().Initialize(enemyID);
		return obj;
	}
	#endregion

	#region Towers
	public bool HasEnoughMoneyToBuild() {
		int targetCost = Tower.Costs.GUN_TIER_0;
		switch(ghostManager.GetActiveType()) {
		case Tower.TYPE_GUN:
			targetCost = Tower.Costs.GUN_TIER_0;
			break;
		case Tower.TYPE_LASER:
			targetCost = Tower.Costs.LASER_TIER_0;
			break;
		case Tower.TYPE_ROCKET:
			targetCost = Tower.Costs.ROCKET_TIER_0;
			break;
		}
		return PlayerStats.money >= targetCost;
	}

	public void SpawnTower(Vector3 spawnPosition) {
		int towerID = ghostManager.GetActiveType();
		GameObject prefab;
		int cost;
		switch(towerID) {
		case Tower.TYPE_GUN:
			prefab = towerPrefabs.gun;
			cost = Tower.Costs.GUN_TIER_0;
			break;
		case Tower.TYPE_LASER:
			prefab = towerPrefabs.laser;
			cost = Tower.Costs.LASER_TIER_0;
			break;
		case Tower.TYPE_ROCKET:
			prefab = towerPrefabs.rocket;
			cost = Tower.Costs.ROCKET_TIER_0;
			break;
		default: 
			Debug.LogWarning("Couldn't spawn tower: invalid towerID.");	
			return;
		}

		GameObject newTower = (GameObject)Instantiate(prefab, spawnPosition, Quaternion.identity);
		newTower.name = prefab.name+"_LVL0";
		PlayerStats.money -= cost;
		newTower.GetComponent<Tower>().StartBuild(towerID);
	}

	public bool SellTower() {
		if (selectedObject != null) {
			Tower T = selectedObject.GetComponent<Tower>();
			int returnValue = Mathf.RoundToInt(T.GetSellValue()*sellReturnPercent);
		
			if (T.StartSell()) { //are able to sell (not currently building, selling, or upgrading)
				PlayerStats.money += returnValue;
				selectedObject = null;
				return true;
			}
		}

		return false;
	}

	public bool UpgradeTower() {
		if (selectedObject != null) {
			Tower T = selectedObject.GetComponent<Tower>();
			int cost = T.GetUpgradeCost();
			if ( (PlayerStats.money >= cost) && (T.upgradeTier != Tower.TIER_2) ) { //have enough money and arent fully upgraded yet
				if (T.StartUpgrade()) { //are able to upgrade (not currently building, selling, or upgrading)
					PlayerStats.money -= cost;
					rangeTracerScript.SetVertices(T.range); //Update the range tracer now that the turret has been upgraded
					return true;
				}
			}
		}
		return false;
	}
	#endregion

	#region Ghosts
	void InitializeGhosts() {
		GameObject container = new GameObject("GhostObjects");

		ghostManager.towerType.gun = (GameObject)Instantiate(ghostManager.towerType.gun, Vector3.zero, Quaternion.identity, container.transform);
		ghostManager.towerType.gun.name = "Ghost_Gun";
		ghostManager.towerType.gun.GetComponent<GhostDetection>().Initialize(this, ghostManager.collideRadius);

		ghostManager.towerType.laser = (GameObject)Instantiate(ghostManager.towerType.laser, Vector3.zero, Quaternion.identity, container.transform);
		ghostManager.towerType.laser.name = "Ghost_Laser";
		ghostManager.towerType.laser.GetComponent<GhostDetection>().Initialize(this, ghostManager.collideRadius);

		ghostManager.towerType.rocket = (GameObject)Instantiate(ghostManager.towerType.rocket, Vector3.zero, Quaternion.identity, container.transform);
		ghostManager.towerType.rocket.name = "Ghost_Rocket";
		ghostManager.towerType.rocket.GetComponent<GhostDetection>().Initialize(this, ghostManager.collideRadius);

	}
	void ResetGhosts() {
		ghostManager.towerType.gun.SetActive(false);
		ghostManager.towerType.laser.SetActive(false);
		ghostManager.towerType.rocket.SetActive(false);
	}

	public void EnableTowerGhost(int towerID) {

		ResetGhosts();
		// 0->gun ; 1->laser ; 2->rocket
		switch(towerID) {
		case Tower.TYPE_GUN:	ghostManager.activeTower = ghostManager.towerType.gun;		break;
		case Tower.TYPE_LASER: 	ghostManager.activeTower = ghostManager.towerType.laser;	break;
		case Tower.TYPE_ROCKET:	ghostManager.activeTower = ghostManager.towerType.rocket;	break;
		default: 
			Debug.LogWarning("Couldn't activate ghost object: invalid towerID.");	
			return;
		}

		ghostManager.activeTower.SetActive(true);
		ghostManager.isActive = true;
	}

	public void DisableTowerGhost() {
		ResetGhosts();
		ghostManager.isActive = false;
	}
		
	#endregion
}
