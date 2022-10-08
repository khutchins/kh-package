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

		public override void Begin() {
            if (Is2DSound) Audio?.PlayOneShot();
            else Audio?.PlayClipAtPoint(this.transform.position);
            Finished();
		}
    }
}