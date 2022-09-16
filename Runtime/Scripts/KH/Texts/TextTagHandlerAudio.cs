using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KH.Audio;
using UnityEngine;

namespace KH.Texts {
    public class TextTagHandlerAudio : TextTagHandler {

        [Tooltip("Keys that can be referenced in the text to play an audio event.")]
        [SerializeField] TextCombo[] AudioHooks;

        [System.Serializable]
        public class TextCombo {
            public string Key;
            public AudioEvent AudioEvent;
        }

        public override void TextProgressed(TextUpdate textUpdate) {
            foreach (TextToken token in textUpdate.UnrecognizedTags.Where(x => x.key == "audio")) {
                AudioEvent aEvent = EventForToken(token.value);
                if (aEvent == null) {
                    Debug.LogWarning($"No sound found for token value {token.value}");
                }
                aEvent?.PlayOneShot();
            }
        }

        AudioEvent EventForToken(string key) {
            foreach (TextCombo combo in AudioHooks) {
                if (combo.Key == key) return combo.AudioEvent;
            }
            return null;
        }
    }
}