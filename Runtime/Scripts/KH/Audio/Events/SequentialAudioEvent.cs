using System.Collections;
using UnityEngine;
using static PlasticGui.PlasticTableColumn;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Sequential")]
	public class SequentialAudioEvent : AudioEvent {
		public AudioEvent[] events;

		private int _index = 0;

		public void Reset() {
			_index = 0;
		}

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool isManaged) {
            if (events == null || events.Length == 0) return null;

            AudioEvent audio = events[_index];
            _index = (_index + 1) % events.Length;
            return audio.CreateHandle(source, runner, config, isManaged);
        }
	}
}