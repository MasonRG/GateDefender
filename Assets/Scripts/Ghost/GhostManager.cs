using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GhostManager {

	[System.Serializable]
	public class GhostObjects {
		public GameObject gun;
		public GameObject laser;
		public GameObject rocket;
	}

	public GhostObjects towerType;

	[Range(0.5f, 3f)] 
	public float collideRadius = 1.5f;
	public Color validTurretColor = new Color(0f,1f,0f,0.5f);
	public Color validBaseColor = new Color(0f,0.8f,0f,0.5f);
	public Color invalidTurretColor = new Color(1f,0f,0f,0.5f);
	public Color invalidBaseColor = new Color(0.8f,0f,0f,0.5f);
	[HideInInspector] public GameObject activeTower;
	[HideInInspector] public bool isActive;
	[HideInInspector] public bool isValid;
	[HideInInspector] public bool isColliding;

	public int GetActiveType() {
		if (activeTower == towerType.gun)			return Tower.TYPE_GUN;
		else if (activeTower == towerType.laser)	return Tower.TYPE_LASER;
		else if (activeTower == towerType.rocket)	return Tower.TYPE_ROCKET;
		else {
			Debug.LogWarning("Error: could not determine active ghost tower type.");
			return -1;
		}
	}
}
