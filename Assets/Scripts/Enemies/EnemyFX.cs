using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFX : MonoBehaviour {

	public ParticleSystem ps;
	public ParticleSystemRenderer rend;
	public AudioSource aud;
	public AudioClip[] clips;
	public Material[] materials;

	public void Initialize(int ID) {
		
		ParticleSystem.EmissionModule emit = ps.emission;

		//Material and emission is determined by color, indicated by the ID number mod4
		int i = ID % 4;
		rend.material = materials[i];
		emit.SetBursts( new ParticleSystem.Burst[] { new ParticleSystem.Burst(0.0f, (short)(15+(i*5)), (short)(15+(i*5))) } );

		ps.Play();
		if (AudioManager.RequestKillSound()) {
			int c = Random.Range(0, clips.Length);
			float p = Random.Range(0.95f, 1.05f);

			aud.clip = clips[c];
			aud.pitch = p;
			aud.Play();
			Invoke("RetireSound", 0.8f);
		}
	}

	void RetireSound() {
		AudioManager.RetireKillSound();
	}
}
