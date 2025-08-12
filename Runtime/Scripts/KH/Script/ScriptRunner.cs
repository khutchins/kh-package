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

        public void RegisterHandler(string command, System.Func<string[], string> callback) {
            RegisterHandler(new Command() {
                Name = command,
                RunCallback = callback,
            });
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

        /// <summary>
        /// Execute a command line and return the resulting output string.
        /// </summary>
        public string Execute(string str) {
            var cmd = CommandParser.ParseText(str).ToArray();
            if (cmd.Length < 1) {
                return "";
            }
            if (!_registeredCmds.TryGetValue(cmd[0], out var handler)) {
                return $"Unrecognized command: {cmd[0]}\nSee all commands with 'help'.";
            }
            try {
                return handler.RunCallback(cmd);
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
            var cmd = CommandParser.ParseText(str).ToArray();
            if (setOutput == null) setOutput = (string _) => { };
            if (cmd.Length < 1) { setOutput?.Invoke(""); yield break; }

            if (!_registeredCmds.TryGetValue(cmd[0], out var handler)) {
                setOutput?.Invoke($"Unrecognized command: {cmd[0]}\nSee all commands with 'help'.");
                yield break;
            }

            // Prefer async if provided
            if (handler.RunCallbackAsync != null) {
                yield return handler.RunCallbackAsync(cmd, setOutput);
                yield break;
            }

            // Fallback to sync path
            string result;
            try {
                result = handler.RunCallback != null ? handler.RunCallback(cmd) : "";
            } catch (Exception e) {
                result = e.Message;
            }
            setOutput?.Invoke(result);
        }

        public IEnumerable<string> GetCommandNames() => _registeredCmds.Keys;
        public bool TryGetCommand(string name, out Command command) => _registeredCmds.TryGetValue(name, out command);

        public static string ExpectString(string[] cmds, int idx) {
            return GetArg(cmds, idx);
        }

        public static int ExpectInt(string[] cmds, int idx) {
            var arg = GetArg(cmds, idx);
            if (int.TryParse(arg, out int result)) {
                return result;
            } else {
                throw new Exception($"Argument {idx} ({arg}) was expected to be an int, but it wasn't.");
            }
        }

        public static float ExpectFloat(string[] cmds, int idx) {
            var arg = GetArg(cmds, idx);
            try {
                return float.Parse(arg, CultureInfo.InvariantCulture);
            } catch (Exception) {
                throw new Exception($"Argument {idx} ({arg}) was expected to be a float, but it wasn't.");
            }
        }

        private static string GetArg(string[] cmds, int idx) {
            if (idx + 1 >= cmds.Length) {
                throw new Exception($"Expected at least {idx + 1} arguments.");
            }
            return cmds[idx + 1];
        }

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