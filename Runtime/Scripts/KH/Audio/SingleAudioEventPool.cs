using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;
using KH.Audio;
using KH;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace KH.Audio {
	[CreateAssetMenu(menuName = "KH/Audio Events/Single with Pool")]
	public class SingleAudioEventPool : AudioEvent {
		public AudioClip clip;
		[Tooltip("Initial pool size.")]
		[SerializeField]
		int PoolSize = 5;
		[Tooltip("If true, the pool will not be allowed to grow, and the oldest element will be recycled.")]
		[SerializeField]
		bool CapSize = false;

		public RangedFloat volume = RangedFloat.One();

		[MinMaxRange(0, 2)]
		public RangedFloat pitch = RangedFloat.One();

		private List<AudioSource> Pool = new List<AudioSource>();
		private int _lastSource = -1;

		[SerializeField] AudioMixerGroup MixerGroup;

		public void InitializePool() {
			Pool.Clear();
			for (int i = 0; i < PoolSize; i++) {
				AddSourceToPool();
			}
		}

		private AudioSource GetAudioSource() {
			if (Pool.Count < PoolSize) {
				Debug.LogWarning($"AudioEvent {this.name} was not initialized! This may cause odd behavior.");
			}
			for (int i = 0; i < Pool.Count; i++) {
				AudioSource source = Pool[i];
				if (source == null) {
					source = GenerateSource();
					Pool[i] = source;
				}
				if (!source.isPlaying) {
					_lastSource = i;
					return source;
				}
			}
			if (CapSize) {
				_lastSource = (_lastSource + 1) % Pool.Count;
				return Pool[_lastSource];
			} else {
				_lastSource = Pool.Count;
				return AddSourceToPool();
			}
		}

		private AudioSource GenerateSource() {
			AudioSource audioSource = ASHelper.MakeAudioSource();
			audioSource.gameObject.name = $"AS: {this.name} (Pool)";
			audioSource.outputAudioMixerGroup = MixerGroup;
			return audioSource;
		}

		private AudioSource AddSourceToPool() {
			var source = GenerateSource();
			Pool.Add(source);
			return source;
		}

		public override AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
			// If using an explicit audio source, pool is not used.
			if (clip == null) return null;

			source.clip = clip;
			source.volume = GetVolume(volumeMod);
			source.pitch = GetPitch(pitchMod);
			source.Play();

			return source;
		}

		private float GetVolume(float volumeMod) {
			return Random.Range(volume.minValue, volume.maxValue) * volumeMod;
		}

		private float GetPitch(float pitchMod) {
			return Random.Range(pitch.minValue, pitch.maxValue) * pitchMod;
		}

		public override AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return null;

			AudioSource source = GetAudioSource();
			ASHelper.RepurposeAudioSource(source, clip, position, false, GetVolume(volumeMod), GetPitch(pitchMod), MixerGroup);
			source.Play();
			return source;
		}

		public override AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
			if (clip == null) return null;

			AudioSource source = GetAudioSource();
			ASHelper.RepurposeAudioSource(source, clip, Vector3.zero, true, GetVolume(volumeMod), GetPitch(pitchMod), MixerGroup);
			source.Play();
			return source;
		}
	}
}