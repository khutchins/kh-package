using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Sequential")]
	public class SequentialAudioEvent : AudioEvent {
		public AudioEvent[] events;

		private int _index = -1;

		public void Reset() {
			_index = -1;
		}

		public override void Play(AudioSource source) {
			if (events.Length == 0) return;

			events[NewIndex()].Play(source);
		}

		public override void PlayClipAtPoint(Vector3 position) {
			if (events.Length == 0) return;

			events[NewIndex()].PlayClipAtPoint(position);
		}

		private int NewIndex() {
			_index = (_index + 1) % events.Length;
			return _index;
		}
	}
}