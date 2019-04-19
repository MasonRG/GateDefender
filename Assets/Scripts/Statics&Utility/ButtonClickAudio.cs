using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClickAudio : MonoBehaviour {

	private AudioSource aud;
	public AudioClip[] clips;
	public AudioClip winSound;
	public AudioClip loseSound;

	void Start() {
		aud = GetComponent<AudioSource>();
	}

	public void PlayButtonSound(int index) {
		if (aud.isPlaying)
			aud.Stop();
		index = Mathf.Clamp(index, 0, clips.Length-1);
		aud.clip = clips[index];
		aud.Play();
	}

	public void PlayGameOverSound(bool isWin) {
		if (aud.isPlaying)
			aud.Stop();
		aud.clip = (isWin)? winSound : loseSound;
		aud.Play();
	}
}
