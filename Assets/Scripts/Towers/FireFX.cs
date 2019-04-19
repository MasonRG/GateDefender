using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireFX : MonoBehaviour {

	private AudioSource aud;
	private ParticleSystem sys;

	public Transform damageFX;
	private AudioSource dmgAud;
	private ParticleSystem dmgSys;


	void Awake() {
		aud = GetComponent<AudioSource>();
		sys = GetComponent<ParticleSystem>();
		dmgAud = damageFX.GetComponent<AudioSource>();
		dmgSys = damageFX.GetComponent<ParticleSystem>();
	}
		

	public void StartFiring(int towerType) {
		if (!aud.isPlaying) {
			if (AudioManager.RequestFireSound(towerType)) {
				aud.Play();
			}
		}
		if (!sys.isPlaying)
			sys.Play();
	}
	public void StopFiring(int towerType) {
		if (sys.isPlaying) {
			if (aud.isPlaying) {
				AudioManager.RetireFireSound(towerType);
				aud.Stop();
			}
			sys.Stop();
		}
	}

	public void ReParentDamageFX() {
		damageFX.parent = Common.GetRootObject(damageFX);
	}
	public void MoveDamageFX(Vector3 position) {
		damageFX.position = position;
	}
	public void OrientDamageFX(Vector3 lookAt) {
		damageFX.LookAt(lookAt);
	}
	public void OrientDamageFX(Quaternion rotation) {
		damageFX.rotation = rotation;
	}

	public void StartDamageFX(int towerType) {
		if(!dmgSys.isPlaying)
			dmgSys.Play();
		if (dmgAud != null && !dmgAud.isPlaying) {
			if (AudioManager.RequestDamageSound(towerType))
				dmgAud.Play();
		}
	}
	public void StopDamageFX(int towerType) {
		if (dmgSys.isPlaying) {
			if (dmgAud != null && dmgAud.isPlaying) {
				AudioManager.RetireDamageSound(towerType);
				dmgAud.Stop();
			}
			dmgSys.Stop();
		}

	}
}
