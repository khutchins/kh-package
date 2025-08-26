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
        [SerializeField] LineSpecQueueRegistry Registry;

        [KVBSCommand("say_text", "Plays a text line to the given queue.")]
        [KVBSArg(typeof(LineSpecQueue), "queue", "The queue to enqueue the line into.")]
        [KVBSArg("string", "line", "The line to enqueue.")]
        private static void _KVBSDefinition() { }

        private void OnEnable() {
            if (Channel == null) return;
            if (Registry == null) return;

            IEnumerator WaitForLine(string[] argv, Action<string> logger) {
                string name = ScriptRunner.ExpectString(argv, 0);
                var queue = Registry.GetItem(name);
                if (queue == null) {
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