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
            AudioSource source = null;
            if (Is2DSound) source = Audio.PlayOneShot();
            else source = Audio.PlayClipAtPoint(this.transform.position);
            if (WaitForFinish) {
                StartCoroutine(WaitForFinishCoroutine(source));
            } else {
                Finished();
            }
		}

        IEnumerator WaitForFinishCoroutine(AudioSource source) {
            yield return new WaitWhile(() => source != null && source.isPlaying);
            Finished();
        }
    }
}