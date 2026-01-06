using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    [CreateAssetMenu(menuName = "KH/Subtitles/Sound Effects")]
    public class SubtitleObjectSfx : SubtitleObject {
        public string Sfx;
        public float Length;
        public override Subtitle AsSubtitle() {
            return Subtitle.ForSoundEffect(Sfx, Length);
        }
    }
}