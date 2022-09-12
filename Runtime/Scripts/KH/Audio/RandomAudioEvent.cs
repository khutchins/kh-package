using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Random")]
	public class RandomAudioEvent : MultipleAudioEvent {
		public AudioEvent[] events;

		internal override AudioEvent NextEvent() {
			if (events == null || events.Length == 0) return null;
			return events[Random.Range(0, events.Length)];
		}
	}
}