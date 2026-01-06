using KH.Audio;
using System.Collections;
using UnityEngine;

namespace KH.Audio {
    public class StartLoopStopPlaybackHandle : AudioPlaybackHandle {
        private readonly StartLoopStopAudioEvent _asset;
        private Coroutine _activeRoutine;

        public StartLoopStopPlaybackHandle(StartLoopStopAudioEvent asset, AudioSource source, AudioProxy proxy, bool isManaged)
            : base(source, proxy, isManaged) { 
            _asset = asset;
        }

        public override void Play() => _activeRoutine = Runner.StartCoroutine(RunSequence());

        private IEnumerator RunSequence() {
            if (_asset.StartSound != null) {
                _asset.StartSound.Prepare().PlayUsingSource(Source);
                yield return new WaitWhile(() => Source.isPlaying);
            }
            if (_asset.LoopSound != null) {
                _asset.LoopSound.Prepare().PlayUsingSource(Source);
                Source.loop = true;
                Source.Play();
            }
        }

        public override void Stop() {
            if (_activeRoutine != null) Runner.StopCoroutine(_activeRoutine);
            Runner.StartCoroutine(GracefulStop());
        }

        private IEnumerator GracefulStop() {
            Source.Stop();
            Source.loop = false;
            if (_asset.StopSound != null) {
                _asset.StopSound.Prepare().PlayUsingSource(Source);
                if (IsManaged) yield return new WaitWhile(() => Source.isPlaying);
            }
            _isDone = true;
            Cleanup();
        }

        public override bool IsPlaying {
            get {
                if (_isDone || Source == null) return false;
                return true;
            }
        }
    }
}