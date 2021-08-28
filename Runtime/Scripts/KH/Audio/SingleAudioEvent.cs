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

		public override void Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return;

			source.clip = clip;
			source.volume = Random.Range(volume.minValue, volume.maxValue);
			source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
			source.Play();
		}

		public override void PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return;

			ASHelper.PlayClipAtPoint(clip, position, false, volumeMod * Random.Range(volume.minValue, volume.maxValue), pitchMod * Random.Range(pitch.minValue, pitch.maxValue));
		}

		public override void PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return;

			ASHelper.PlayClipAtPoint(clip, Vector3.zero, true, volumeMod * Random.Range(volume.minValue, volume.maxValue), pitchMod * Random.Range(pitch.minValue, pitch.maxValue));
		}
	}
}