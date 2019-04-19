using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {


	private int health;
	private float speed;
	private int damage; //how much to damage the player: logarithmically based on health
	private int value; //how much money to pay the player upon death: logarithmically based on health
	private int id; //used to determine what material/emission to use on death vfx -> in range [0,4]
	private float gunResist;
	private float laserResist;
	private float rocketResist;

	private Waypoint wp;
	private float offset;

	//healthbar needs access to health
	public int Health { get { return health; } }

	//gamecontroller needs access to color index
	public int ID { get { return id;} }

	public void Initialize(EnemyManager manager, GameController gc, Waypoint wp, float offset) {
		//Import stats from the enemy creator
		//Damage and value are calculated by the GameController in the editor
		health = manager.startHealth;
		speed = manager.startSpeed;
		damage = manager.damage;
		value = manager.value;
		id = manager.id;
		gunResist = manager.gunResistance;
		laserResist = manager.laserResistance;
		rocketResist = manager.rocketResistance;

		//Set up waypoint traversal
		this.wp = wp;
		this.offset = offset;
	}

	void Update() {

		const float rotateSpeed = Mathf.Deg2Rad * 180f; //radians per second
		Vector3 direction = wp.FindWay(transform.position, offset);
		float sqrDistance = direction.sqrMagnitude;

		if (sqrDistance < 0.075f) {
			//If we get to the last node damage the player and remove the enemy
			if (wp.next == null)
				DamagePlayer();
			else
				wp = wp.next;
		}

		transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(transform.forward, direction, rotateSpeed * Time.deltaTime, 0.0f));
		transform.position += direction.normalized * speed * Time.deltaTime;

	//	Debug.DrawLine (transform.position, transform.position + direction, Color.green);
	}
		
	void DamagePlayer() {
		PlayerStats.health = Mathf.Clamp(PlayerStats.health-damage, 0, PlayerStats.health);
		Destroy(gameObject);
	}


	//Returns 'value' if this enemy was destroyed by the attack; 0 otherwise
	public int TakeDamage(int dmg, int towerType) {

		//Get appropriate resistance level
		float resistance;
		switch(towerType) {
		case Tower.TYPE_GUN:	resistance = gunResist;		break;
		case Tower.TYPE_LASER:	resistance = laserResist; 	break;
		case Tower.TYPE_ROCKET:	resistance = rocketResist; 	break;
		default:				resistance = 0;				break;
		}

		//Apply resistance to incoming damage (resistance is 0 to 1, 0 being default damage and 1 being invulnerability)
		//Note: negative resistance is vulnerability; so it will be a factor between 1 and 2 (2 being double damage)
		if (resistance < 0)
			resistance = 1f + Mathf.Abs(resistance);//convert negative value to positive value in [1,2]
		else
			resistance = 1f - resistance;

		health -= (int)(dmg*resistance);
		bool wasFatal = health <= 0;

		if (wasFatal) {
			Destroy(gameObject);
			return value;
		} else return 0;
	}
}
