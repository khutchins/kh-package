using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/WeightedRandom")]
	public class WeightedRandomAudioEvent : AudioEvent {
		[Serializable]
		public struct CompositeEntry {
			public AudioEvent Event;
			public float Weight;
		}

		public CompositeEntry[] Entries;

		private AudioEvent NextEvent() {
			float totalWeight = 0;
			for (int i = 0; i < Entries.Length; i++) {
				totalWeight += Entries[i].Weight;
			}

			float pick = Random.Range(0, totalWeight);
			for (int i = 0; i < Entries.Length; i++) {
				if (pick > Entries[i].Weight) {
					pick -= Entries[i].Weight;
					continue;
				}

				return Entries[i].Event;
			}
			return null;
		}

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool isManaged) {
            var aevent = NextEvent();
			if (aevent == null) return null;
			return aevent.CreateHandle(source, runner, config, isManaged);
        }
    }
}