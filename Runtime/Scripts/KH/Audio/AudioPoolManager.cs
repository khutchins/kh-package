using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace KH.Audio {
    public class AudioPoolManager : MonoBehaviour {
        [SerializeField]
        private SingleAudioEventPool[] PooledEvents;

        private void Awake() {
            foreach (SingleAudioEventPool pool in PooledEvents) {
                pool.InitializePool();
            }
        }
    }
}