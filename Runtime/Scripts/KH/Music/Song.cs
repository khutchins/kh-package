using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Music {
    [CreateAssetMenu(menuName = "KH/Song")]
    public class Song : ScriptableObject {
        public string Artist;
        public string Title;
        public AudioClip Audio;
    }
}