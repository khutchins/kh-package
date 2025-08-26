using KH.Audio;
using UnityEngine;

namespace KH.Script {
    [KVBSAlias]
    [CreateAssetMenu(fileName = "AudioEventRegistry", menuName = "KH/Registries/AudioEvent Registry")]
    public class AudioEventRegistry : RegistrySO<AudioEvent> { }
}