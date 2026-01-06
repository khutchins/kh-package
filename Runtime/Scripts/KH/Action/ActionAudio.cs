using KH.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
    public class ActionAudio : Action {
        [SerializeField]
        AudioEvent Audio;
        [SerializeField]
        bool Is2DSound;
        [SerializeField]
        bool WaitForFinish;

		public override void Begin() {
            if (Audio == null) {
                Finished();
                return;
            }
            AudioPlaybackHandle handle;
            if (Is2DSound) {
                handle = Audio.Prepare().PlayIn2D();
            } else {
                handle = Audio.Prepare().PlayAtPoint(this.transform.position);
            }

            if (WaitForFinish) {
                // Use the new handle helper!
                StartCoroutine(Wait(handle));
            } else {
                Finished();
            }
        }

        private IEnumerator Wait(AudioPlaybackHandle handle) {
            yield return handle.WaitForCompletion();
            Finished();
        }
    }
}