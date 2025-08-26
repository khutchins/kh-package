using KH.Audio;
using KH.Texts;
using UnityEngine;

namespace KH.Script {
    [KVBSAlias]
    [CreateAssetMenu(fileName = "LineSpecQueueRegistry", menuName = "KH/Registries/LineSpecQueue Registry")]
    public class LineSpecQueueRegistry : RegistrySO<LineSpecQueue> { }
}