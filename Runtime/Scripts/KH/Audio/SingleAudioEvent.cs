using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using UnityEngine.Audio;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Single")]
	public class SingleAudioEvent : AudioEvent {
		public AudioClip clip;

		public RangedFloat volume = RangedFloat.One();

		[MinMaxRange(0, 2)]
		public RangedFloat pitch = RangedFloat.One();

		public AudioMixerGroup MixerGroup;

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool managed) {
            source.clip = clip;
            source.volume = Random.Range(volume.minValue, volume.maxValue) * config.VolumeMod;
            source.pitch = Random.Range(pitch.minValue, pitch.maxValue) * config.PitchMod;
            source.outputAudioMixerGroup = MixerGroup;
            return new AudioPlaybackHandle(source, runner, managed);
        }
    }
}