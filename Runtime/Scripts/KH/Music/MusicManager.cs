using KH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Music {
    [RequireComponent(typeof(AudioSource))]
    public class MusicManager : MonoBehaviour {

        public static MusicManager INSTANCE;

        private AudioSource _source;
        private List<MusicInfoInstance> _musicStates = new List<MusicInfoInstance>();
        private MusicInfoInstance _currentInstance;
        private static readonly float FADE_IN_TIME = 0.5f;

        private SingleCoroutineManager _fadeInstance;

        private void Awake() {
            INSTANCE = this;
            _source = GetComponent<AudioSource>();
            _source.loop = true;
            _fadeInstance = new SingleCoroutineManager(this);
        }

        class MusicInfoInstance {
            public readonly string Token;
            public readonly MusicInfo Info;
            public MusicState State;

            public MusicInfoInstance(string token, MusicInfo info) {
                Token = token;
                Info = info;
                State = new MusicState() { Position = 0 };
            }
        }

        public struct MusicState {
            public int Position;
        }

        public void ClearAndSetDefault(MusicInfo info, bool forceRestart = false) {
            if (info == null) {
                _source.Stop();
                _musicStates.Clear();
                return;
            }
            _musicStates.Clear();
            if (_currentInstance == null || info == null || _currentInstance.Info.Audio != info.Audio || forceRestart) {
                _source.Stop();
                MusicInfoInstance instance = new MusicInfoInstance("", info);
                AddAndPlay(instance);
            } else {
                // Just restore the default.
                _musicStates.Add(_currentInstance);
            }
        }

        public void AddMusicToStack(string token, MusicInfo info) {
            MusicInfoInstance instance = new MusicInfoInstance(token, info);
            AddAndPlay(instance);
        }

        private void AddAndPlay(MusicInfoInstance instance) {
            _musicStates.Add(instance);
            PlayTopOfStack();
        }

        public void PopMusicFromStack(string token) {
            for (int i = _musicStates.Count - 1; i >= 0; i--) {
                if (_musicStates[i].Token == token) {
                    _musicStates.RemoveAt(i);
                    break;
                }
            }
            // Only clear if it was on top.
            if (_currentInstance.Token == token) {
                PlayTopOfStack();
            }
        }

        private void PlayTopOfStack() {
            if (_currentInstance != null) {
                _currentInstance.State.Position = _source.timeSamples;
            }

            if (_musicStates.Count == 0) {
                _currentInstance = null;
            } else {
                _currentInstance = _musicStates[_musicStates.Count - 1];
            }
            PlayInternal();
        }

        private void PlayInternal() {
            _fadeInstance.StopCoroutine();
            if (_currentInstance == null) {
                _source.Stop();
            } else {
                _source.clip = _currentInstance.Info.Audio;
                _source.volume = _currentInstance.Info.Volume;
                _source.Play();
                if (_currentInstance.Info.FadesIn) {
                    _fadeInstance.StartCoroutine(FadeFrom(0, _currentInstance.Info.Volume, FADE_IN_TIME));
                }
            }
        }

        private IEnumerator FadeFrom(float from, float to, float duration) {
            yield return EZTween.DoPercentAction((float percent) => {
                _source.volume = Mathf.Lerp(from, to, percent);
            }, duration);
        }

        public class MusicInfo {
            public readonly AudioClip Audio;
            public readonly float Volume;
            public readonly bool FadesIn;

            MusicInfo(AudioClip audio, float volume, bool fadesIn) {
                Audio = audio;
                Volume = volume;
                FadesIn = fadesIn;
            }

            public class Builder {
                private AudioClip _clip;
                private float _volume;
                private bool _fadesIn;

                public Builder(AudioClip audio, float volume = 1) {
                    _clip = audio;
                    _volume = volume;
                }

                public Builder SetFadesIn(bool fadesIn) {
                    _fadesIn = fadesIn;
                    return this;
                }

                public MusicInfo Build() {
                    return new MusicInfo(_clip, _volume, _fadesIn);
                }
            }
        }
    }
}