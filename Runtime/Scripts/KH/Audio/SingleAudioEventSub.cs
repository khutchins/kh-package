using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Single With Subtitle")]
	public class SingleAudioEventSub : SingleAudioEvent {
		public SubtitleObject Subtitle;

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool managed) {
            if (Subtitle != null) SubtitleManager.AddSubtitleToDefaultManager(Subtitle);
            return base.CreateHandle(source, runner, config, managed);
        }
	}
}