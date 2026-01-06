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

        private List<AudioSource> _pool = new List<AudioSource>();
        private int _lastIndex = -1;

        [SerializeField] AudioMixerGroup MixerGroup;

        public void InitializePool() {
            _pool.Clear();
            for (int i = 0; i < PoolSize; i++) {
                AddSourceToPool();
            }
        }

        private void ValidatePool() {
            if (_pool.Count > 0 && _pool[0] == null) {
                _pool.Clear();
                _lastIndex = -1;
            }
        }

        internal override (AudioSource source, AudioProxy proxy) GetManagedSource(PlaybackConfig config) {
            ValidatePool();
            if (_pool.Count == 0) InitializePool();

            AudioSource picked = null;

            foreach (var s in _pool) {
                if (!s.gameObject.activeSelf) {
                    picked = s;
                    break;
                }
            }

            if (picked == null && !CapSize) picked = AddSourceToPool();

            if (picked == null) {
                _lastIndex = (_lastIndex + 1) % _pool.Count;
                picked = _pool[_lastIndex];
            }

            picked.gameObject.SetActive(true);
            if (config.Position.HasValue) picked.transform.position = config.Position.Value;

            return (picked, picked.GetComponent<AudioProxy>());
        }

        private AudioSource GenerateSource() {
            AudioSource audioSource = ASHelper.MakeAudioSource();
            audioSource.gameObject.name = $"AS: {this.name} (Pool)";
            audioSource.outputAudioMixerGroup = MixerGroup;
            audioSource.gameObject.AddComponent<AudioProxy>();
            audioSource.gameObject.SetActive(false);
            return audioSource;
        }

        private AudioSource AddSourceToPool() {
            var source = GenerateSource();
            _pool.Add(source);
            return source;
        }

        public override AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool managed) {
            source.clip = clip;
            source.volume = Random.Range(volume.minValue, volume.maxValue) * config.VolumeMod;
            source.pitch = Random.Range(pitch.minValue, pitch.maxValue) * config.PitchMod;

            return new PooledPlaybackHandle(source, runner, managed);
        }
    }
}