using System.Collections;
using UnityEngine;

namespace KH.Audio {
    public class AudioPlaybackHandle {
        public readonly AudioSource Source;
        protected readonly AudioProxy Runner;
        protected readonly bool IsManaged;
        protected bool _isDone = false;

        private Coroutine _followRoutine;

        public AudioPlaybackHandle(AudioSource source, AudioProxy runner, bool isManaged) {
            Source = source;
            Runner = runner;
            IsManaged = isManaged;
        }

        public virtual void Play() {
            Source.Play();
            if (IsManaged) {
                Object.Destroy(Source.gameObject, Source.clip.length / Mathf.Max(0.01f, Source.pitch) + 0.1f);
            }
        }


        public virtual void Stop() {
            StopImmediate();
        }

        public virtual void StopImmediate() {
            _isDone = true;
            if (_followRoutine != null) Runner.StopCoroutine(_followRoutine);
            if (Source != null) Source.Stop();
            Cleanup();
        }

        protected void Cleanup() {
            if (IsManaged && Source != null) {
                Object.Destroy(Source.gameObject);
            }
        }

        public void SetFollow(Transform target) {
            if (_followRoutine != null) Runner.StopCoroutine(_followRoutine);

            IEnumerator FollowTransform(Transform target) {
                while (Source != null && target != null) {
                    Source.transform.position = target.position;
                    yield return null;
                }
            }
            _followRoutine = Runner.StartCoroutine(FollowTransform(target));
        }

        public virtual bool IsPlaying {
            get {
                if (_isDone || Source == null) return false;
                return Source.isPlaying;
            }
        }

        public IEnumerator WaitForCompletion() {
            yield return null;
            while (IsPlaying) {
                yield return null;
            }
        }
    }
}