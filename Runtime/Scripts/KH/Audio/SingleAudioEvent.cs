using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "Audio Events/Single")]
	public class SingleAudioEvent : AudioEvent {
		public AudioClip clip;

		public RangedFloat volume = RangedFloat.One();

		[MinMaxRange(0, 2)]
		public RangedFloat pitch = RangedFloat.One();

		public override void Play(AudioSource source) {
			if (clip == null) return;

			source.clip = clip;
			source.volume = Random.Range(volume.minValue, volume.maxValue);
			source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
			source.Play();
		}
	}
}