using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    [CreateAssetMenu(menuName = "KH/Subtitles/Voice")]
    public class SubtitleObjectVoice : SubtitleObject {
        public string Speaker;
        public Color Color;
        public string Message;
        public float Length;

        public override Subtitle AsSubtitle() {
            return Subtitle.ForVoiceLine(Speaker, Message, Length, Color);
        }
    }
}