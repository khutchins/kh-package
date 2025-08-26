using KH.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace KH.Console {
    public class ScriptRunner {

        private Dictionary<string, Command> _registeredCmds = new Dictionary<string, Command>();
        private Trie _trie = new Trie();

        public void RegisterHandler(Command command) {
            if ((command.RunCallback == null && command.RunCallbackAsync == null) || string.IsNullOrWhiteSpace(command.Name)) {
                Debug.LogWarning($"Commands must have a valid Name and RunCallback or RunCallbackAsync specified. This will not be added.");
                return;
            }
            if (_registeredCmds.ContainsKey(command.Name)) {
                Debug.LogWarning($"Console already recognizes command '{command.Name}'. Existing handler will be overwritten.");
            }
            _registeredCmds[command.Name] = command;
            if (!command.HideCommand) {
                _trie.Insert(command.Name);
            }

        }

        public void UnregisterRegistrar(object registrar) {
            foreach (Command cmd in _registeredCmds.Values.Where(x => x.Registrar == registrar).ToList()) {
                UnregisterHandler(cmd);
            }
        }

        public void UnregisterHandler(Command command) {
            UnregisterHandler(command.Name);
        }

        public void UnregisterHandler(string command) {
            _registeredCmds.Remove(command);
            _trie.Remove(command);
        }

        public (string prefix, string output) Autocomplete(string text) {
            string[] tokens = CommandParser.ParseText(text, true).ToArray();
            if (tokens.Length < 1) {
                tokens = new string[] { "" };
            }

            string last = tokens[^1];
            if (last == null) last = "";
            Trie trie = null;
            if (tokens.Length == 1) {
                trie = _trie;
            } else {
                string cmd = tokens[0];
                if (!string.IsNullOrWhiteSpace(cmd) && _registeredCmds.ContainsKey(cmd)) {
                    Command command = _registeredCmds[cmd];
                    if (command.Autocomplete != null) {
                        List<string> autocomplete = command.Autocomplete(tokens)?.ToList();
                        if (autocomplete != null && autocomplete.Count > 0) {
                            trie = new Trie();
                            foreach (string val in autocomplete) {
                                trie.Insert(val);
                            }
                        }
                    }
                }
            }
            if (trie != null) {
                string shortestShared = trie.GetShortestSharedPrefix(last);
                List<string> words = trie.WordsWithPrefix(last).OrderBy(x => x).ToList();
                string prior = tokens.Length > 1 ? string.Join(' ', tokens.Select(x => EscapeStringIfNecessary(x)).ToArray(), 0, tokens.Length - 1) + " " : "";
                if (words.Count > 1) {
                    return ((prior + EscapeStringIfNecessary(shortestShared, false), string.Join(' ', words)));
                } else {
                    return (prior + EscapeStringIfNecessary(shortestShared, true) + " ", null);
                }
            } else {
                return (null, "No valid autocompletions.");
            }
        }

        private static bool IsComment(string str) {
            return str.TrimStart().StartsWith("#");
        }

        private Invocation CreateInvocation(string commandString, Action<string> setOutput = null) {
            setOutput ??= (string _) => { };
            if (IsComment(commandString)) return null;
            var cmd = CommandParser.ParseText(commandString).ToArray();
            if (cmd.Length < 1) {
                return null;
            }

            if (!_registeredCmds.TryGetValue(cmd[0], out var handler)) {
                setOutput($"Unrecognized command: {cmd[0]}\nSee all commands with 'help'.");
                return null;
            }
            return new Invocation(this, handler, cmd, setOutput);
        }

        public string RunOrStart(MonoBehaviour runner, string str) {
            string output = "";
            var invocation = CreateInvocation(str, (str) => { output += str + "\n"; });
            if (invocation == null) return output;
            try {
                if (invocation.Command.RunCallbackAsync != null) {
                    // Don't wait for this to finish, as they're often dependent on in-game actions, which could cause a softlock.
                    runner.StartCoroutine(invocation.Command.RunCallbackAsync(invocation));
                    return "Running asynchronous command.";
                }
                invocation.Command.RunCallback(invocation);
                return output;
            } catch (System.Exception e) {
                return e.Message;
            }
        }

        /// <summary>
        /// Execute a line using a coroutine-friendly path.
        /// If the command has RunCallbackAsync, we yield it and let it push updates via setOutput.
        /// Otherwise, we run the sync callback and set the output once.
        /// </summary>
        public IEnumerator ExecuteAsync(string str, Action<string> setOutput) {
            var invocation = CreateInvocation(str, setOutput);
            if (invocation == null) yield break;

            // Prefer async if provided
            if (invocation.Command.RunCallbackAsync != null) {
                yield return invocation.Command.RunCallbackAsync(invocation);
                yield break;
            }

            // Fallback to sync path
            try {
                invocation.Command.RunCallback?.Invoke(invocation);
            } catch (Exception) {
            }
        }

        public IEnumerable<string> GetCommandNames() => _registeredCmds.Keys;
        public bool TryGetCommand(string name, out Command command) => _registeredCmds.TryGetValue(name, out command);

        public static string EscapeStringIfNecessary(string text, bool addTerminatingCharacter = true) {
            if (!text.Any(char.IsWhiteSpace)) {
                return text;
            }
            bool hasQuotes = text.Contains('"');
            bool hasSingleQuotes = text.Contains('\'');
            if (hasQuotes && !hasSingleQuotes) {
                // Escape \
                return $"'{text.Replace("\\", "\\\\")}{(addTerminatingCharacter ? "'" : "")}";
            } else if (hasQuotes) {
                // Escape \ and "
                return $"\"{text.Replace("\\", "\\\\").Replace("\"", "\\\"")}{(addTerminatingCharacter ? '"' : "")}";
            } else {
                // No escape necessary
                return $"\"{text}{(addTerminatingCharacter ? '"' : "")}";
            }
        }
    }
}