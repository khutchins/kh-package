using KH.Script;
using UnityEngine;

// All AudioEvents inspired by https://www.youtube.com/watch?v=6vmRwLYWNRo.
namespace KH.Audio {
	[KVBSAlias]
	public abstract class AudioEvent : ScriptableObject {
		public abstract AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f);
		public abstract AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f);
		public abstract AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f);
	}
}