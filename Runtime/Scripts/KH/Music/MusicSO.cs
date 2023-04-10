using System.Collections;
using UnityEngine;

namespace KH.Music {
    [CreateAssetMenu(menuName = "KH/Music/Music")]
    public class MusicSO : ScriptableObject {
        public AudioClip Clip;
        public float Volume = 1;
        public bool FadesIn;

        public MusicManager.MusicInfo ToInfo() {
            return new MusicManager.MusicInfo.Builder(Clip, Volume).SetFadesIn(FadesIn).Build();
        }
    }
}