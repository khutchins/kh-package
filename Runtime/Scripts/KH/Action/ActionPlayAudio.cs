using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionPlayAudio : Action {

		public AudioSource Audio;
		public bool WaitForCompletion = false;

		public override void Begin() {
			if (Audio == null) {
				Debug.LogWarning("PlayAudio playing null audio");
				Finished();
			}
			Audio.Play();
			if (WaitForCompletion) {
				StartCoroutine(WaitForSource(Audio));
			} else {
				Finished();
			}
		}

		IEnumerator WaitForSource(AudioSource audio) {
			while (audio.isPlaying) {
				yield return null;
			}
			Finished();
		}
	}
}