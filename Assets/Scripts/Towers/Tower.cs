using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Base class to store relevant constants/methods 
 * and instance specifications about towers.
 */
public abstract class Tower : MonoBehaviour {

	#region Tower Constants

	public const int TYPE_GUN=0, TYPE_LASER=1, TYPE_ROCKET=2;
	public const int TIER_0=0, TIER_1=1, TIER_2=2;

	public static class Costs {
		public const int GUN_TIER_0=50, GUN_TIER_1=150, GUN_TIER_2=400;
		public const int LASER_TIER_0=175, LASER_TIER_1=350, LASER_TIER_2=775;
		public const int ROCKET_TIER_0=325, ROCKET_TIER_1=550, ROCKET_TIER_2=975;
	}
	public static class Ranges {
		public const float GUN_TIER_0=3.5f, GUN_TIER_1=4.25f, GUN_TIER_2=5f;
		public const float LASER_TIER_0=5f, LASER_TIER_1=6f, LASER_TIER_2=7f;
		public const float ROCKET_TIER_0=7f, ROCKET_TIER_1=8f, ROCKET_TIER_2=9f;
	}
	public static class Damages {
		public const int GUN_TIER_0=10, GUN_TIER_1=16, GUN_TIER_2=25;
		public const int LASER_TIER_0=400, LASER_TIER_1=600, LASER_TIER_2=950;
		public const int ROCKET_TIER_0=550, ROCKET_TIER_1=840, ROCKET_TIER_2=1200;
	}
	public static class FireRates {
		public const float GUN_TIER_0=0.1f, GUN_TIER_1=0.08f, GUN_TIER_2=0.05f;
		public const float LASER_TIER_0=2.0f, LASER_TIER_1=1.75f, LASER_TIER_2=1.5f;
		public const float ROCKET_TIER_0=4f, ROCKET_TIER_1=3.5f, ROCKET_TIER_2=3f;
	}
	public static class AreaOfEffects {
		public const float GUN_TIER_0=0f, GUN_TIER_1=0f, GUN_TIER_2=0f;
		public const float LASER_TIER_0=0.5f, LASER_TIER_1=0.6875f, LASER_TIER_2=0.8125f;
		public const float ROCKET_TIER_0=1.25f, ROCKET_TIER_1=1.6625f, ROCKET_TIER_2=2.075f;
	}
	#endregion


	#region Instance-specific variables
	public Mesh[] meshTiers;
	public Material[] materialTiers;
	public Transform[] muzzleTiers;
	public SkinnedMeshRenderer smRenderer;

	public Transform rotateOrigin;
	public Transform turretBody;
	public Transform activeMuzzle;

	[HideInInspector] public int towerType;
	[HideInInspector] public int upgradeTier;
	[HideInInspector] public float range;
	[HideInInspector] public int damage;
	[HideInInspector] public float fireRate;
	[HideInInspector] public float areaOfEffect;

	public SphereCollider detectionCollider; //the sphere collider used to detect nearby enemies
	public FireFX fireFXScript; //Script to handle playing and stopping tower fx

	//Triggers will populate set: use hashset to avoid duplicates and because we don't need indexing
	protected HashSet<GameObject> nearbyEnemies = new HashSet<GameObject>();
	protected bool towerIsActive = false; //Used to determine if the tower is animating or otherwise not able to engage enemies
	protected GameController gc;
	protected Animator anim;
	#endregion



	#region Initialization
	public void Initialize(int type) {

		gc = GameObject.FindWithTag("GameController").GetComponent<GameController>();
		anim = GetComponent<Animator>();
		towerType = type;
		upgradeTier = TIER_0;

		switch(type) {
		case TYPE_GUN:
			range = Ranges.GUN_TIER_0;
			damage = Damages.GUN_TIER_0;
			fireRate = FireRates.GUN_TIER_0;
			areaOfEffect = AreaOfEffects.GUN_TIER_0;
			break;
		case TYPE_LASER:
			range = Ranges.LASER_TIER_0;
			damage = Damages.LASER_TIER_0;
			fireRate = FireRates.LASER_TIER_0;
			areaOfEffect = AreaOfEffects.LASER_TIER_0;
			break;
		case TYPE_ROCKET:
			range = Ranges.ROCKET_TIER_0;
			damage = Damages.ROCKET_TIER_0;
			fireRate = FireRates.ROCKET_TIER_0;
			areaOfEffect = AreaOfEffects.ROCKET_TIER_0;
			break;
		default: 
			Debug.LogWarning("Couldn't initialize tower: invalid tower type.");	
			return;
		}

		fireFXScript.ReParentDamageFX();
		detectionCollider.radius = range;
	}
	#endregion


	#region Enemy Targetting
	//As enemies enter our range, we add them to our set of nearby enemies
	void OnTriggerEnter(Collider other) {
		GameObject rootObj = Common.GetRootObject(other.transform).gameObject;
		if (rootObj.tag.Equals("Enemy"))
			nearbyEnemies.Add(rootObj);
	}

