using UnityEngine;

// All AudioEvents inspired by https://www.youtube.com/watch?v=6vmRwLYWNRo.
namespace KH.Audio {
	public abstract class AudioEvent : ScriptableObject {
        public PlaybackBuilder Prepare() {
            return new PlaybackBuilder(this);
        }

        internal AudioPlaybackHandle ExecutePlay(AudioSource source, AudioProxy runner, PlaybackConfig config, bool isManaged) {
            var handle = CreateHandle(source, runner, config, isManaged);
            if (config.FollowTarget != null) handle.SetFollow(config.FollowTarget);
            handle.Play();
            return handle;
        }

        internal virtual (AudioSource source, AudioProxy proxy) GetManagedSource(PlaybackConfig config) {
            AudioSource source = ASHelper.MakeAudioSource();
            AudioProxy proxy = source.gameObject.AddComponent<AudioProxy>();
            return (source, proxy);
        }

        public abstract AudioPlaybackHandle CreateHandle(AudioSource source, AudioProxy runner, PlaybackConfig config, bool isManaged);

        // Legacy methods
        public AudioSource Play(AudioSource source, float volumeMod = 1f, float pitchMod = 1f) {
            var handle = Prepare().WithVolume(volumeMod).WithPitch(1f).PlayUsingSource(source);
            return handle.Source;
        }
        public AudioSource PlayClipAtPoint(Vector3 position, float volumeMod = 1f, float pitchMod = 1f) {
            var handle = Prepare().WithVolume(volumeMod).WithPitch(1f).PlayAtPoint(position);
            return handle.Source;
        }

        public AudioSource PlayOneShot(float volumeMod = 1f, float pitchMod = 1f) {
            var handle = Prepare().WithVolume(volumeMod).WithPitch(1f).PlayIn2D();
            return handle.Source;
        }
    }
}