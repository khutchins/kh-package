using UnityEngine;

namespace KH.Script {
    public class KVBSAsset : ScriptableObject {
        [TextArea][SerializeField] private string text;
        public string Text => text;

        public void SetText(string value) => text = value ?? string.Empty;
    }
}