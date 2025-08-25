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
    public class ScriptSoundPlayer : MonoBehaviour {
        [SerializeField] CommandChannel Channel;
        [SerializeField] AudioEvent[] audioEvents = Array.Empty<AudioEvent>();
        private Dictionary<string, AudioEvent> _audioEventsByName;

        private void Awake() {
            RebuildDictionaries();
        }

        private void RebuildDictionaries() {
            _audioEventsByName = (audioEvents ?? Array.Empty<AudioEvent>()).Where(x => x != null).GroupBy(x => x.name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

#if UNITY_EDITOR
        [KVBSCommand("play_sound", typeof(AudioEvent))]
        private static void _KVBSDefinition() { }
#endif

        private void OnEnable() {
            if (Channel == null) return;

            IEnumerator WaitForAudio(string[] argv, Action<string> logger) {
                string name = ScriptRunner.ExpectString(argv, 0);
                if (!_audioEventsByName.TryGetValue(name, out var aevent)) {
                    logger($"Unknown audio event '{name}'");
                    yield break;
                }
                var source = aevent.PlayOneShot();
                yield return new WaitWhile(() => source != null && source.isPlaying);
            }

            Channel.Register(new Command {
                Registrar = this,
                Name = "play_sound",
                Description = "play_sound AudioEventName - Play sound in 2D space.",
                RunCallbackAsync = WaitForAudio,
                Autocomplete = (parts) => {
                    if (parts.Length == 2) return _audioEventsByName.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
                    return Array.Empty<string>();
                }
            });
        }

        private void OnDisable() {
            if (Channel == null) return;
            Channel.UnregisterRegistrar(this);
        }

#if UNITY_EDITOR
        [ContextMenu("Populate References")]
        private void PopulateFromProject() {
            audioEvents = LoadAllAssetsOfType<AudioEvent>();

            RebuildDictionaries();
            EditorUtility.SetDirty(this);
            Debug.Log($"Populated: events={audioEvents.Length}");
        }

        private static T[] LoadAllAssetsOfType<T>() where T : UnityEngine.Object {
            string typeName = typeof(T).Name;
            var guids = AssetDatabase.FindAssets($"t:{typeName}");
            var assets = new List<T>(guids.Length);
            foreach (var guid in guids) {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath<T>(path);
                if (asset != null) assets.Add(asset);
            }
            return assets.ToArray();
        }
#endif
    }
}