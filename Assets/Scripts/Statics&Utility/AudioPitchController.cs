using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioPitchController : MonoBehaviour {

	private AudioSource aud;
	private float originalVolume;
	private float originalPitch;

	void Start() {
		aud = GetComponent<AudioSource>();
		originalVolume = aud.volume;
		originalPitch = aud.pitch;
	}

	void Update () {
		float ts = Mathf.Round(Time.timeScale);
		if (!Mathf.Approximately(ts, 0f)) {
			aud.pitch = originalPitch * ts;
			aud.volume = originalVolume;
		} else
			aud.volume = 0f;
	}
}
