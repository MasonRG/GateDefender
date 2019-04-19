using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyManager {

	public string name = "Enemy";	//Name of this type of enemy (currently unused)
	public GameObject prefab;		//The prefab/model this enemy will use
	public float modelWidth = 1f;	//How wide the model is along the x-axis
	public float startSpeed = 5f;	//Initial speed
	public int startHealth = 100;	//Initial health

	//precomputed, called from GameController in inspector
	[HideInInspector]public int damage; 
	[HideInInspector]public int value;
	[HideInInspector]public int id;

	//Values [-1,0] are vulnerability: ie. -1 means double damage, 0 is normal damage
	//Values [0,1] are resistance: ie. 1 means invulverability, 0 is normal damage
	[Range(-1f,1f), Tooltip("Resistance to gun towers: -1 receives double damage, 1 receives none.")]
	public float gunResistance = 0f;
	[Range(-1f,1f), Tooltip("Resistance to laser towers: -1 receives double damage, 1 receives none.")]
	public float laserResistance = 0f;
	[Range(-1f,1f), Tooltip("Resistance to rocket towers: -1 receives double damage, 1 receives none.")]
	public float rocketResistance = 0f;



	//For quickly changing all enemy health and speed
	static readonly int[] HEALTHS = 
	{
		40,85,140,250, //spheres
		120,160,200,290, //boxes
		325,365,400,475, //hexes
		1200,1700,2400,3400 //diamonds
	};
	static readonly float[] SPEEDS = 
	{
		2.6f,2.4f,2.2f,2.0f, //spheres
		3.4f,3.2f,3.0f,2.8f, //boxes
		4.2f,4.0f,3.8f,3.6f, //hexes
		5.0f,4.8f,4.6f,4.4f //diamonds
	};	

	/* Damage and value are based on the health of the enemy.
	 * Sqrt is used to lower value deviation.
	 * Scalefactor linearly scales all values.
	 * lvl4Reduction is used to further reduce for high-level enemies
	 * due to the large increase in health that they get.
	 */
	public static void AssignAndComputeVariables(EnemyManager[] enemiesArray, float damageFactor, float valueFactor) {
		const float R = 0.33f;
		int i = 0;
		foreach(EnemyManager em in enemiesArray) {
			em.startHealth = HEALTHS[i];
			em.startSpeed = SPEEDS[i];
			float lvl4Reduction = (em.startHealth > 1000)? R : 1f;
			em.damage = Mathf.RoundToInt( Mathf.Sqrt(em.startHealth*damageFactor*lvl4Reduction) );
			em.value = Mathf.RoundToInt( Mathf.Sqrt(em.startHealth*valueFactor*lvl4Reduction) );
			em.id = i;
			i++;
		}
	}
}
