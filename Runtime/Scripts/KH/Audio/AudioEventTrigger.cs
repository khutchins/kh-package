using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    /// <summary>
    /// Provides a thin wrapper around an AudioEvent to allow it to be called via Unity Events.
    /// </summary>
    public class AudioEventTrigger : MonoBehaviour {
        public AudioEvent AudioEvent;

        public void PlayOneShot() {
            AudioEvent?.PlayOneShot();
		}

        public void Play(AudioSource source) {
            AudioEvent?.Play(source);
		}

        public void PlayClipAtPoint(Vector3 position) {
            AudioEvent?.PlayClipAtPoint(position);
		}
    }
}