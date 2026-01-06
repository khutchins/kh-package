using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    [CreateAssetMenu(menuName = "KH/Subtitles/VoiceSpeaker")]
    public class SubtitleObjectVoiceSpeaker : SubtitleObject {
        public SubtitleSpeaker Speaker;
        public string Message;
        public float Length;

        public override Subtitle AsSubtitle() {
            return Subtitle.ForVoiceLine(Speaker.Name, Message, Length, Speaker.Color);
        }
    }
}