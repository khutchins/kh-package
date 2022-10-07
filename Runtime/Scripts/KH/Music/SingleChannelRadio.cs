using System.Collections;
using System.Collections.Generic;
using KH.References;
using UnityEngine;

namespace KH.Music {
    public class SingleChannelRadio : MonoBehaviour {
        [Tooltip("Current radio station in the world somewhere.")]
        [SerializeField] RadioStation RadioStation;
        [SerializeField] AudioSource AudioSource;
        [Tooltip("Will hold the song title, like 'Song Artist - Song Title'.")]
        [SerializeField] StringReference SongInfoRef;

        private bool _on;
        private Coroutine _playingCoroutine;

        private void Awake() {
            AudioSource.loop = false;
        }

        public void TurnOn() {
            if (_on) return;
            _on = true;

            if (_playingCoroutine == null) {
                _playingCoroutine = StartCoroutine(PlayingCoroutine());
            }
        }

        public void TurnOff() {
            if (!_on) return;
            _on = false;

            if (_playingCoroutine != null) {
                StopCoroutine(_playingCoroutine);
                _playingCoroutine = null;
            }
            AudioSource.Stop();

            SongInfoRef?.SetValue(null);
        }

        public void Toggle() {
            if (_on) TurnOff();
            else TurnOn();
        }

        IEnumerator PlayingCoroutine() {
            RadioStation.SongSnapshot snapshot = RadioStation.GetCurrentPosition();
            while (true) {
                if (SongInfoRef != null) {
                    SongInfoRef.SetValue($"{snapshot.song.Artist} - {snapshot.song.Title}");
                }
                AudioSource.clip = snapshot.song.Audio;
                AudioSource.time = snapshot.startTime;
                AudioSource.Play();

                while (AudioSource.isPlaying) yield return null;

                snapshot = RadioStation.GetNextSong(snapshot);
            }
        }
    }
}