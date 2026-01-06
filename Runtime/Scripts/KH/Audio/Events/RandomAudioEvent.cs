using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Random")]
	public class RandomAudioEvent : AudioEvent {
		public AudioEvent[] events;

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool managed) {
            if (events == null || events.Length == 0) return null;
            var chosen = events[Random.Range(0, events.Length)];
            return chosen.CreateHandle(source, runner, config, managed);
        }
	}
}