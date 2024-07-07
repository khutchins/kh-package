using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Menutee;
using System.Globalization;
using System.Text;

namespace KH.Console {
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Canvas))]
    public class ConsoleManager : MonoBehaviour, IMenu {
        public static ConsoleManager INSTANCE;
        [SerializeField] TMP_Text InputText;
        [SerializeField] TMP_Text OutputText;
        [Tooltip("Whether ` + shift will toggle console and ESC will close it. Otherwise, do it yourself (it implements IMenu, so you can use that).")]
        [SerializeField] bool UseDefaultDisplayControls = true;
        [SerializeField] int CommandHistory = 20;

        private Dictionary<string, Command> _registeredCmds = new Dictionary<string, Command>();
        private Trie _trie = new Trie();
        private bool _isUp = false;
        private Canvas _canvas;
        private string _currentText = "";
        private bool _blinkOn = false;

        private SingleCoroutineManager _blinkCoroutine;

        private void Awake() {
            _canvas = GetComponent<Canvas>();
            _blinkCoroutine = new SingleCoroutineManager(this);
            _canvas.enabled = false;
            INSTANCE = this;
            RegisterHandler(new Command() {
                Name = "getver",
                Description = "Gets the current build version.",
                RunCallback = (string[] cmd) => {
                    return Application.version;
                }
            });

            RegisterHandler(new Command() {
                Name = "help",
                Description = "Prints out the list of commands if no argument is given, prints out description if one is given. You know this, as you can only get this description by using it.",
                RunCallback = (string[] cmd) => {
                    if (cmd.Length <= 1) {
                        string commands = string.Join(' ', _registeredCmds.Keys.OrderBy(a => a));
                        return $"Available commands: {commands}";
                    } else {
                        if (_registeredCmds.TryGetValue(cmd[1], out Command value)) {
                            if (string.IsNullOrWhiteSpace(value.Description)) {
                                return $"No description for command: {value.Name}";
                            } else {
                                return $"{value.Name}: {value.Description}";
                            }
                        } else {
                            return $"No command with name: {cmd[0]}";
                        }
                    }
                },
                Autocomplete = (string[] cmd) => {
                    if (cmd.Length > 2) {
                        return new string[0];
                    } else {
                        return _registeredCmds.Keys.ToArray();
                    }
                }
            });
        }

        public void RegisterHandler(Command command) {
            if (command.RunCallback == null || string.IsNullOrWhiteSpace(command.Name)) {
                Debug.LogWarning($"Commands must have a valid Name and RunCallback specified. This will not be added.");
                return;
            }
            if (_registeredCmds.ContainsKey(command.Name)) {
                Debug.LogWarning($"Console already recognizes command '{command}'. Existing handler will be overwritten.");
            }
            _registeredCmds[command.Name] = command;
            _trie.Insert(command.Name);

        }

        public void RegisterHandler(string command, System.Func<string[], string> callback) {
            RegisterHandler(new Command() {
                Name = command,
                RunCallback = callback,
            });
        }

        public void UnregisterHandler(Command command) {
            UnregisterHandler(command.Name);
        }

        public void UnregisterHandler(string command) {
            _registeredCmds.Remove(command);
        }

        private bool IsCtrlDown(params KeyCode[] keys) {
            if (!(UnityEngine.Input.GetKey(KeyCode.LeftControl) || UnityEngine.Input.GetKey(KeyCode.RightControl))) {
                return false;
            }
            foreach (KeyCode key in keys) {
                if (!UnityEngine.Input.GetKeyDown(key)) return false;
            }
            return true;
        }

        private bool IsDown(params KeyCode[] keys) {
            foreach (KeyCode key in keys) {
                if (!UnityEngine.Input.GetKeyDown(key)) return false;
            }
            return true;
        }

        private void HandleAutocomplete() {
            string[] tokens = ParseText(_currentText, true).ToArray();
            if (tokens.Length < 1) {
                return;
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
                    SetCurrentText(prior + EscapeStringIfNecessary(shortestShared, false));
                    OutputText.text = string.Join(' ', words);
                } else {
                    SetCurrentText(prior + EscapeStringIfNecessary(shortestShared, true) + " ");
                }
            } else {
                OutputText.text = "No valid autocompletions.";
            }
        }

        /// <summary>
        /// Handles keyboard input that doesn't appear in the inputString, like Tab and Ctrl.
        /// Notably, delete and backspace do not need to be handled here.
        /// </summary>
        /// <returns></returns>
        private bool HandleSpecialInputControls() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab)) {
                // Tab doesn't show up in inputString, for whatever reason, so it has
                // to be handled here.
                HandleAutocomplete();
                return true;
            } else if (IsCtrlDown(KeyCode.V)) {
                SetCurrentText(_currentText + GUIUtility.systemCopyBuffer);
                return true;
            }
            return false;
        }

        private void HandleDefaultInputControls() {
            foreach (char c in UnityEngine.Input.inputString) {
                if (c == '\b') { // backspace
                    if (_currentText.Length != 0) {
                        SetCurrentText(_currentText[0..^1]);
                    }
                } else if (c == 0x7F) { // Ctrl + Backspace, remove last word.
                    if (_currentText.Length != 0) {
                        string[] elements = _currentText.Trim().Split(' ');
                        if (elements.Length > 0) {
                            SetCurrentText(string.Join(' ', elements, 0, elements.Length - 1));
                        }
                    }
                } else if (c == '\n' || c == '\r') {
                    HandleInput(_currentText);
                    SetCurrentText("");
                } else {
                    SetCurrentText(_currentText + c);
                }
            }
        }

        private void Update() {
            if (UseDefaultDisplayControls) {
                if (UnityEngine.Input.GetKeyDown(KeyCode.BackQuote) && (UnityEngine.Input.GetKey(KeyCode.LeftShift) || UnityEngine.Input.GetKey(KeyCode.RightShift))) {
                    MenuStack.Shared.ToggleMenu(this);
                    return;
                }
            }
            if (!_isUp) return;
            if (UseDefaultDisplayControls) {
                if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
                    MenuStack.Shared.PopAndCloseMenu(this);
                    return;
                }
            }

            if (!HandleSpecialInputControls()) {
                HandleDefaultInputControls();
            }
        }

        private string GetDebugOutput() {
            string[] cmd = ParseText(_currentText).ToArray();
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < cmd.Length; i++) {
                text.Append($"{i}: {cmd[i]}\n");
            }
            return text.ToString();
        }

        void HandleInput(string str) {
            string[] cmd = ParseText(str).ToArray();
            string output = $"cmd: {str}\n";
            if (cmd.Length < 1) {
                output = "";
            } else if (!_registeredCmds.ContainsKey(cmd[0])) {
                output += $"Unrecognized command: {cmd[0]}\n";
                output += "See all commands with 'help'.";
            } else {
                var handler = _registeredCmds[cmd[0]];
                try {
                    output += handler.RunCallback(cmd);
                } catch (System.Exception e) {
                    output += e.Message;
                }
            }
            OutputText.text = output;
        }

        public MenuAttributes GetMenuAttributes() {
            var attrs = MenuAttributes.StandardPauseMenu();
            attrs.cursorLockMode = CursorLockMode.None;
            attrs.timeScale = 0;
            return attrs;
        }

        private void SetCurrentText(string text, bool blink = true) {
            InputText.text = $"> {text}{(blink ? "_" : "")}";
            _currentText = text;
            RefreshText();
        }

        private void SetBlink(bool blink) {
            _blinkOn = blink;
            RefreshText();
        }

        private void RefreshText() {
            InputText.text = $"> {_currentText}{(_blinkOn ? "_" : "")}";
        }

        private IEnumerator BlinkCoroutine() {
            while (true) {
                SetBlink(!_blinkOn);
                yield return new WaitForSecondsRealtime(0.75f);
            }
        }

        public void SetMenuUp(bool newUp) {
            _canvas.enabled = newUp;
            _isUp = newUp;
            if (_isUp) {
                _blinkCoroutine.StartCoroutine(BlinkCoroutine());
            }
            if (!_isUp) {
                _blinkCoroutine.StopCoroutine();
                _blinkOn = false;
                OutputText.text = "";
                SetCurrentText("");
            }
        }

        public void SetMenuOnTop(bool isTop) {
        }

        public static string ExpectString(string[] cmds, int idx) {
            return GetArg(cmds, idx);
        }

        public static int ExpectInt(string[] cmds, int idx) {
            var arg = GetArg(cmds, idx);
            if (int.TryParse(arg, out int result)) {
                return result;
            } else {
                throw new System.Exception($"Argument {idx} ({arg}) was expected to be an int, but it wasn't.");
            }
        }

        public static float ExpectFloat(string[] cmds, int idx) {
            var arg = GetArg(cmds, idx);
            try {
                return float.Parse(arg, CultureInfo.InvariantCulture);
            } catch (System.Exception) {
                throw new System.Exception($"Argument {idx} ({arg}) was expected to be a float, but it wasn't.");
            }
        }

        private static string GetArg(string[] cmds, int idx) {
            if (idx + 1 >= cmds.Length) {
                throw new System.Exception($"Expected at least {idx+1} arguments.");
            }
            return cmds[idx + 1];
        }

        enum ParseContext {
            Standard,
            Start,
            InEscape
        }
        enum TermChar {
            Whitespace,
            SingleQuotes,
            DoubleQuotes
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

        public static IEnumerable<string> ParseText(string text, bool includeTrailingWhitespace = false) {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            char termChar = '\0';
            StringBuilder token = new StringBuilder();
            var context = ParseContext.Start;
            bool hadWhitespace = false;

            void Reset() {
                token.Clear();
                termChar = '\0';
                context = ParseContext.Start;
                hadWhitespace = false;
            }

            for (int i = 0; i < text.Length; i++) {
                char curr = text[i];

                if (context == ParseContext.Start) {
                    if (curr == '\'' || curr == '"') {
                        termChar = curr;
                        context = ParseContext.Standard;
                    } else if (char.IsWhiteSpace(curr)) {
                        // Eat character, as empty tokens are removed (unless double quoted.)
                        hadWhitespace = true;
                        context = ParseContext.Start;
                    } else {
                        // Replay character in standard context, since we know an escape isn't relevant.
                        i--;
                        context = ParseContext.Standard;
                    }
                } else if (context == ParseContext.InEscape) {
                    if (curr == '\\') {
                        token.Append(curr);
                    } else if (termChar != '\0' && curr == termChar) {
                        token.Append(termChar);
                    } else {
                        Debug.LogWarning($"Unexpected escape character: '{curr}'. Assuming that escaping was not intended. Don't do this, as escape characters can be added.");
                        token.Append('\\');
                        token.Append(curr);
                    }
                    context = ParseContext.Standard;
                } else {
                    // String started with " or ', putting it in complex mode.
                    if (termChar != '\0') {
                        if (curr == '\\') {
                            context = ParseContext.InEscape;
                        } else if (curr == termChar) {
                            yield return token.ToString();
                            Reset();
                        } else {
                            token.Append(curr);
                        }
                    } else { // Just a normal string, terminated by whitespace.
                        if (char.IsWhiteSpace(curr)) {
                            yield return token.ToString();
                            Reset();
                            hadWhitespace = true;
                        } else {
                            token.Append(curr);
                        }
                    }
                }
            }

            string remainder = token.ToString();
            if (!string.IsNullOrWhiteSpace(remainder)) {
                yield return remainder;
            } else if (hadWhitespace && includeTrailingWhitespace) {
                yield return "";
            }
        }
    }

    public struct Command {
        public string Name;
        public string Description;
        public System.Func<string[], string> RunCallback;
        public System.Func<string[], IEnumerable<string>> Autocomplete;
    }
}