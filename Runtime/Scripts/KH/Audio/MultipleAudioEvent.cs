using KH.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
	public abstract class MultipleAudioEvent : AudioEvent {
		public override AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
			return NextEvent()?.Play(source, volumeMod, pitchMod);
		}

		public override AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
			return NextEvent()?.PlayClipAtPoint(position, volumeMod, pitchMod);
		}

		public override AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
			return NextEvent()?.PlayOneShot(volumeMod, pitchMod);
		}

		internal abstract AudioEvent NextEvent();
	}
}