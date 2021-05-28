using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Random")]
	public class RandomAudioEvent : AudioEvent {
		public AudioEvent[] events;

		public override void Play(AudioSource source) {
			if (events.Length == 0) return;

			events[Random.Range(0, events.Length)].Play(source);
		}

		public override void PlayClipAtPoint(Vector3 position) {
			if (events.Length == 0) return;

			events[Random.Range(0, events.Length)].PlayClipAtPoint(position);
		}
	}
}