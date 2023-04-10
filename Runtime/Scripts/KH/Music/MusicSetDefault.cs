using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Music {
    public class MusicSetDefault : MonoBehaviour {
        [SerializeField] MusicSO Music;

        private void Start() {
            if (MusicManager.INSTANCE == null) {
                Debug.LogWarning("No instance of MusicManager!");
                return;
            }
            MusicManager.INSTANCE.ClearAndSetDefault(Music.ToInfo());
        }
    }
}