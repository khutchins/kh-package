using KH.Audio;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KH.Notes {
    [CreateAssetMenu]
    public class Note : ScriptableObject {
        public Sprite Image;
        [TextArea]
        public string[] Pages;
        public AudioEvent PickUpAudio;
        public AudioEvent PageTurnAudio;
    }
}