using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Single")]
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

		public override void PlayClipAtPoint(Vector3 position) {
			if (clip == null) return;

			ASHelper.PlayClipAtPoint(clip, position, Random.Range(volume.minValue, volume.maxValue), Random.Range(pitch.minValue, pitch.maxValue));
		}
	}
}