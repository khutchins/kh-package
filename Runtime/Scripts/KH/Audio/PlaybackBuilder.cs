using UnityEngine;

namespace KH.Audio {
    public class PlaybackBuilder {
        private readonly AudioEvent _asset;
        private PlaybackConfig _config;

        public PlaybackBuilder(AudioEvent asset) {
            _asset = asset;
            _config = PlaybackConfig.Default;
        }

        public PlaybackBuilder WithVolume(float vol) {
            _config.VolumeMod = vol;
            return this;
        }

        public PlaybackBuilder WithPitch(float pitch) {
            _config.PitchMod = pitch;
            return this;
        }

        public AudioPlaybackHandle PlayUsingSource(AudioSource source) {
            if (!source.TryGetComponent<AudioProxy>(out var runner)) runner = source.gameObject.AddComponent<AudioProxy>();
            return _asset.ExecutePlay(source, runner, _config, isManaged: false);
        }

        public AudioPlaybackHandle PlayAtPoint(Vector3 position) {
            _config.Position = position;
            _config.SpatialBlend = 1f;
            return SpawnAndExecute();
        }

        public AudioPlaybackHandle PlayFollowing(Transform target) {
            _config.FollowTarget = target;
            _config.SpatialBlend = 1f;
            return SpawnAndExecute();
        }

        public AudioPlaybackHandle PlayIn2D() {
            _config.SpatialBlend = 0f;
            return SpawnAndExecute();
        }

        private AudioPlaybackHandle SpawnAndExecute() {
            var (source, runner) = _asset.GetManagedSource(_config);
            if (_config.Position.HasValue) source.transform.position = _config.Position.Value;
            source.spatialBlend = _config.SpatialBlend;
            return _asset.ExecutePlay(source, runner, _config, isManaged: true);
        }
    }
}