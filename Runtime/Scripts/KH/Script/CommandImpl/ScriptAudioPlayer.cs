using KH.Audio;
using KH.Console;
using KH.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace KH.Script {
    public class ScriptAudioPlayer : MonoBehaviour {
        [SerializeField] CommandChannel Channel;
        [SerializeField] AudioEventRegistry Registry;

        // Definition for the 'play_sound' command.
        [KVBSCommand("play_sound", "Plays the given AudioEvent.")]
        [KVBSArg(typeof(AudioEvent), "audioEvent", "The name of the AudioEvent asset to play.")]
        private static void _KVBSPlaySoundDefinition() { }

        private void OnEnable() {
            if (Channel == null) return;
            if (Registry == null) return;

            IEnumerator WaitForAudio(string[] argv, Action<string> logger) {
                string name = ScriptRunner.ExpectString(argv, 0);
                AudioEvent aevent = Registry.GetItem(name);
                if (aevent == null) {
                    logger($"Unknown audio event '{name}'");
                    yield break;
                }
                yield return aevent.Prepare().PlayIn2D().WaitForCompletion();
            }

            Channel.Register(new Command {
                Registrar = this,
                Name = "play_sound",
                Description = "play_sound AudioEventName - Play sound in 2D space.",
                RunCallbackAsync = WaitForAudio,
                Autocomplete = (parts) => {
                    if (parts.Length == 2) return Registry.AllItemNames.OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
                    return Array.Empty<string>();
                }
            });
        }

        private void OnDisable() {
            if (Channel == null) return;
            Channel.UnregisterRegistrar(this);
        }
    }
}