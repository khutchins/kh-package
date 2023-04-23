using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    /// <summary>
    /// Provides a thin wrapper around an AudioEvent to allow it to be called via Unity Events.
    /// </summary>
    public class AudioEventTrigger : MonoBehaviour {
        public AudioEvent AudioEvent;
        [Tooltip("How long this trigger should wait before retriggering.")]
        [SerializeField] float RetriggerCooldown = 0f;

        private float _lastPlay = 0f;

        private void CheckAndPlay(System.Action playAction) {
            if (Time.unscaledTime >= _lastPlay + RetriggerCooldown) {
                _lastPlay = Time.unscaledTime;
                playAction();
            }
        }

        public void PlayOneShot() {
            CheckAndPlay(() => {
                if (AudioEvent != null) AudioEvent.PlayOneShot();
            });
		}

        public void Play(AudioSource source) {
            CheckAndPlay(() => {
                if (AudioEvent != null) AudioEvent.Play(source);
            });
		}

        public void PlayClipAtPoint(Vector3 position) {
            CheckAndPlay(() => {
                if (AudioEvent != null) AudioEvent.PlayClipAtPoint(position);
            });
		}
    }
}