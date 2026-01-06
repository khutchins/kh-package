using System.Collections;
using UnityEngine;

namespace KH.Audio {
    public class PooledPlaybackHandle : AudioPlaybackHandle {
        public PooledPlaybackHandle(AudioSource source, AudioProxy runner, bool isManaged) : base(source, runner, isManaged) { }

        public override void Play() {
            Source.Play();
            if (IsManaged) {
                Runner.StartCoroutine(DeactivateAfter(Source.clip.length / Mathf.Max(0.01f, Source.pitch) + 0.1f));
            }
        }

        private IEnumerator DeactivateAfter(float delay) {
            yield return new WaitForSeconds(delay);
            StopImmediate();
        }

        public override void Stop() => StopImmediate();

        public override void StopImmediate() {
            if (Source != null) Source.Stop();
            if (IsManaged && Source != null) Source.gameObject.SetActive(false);
        }
    }
}