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

		public override AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return null;

			source.clip = clip;
			source.volume = Random.Range(volume.minValue, volume.maxValue);
			source.pitch = Random.Range(pitch.minValue, pitch.maxValue);
			if (MixerGroup != null) source.outputAudioMixerGroup = MixerGroup;
			source.Play();

			return source;
		}

		public override AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return null;

			return ASHelper.PlayClipAtPoint(clip, position, false, volumeMod * Random.Range(volume.minValue, volume.maxValue), pitchMod * Random.Range(pitch.minValue, pitch.maxValue));
		}

		public override AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return null;

			return ASHelper.PlayClipAtPoint(clip, Vector3.zero, true, volumeMod * Random.Range(volume.minValue, volume.maxValue), pitchMod * Random.Range(pitch.minValue, pitch.maxValue));
		}
	}
}