	//As enemies exit our range, we remove them from our set of nearby enemies
	void OnTriggerExit(Collider other) {
		GameObject rootObj = Common.GetRootObject(other.transform).gameObject;
		if (rootObj.tag.Equals("Enemy"))
			nearbyEnemies.Remove(rootObj);
	}
	#endregion


	#region Rotating
	//Returns true if we are facing the target
	protected bool TurnTowardsTarget(Vector3 position) {
		const float turnRate = 225f * Mathf.Deg2Rad; //how fast to turn
		const float turnThreshold = 15f; //the angle at which we consider the turret to be "facing" the target

		Vector3 localForward = rotateOrigin.forward; //this depends on the import of the model (for blender it swaps y and z axes)
		Vector3 direction = Common.RemoveYComp(position - rotateOrigin.position);

		Vector3 newForward = Vector3.RotateTowards(localForward, direction, turnRate*Time.deltaTime, 0.0f);
		rotateOrigin.rotation = Quaternion.LookRotation(newForward, Vector3.up);
		rotateOrigin.Rotate(0f,0f,-90f);

		return Vector3.Angle(direction, localForward) < turnThreshold;
	}
	#endregion


	#region Deploy/Retract Animation Management
	//To avoid bugs; deployment and retraction must be requested and potentially declined
	//So that's why there are start methods for them, otherwise selling-while-upgrading 
	//or upgrade-while-selling bugs will present themselves and make a disastrous mess.
	private bool isDeploying = false;
	private bool isRetracting = false;
	private bool isUpgrading = false;

	// Spawn --> Deploy
	public void StartBuild(int type) {
		Initialize(type);
		StartCoroutine(ManageBuild());
	}

	// Retract --> Upgrade --> Deploy
	public bool StartUpgrade() {
		if (isDeploying || isRetracting || isUpgrading)
			return false;

		StartCoroutine(ManageUpgrade());
		return true;
	}

	// Sell --> Retract --> Destroy
	public bool StartSell() {
		if (isDeploying || isRetracting || isUpgrading)
			return false;

		StartCoroutine(ManageSell());
		return true;
	}

	GameObject GetGeneralFXPrefab() {
		switch(towerType) {
		case Tower.TYPE_GUN:	return gc.VFXPrefabs.generalGun;
		case Tower.TYPE_LASER:	return gc.VFXPrefabs.generalLaser;
		case Tower.TYPE_ROCKET:	return gc.VFXPrefabs.generalRocket;
		default:				return gc.VFXPrefabs.generalGun;
		}
	}

