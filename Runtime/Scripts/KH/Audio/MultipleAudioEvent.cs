using KH.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MultipleAudioEvent : AudioEvent {
	public override void Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
		NextEvent()?.Play(source, volumeMod, pitchMod);
	}

	public override void PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
		NextEvent()?.PlayClipAtPoint(position, volumeMod, pitchMod);
	}

	internal abstract AudioEvent NextEvent();
}
