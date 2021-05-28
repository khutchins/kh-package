using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Sequential")]
	public class SequentialAudioEvent : MultipleAudioEvent {
		public AudioEvent[] events;

		private int _index = 0;

		public void Reset() {
			_index = 0;
		}

		internal override AudioEvent NextEvent() {
			if (events.Length == 0) return null;

			AudioEvent audio = events[_index];
			_index = (_index + 1) % events.Length;
			return audio;
		}
	}
}