	#region Coroutines for animation logic
	IEnumerator Deploy() { 
		isDeploying = true;

		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_UNFOLD);
		if (!TowerAnimation.IsUnfolding(anim))
			yield return new WaitUntil( () => TowerAnimation.IsUnfolding(anim));
		yield return new WaitUntil( () => !TowerAnimation.IsUnfolding(anim));
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_IDLE);

		isDeploying = false;
	}
	IEnumerator Retract() {
		isRetracting = true;

		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_FOLD);
		if (!TowerAnimation.IsFolding(anim))
			yield return new WaitUntil( () => TowerAnimation.IsFolding(anim));
		yield return new WaitUntil( () => !TowerAnimation.IsFolding(anim));
		TowerAnimation.ChangeState(anim, TowerAnimation.STATE_IDLE);

		isRetracting = false;
	}
	IEnumerator ManageBuild() {
		towerIsActive = false;
		GameObject fxObj = gc.SpawnFX(GetGeneralFXPrefab(), transform.position);
		AudioSource fxAud = fxObj.GetComponent<AudioSource>();
		fxAud.clip = gc.VFXPrefabs.buildSound;
		fxAud.Play();

		StartCoroutine(Deploy());
		if (!isDeploying) yield return new WaitUntil( () => isDeploying );
		yield return new WaitWhile( () => isDeploying );

		towerIsActive = true;
	}

	IEnumerator ManageUpgrade() {
		towerIsActive = false;
		isUpgrading = true;

		// 1. Upgrade the stats of the tower
		UpgradeStats();

		// 2. Retract the current tower
		StartCoroutine(Retract());
		if (!isRetracting) yield return new WaitUntil( () => isRetracting );
		yield return new WaitWhile( () => isRetracting );

		// 3. Upgrade the model and fx
		UpgradeModel();

		GameObject fxObj = gc.SpawnFX(GetGeneralFXPrefab(), transform.position);
		AudioSource fxAud = fxObj.GetComponent<AudioSource>();
		fxAud.clip = gc.VFXPrefabs.upgradeSound;
		fxAud.Play();

		// 4. Re-deploy new tower
		StartCoroutine(Deploy());
		if (!isDeploying) yield return new WaitUntil( () => isDeploying );
		yield return new WaitWhile( () => isDeploying );

		isUpgrading = false;
		towerIsActive = true;
	}

	IEnumerator ManageSell() {
		towerIsActive = false;

		StartCoroutine(Retract());
		if (!isRetracting) yield return new WaitUntil( () => isRetracting );
		yield return new WaitWhile( () => isRetracting );

		GameObject fxObj = gc.SpawnFX(GetGeneralFXPrefab(), transform.position);
		AudioSource fxAud = fxObj.GetComponent<AudioSource>();
		fxAud.clip = gc.VFXPrefabs.sellSound;
		fxAud.Play();
		Sell();
	}
	#endregion

	#endregion


	#region Upgrading
	//Return the cost of the upgrade
	public int GetUpgradeCost() {
		if (upgradeTier == TIER_2) //at max level already
			return 0;

		int cost = 0;
		switch(towerType) {
		case TYPE_GUN:
			if (upgradeTier == TIER_0)	cost = Costs.GUN_TIER_1;
			else						cost = Costs.GUN_TIER_2;
			break;
		case TYPE_LASER:
			if (upgradeTier == TIER_0)	cost = Costs.LASER_TIER_1;
			else 						cost = Costs.LASER_TIER_2;
			break;
		case TYPE_ROCKET:
			if (upgradeTier == TIER_0)	cost = Costs.ROCKET_TIER_1;
			else 						cost = Costs.ROCKET_TIER_2;
			break;
		}
		return cost;
	}

	private bool UpgradeStats() {
		
		switch(towerType) {
		case TYPE_GUN:

			if (upgradeTier == TIER_0) {
				upgradeTier = TIER_1;
				range = Ranges.GUN_TIER_1;
				damage = Damages.GUN_TIER_1;
				fireRate = FireRates.GUN_TIER_1;
				areaOfEffect = AreaOfEffects.GUN_TIER_1;
			}
			else {
				upgradeTier = TIER_2;
				range = Ranges.GUN_TIER_2;
				damage = Damages.GUN_TIER_2;
				fireRate = FireRates.GUN_TIER_2;
				areaOfEffect = AreaOfEffects.GUN_TIER_2;
			}
			break;

		case TYPE_LASER:

			if (upgradeTier == TIER_0) {
				upgradeTier = TIER_1;
				range = Ranges.LASER_TIER_1;
				damage = Damages.LASER_TIER_1;
				fireRate = FireRates.LASER_TIER_1;
				areaOfEffect = AreaOfEffects.LASER_TIER_1;
			}
			else {
				upgradeTier = TIER_2;
				range = Ranges.LASER_TIER_2;
				damage = Damages.LASER_TIER_2;
				fireRate = FireRates.LASER_TIER_2;
				areaOfEffect = AreaOfEffects.LASER_TIER_2;
			}
			break;

		case TYPE_ROCKET:

			if (upgradeTier == TIER_0) {
				upgradeTier = TIER_1;
				range = Ranges.ROCKET_TIER_1;
				damage = Damages.ROCKET_TIER_1;
				fireRate = FireRates.ROCKET_TIER_1;
				areaOfEffect = AreaOfEffects.ROCKET_TIER_1;
			}
			else {
				upgradeTier = TIER_2;
				range = Ranges.ROCKET_TIER_2;
				damage = Damages.ROCKET_TIER_2;
				fireRate = FireRates.ROCKET_TIER_2;
				areaOfEffect = AreaOfEffects.ROCKET_TIER_2;
			}
			break;

		default: 
			Debug.LogWarning("Couldn't upgrade tower: invalid tower type.");	
			return false;
		}
		return true;
	}

	private void UpgradeModel() {
		if (upgradeTier == TIER_1) {
			smRenderer.sharedMesh = meshTiers[0];
			smRenderer.sharedMaterial = materialTiers[0];
			activeMuzzle = muzzleTiers[0];
		}
		else if (upgradeTier == TIER_2) {
			smRenderer.sharedMesh = meshTiers[1];
			smRenderer.sharedMaterial = materialTiers[1];
			activeMuzzle = muzzleTiers[1];
		}

		detectionCollider.radius = range;
		fireFXScript.damageFX.parent = fireFXScript.transform;
		fireFXScript.transform.parent = activeMuzzle;
		fireFXScript.transform.localPosition = Vector3.zero;
		fireFXScript.transform.localRotation = Quaternion.identity;
		fireFXScript.ReParentDamageFX();
	}
	#endregion


	#region Selling
	//Returns the cost of the turret including all upgrades.
	public int GetSellValue() {

		int val = 0;
		switch(towerType) {
		case TYPE_GUN:
			val = Costs.GUN_TIER_0;
			if (upgradeTier != TIER_0)	val += Costs.GUN_TIER_1;
			if (upgradeTier == TIER_2)	val += Costs.GUN_TIER_2;
			break;
		case TYPE_LASER:
			val = Costs.LASER_TIER_0;
			if (upgradeTier != TIER_0)	val += Costs.LASER_TIER_1;
			if (upgradeTier == TIER_2)	val += Costs.LASER_TIER_2;
			break;
		case TYPE_ROCKET:
			val = Costs.ROCKET_TIER_0;
			if (upgradeTier != TIER_0)	val += Costs.ROCKET_TIER_1;
			if (upgradeTier == TIER_2)	val += Costs.ROCKET_TIER_2;
			break;
		default:
			Debug.LogWarning("Couldn't sell tower: invalid tower type or tier.");	
			return 0;
		}

		return val;
	}

	private void Sell() {
		Destroy(gameObject);
	}
	#endregion
}
