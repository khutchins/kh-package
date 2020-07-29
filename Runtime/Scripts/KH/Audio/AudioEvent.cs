using UnityEngine;

namespace KH.Audio {
	public abstract class AudioEvent : ScriptableObject {
		public abstract void Play(AudioSource source);
	}
}