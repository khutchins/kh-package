using KH.Console;
using KH.Script;
using KH.Texts;
using Ratferences;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace KH.Script {
    public class ScriptTextInvoker : MonoBehaviour {
        [SerializeField] CommandChannel Channel;
        [SerializeField] LineSpecQueue[] textQueues = Array.Empty<LineSpecQueue>();
        private Dictionary<string, LineSpecQueue> _textQueuesByName;

        private void Awake() {
            RebuildDictionaries();
        }

        private void RebuildDictionaries() {
            _textQueuesByName = (textQueues ?? Array.Empty<LineSpecQueue>()).Where(x => x != null).GroupBy(x => x.name, StringComparer.OrdinalIgnoreCase).ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
        }

        private void OnEnable() {
            if (Channel == null) return;

            IEnumerator WaitForLine(string[] argv, Action<string> logger) {
                string name = ScriptRunner.ExpectString(argv, 0);
                if (!_textQueuesByName.TryGetValue(name, out var queue)) {
                    logger($"Unknown text queue '{name}'");
                    yield break;
                }
                string line = ScriptRunner.ExpectString(argv, 1);
                yield return queue.EnqueueAndAwait(new LineSpec("", line));
            }

            Channel.Register(new Command {
                Registrar = this,
                Name = "say_text",
                Description = "say_text QueueName <string> - Write line to text queue.",
                RunCallbackAsync = WaitForLine,
                Autocomplete = (parts) => {
                    if (parts.Length == 2) return _textQueuesByName.Keys.OrderBy(k => k, StringComparer.OrdinalIgnoreCase);
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
            textQueues = LoadAllAssetsOfType<LineSpecQueue>();

            RebuildDictionaries();
            EditorUtility.SetDirty(this);
            Debug.Log($"Populated: queues={textQueues.Length}");
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