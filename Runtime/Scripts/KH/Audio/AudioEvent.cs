using UnityEngine;

// All AudioEvents inspired by https://www.youtube.com/watch?v=6vmRwLYWNRo.
namespace KH.Audio {
	public abstract class AudioEvent : ScriptableObject {
		public abstract void Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f);
		public abstract void PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f);
		public abstract void PlayOneShot(float volumeMod = 1f, float pitchMod = 1f);
	}
}