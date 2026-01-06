using System.Collections;
using UnityEngine;

namespace KH.Audio {
    public class PooledPlaybackHandle : AudioPlaybackHandle {
        private SingleCoroutineManager _deactivateManager;
        public PooledPlaybackHandle(AudioSource source, AudioProxy runner, bool isManaged) : base(source, runner, isManaged) {
            _deactivateManager = new SingleCoroutineManager(runner);
        }

        public override void Play() {
            Source.Play();
            if (IsManaged) {
                _deactivateManager.StartCoroutine(DeactivateAfter(Source.clip.length / Mathf.Max(0.01f, Source.pitch) + 0.1f));
            }
        }

        private IEnumerator DeactivateAfter(float delay) {
            yield return new WaitForSeconds(delay);
            StopImmediate();
        }

        public override void Stop() => StopImmediate();

        public override void StopImmediate() {
            _deactivateManager.StopCoroutine();
            if (Source != null) Source.Stop();
            if (IsManaged && Source != null) Source.gameObject.SetActive(false);
        }
    }
}