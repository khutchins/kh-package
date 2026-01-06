using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    [CreateAssetMenu(menuName = "KH/Subtitles/Speaker")]
    public class SubtitleSpeaker : ScriptableObject {
        public string Name;
        public Color Color;
    }
}