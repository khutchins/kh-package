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

        private Dictionary<string, System.Func<string[], string>> _registeredCmds = new Dictionary<string, System.Func<string[], string>>();
        private Trie _trie = new Trie();
        private bool _isUp = false;
        private Canvas _canvas;
        private string _currentText = "";

        private void Awake() {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            INSTANCE = this;
            RegisterHandler("getver", (string[] cmd) => {
                return Application.version;
            });

            RegisterHandler("help", (string[] cmd) => {
                string commands = string.Join(' ', _registeredCmds.Keys.OrderBy(a => a));
                return $"Available commands: {commands}";
            });
        }

        public void RegisterHandler(string command, System.Func<string[], string> callback) {
            if (_registeredCmds.ContainsKey(command)) {
                Debug.LogWarning($"Console already recognizes command '{command}'. Existing handler will be overwritten.");
            }
            _registeredCmds[command] = callback;
            _trie.Insert(command);
        }

        public void UnregisterHandler(string command) {
            _registeredCmds.Remove(command);
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
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab)) {
                // This key doesn't show up in inputString, for whatever reason, so it has
                // to be handled here.
                string[] tokens = ParseText(_currentText).ToArray();
                if (tokens.Length != 1) {
                    return;
                }
                string last = tokens[0];
                if (last == null) last = "";
                string shortestShared = _trie.GetShortestSharedPrefix(last);
                List<string> words = _trie.WordsWithPrefix(_currentText).OrderBy(x => x).ToList();
                if (words.Count > 1) {
                    SetCurrentText(shortestShared);
                    OutputText.text = string.Join(' ', words);
                } else {
                    SetCurrentText(shortestShared + " ");
                }
            }

            foreach (char c in UnityEngine.Input.inputString) {
                if (c == '\b') {
                    if (_currentText.Length != 0) {
                        SetCurrentText(_currentText[0..^1]);
                    }
                } else if (c == 127) { // Ctrl + Backspace, remove last word.
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
                    output += handler(cmd);
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

        private void SetCurrentText(string text) {
            InputText.text = $"> {text}";
            _currentText = text;
        }

        public void SetMenuUp(bool newUp) {
            _canvas.enabled = newUp;
            _isUp = newUp;
            if (!_isUp) {
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
                throw new System.Exception($"Argument {idx} ({arg}) was expected to be an float, but it wasn't.");
            }
        }

        private static string GetArg(string[] cmds, int idx) {
            if (idx + 1 >= cmds.Length) {
                throw new System.Exception($"Expected at least {idx} arguments.");
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

        private static IEnumerable<string> ParseText(string text) {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            char termChar = '\0';
            StringBuilder token = new StringBuilder();
            var context = ParseContext.Start;

            void Reset() {
                token.Clear();
                termChar = '\0';
                context = ParseContext.Start;
            }

            for (int i = 0; i < text.Length; i++) {
                char curr = text[i];

                if (context == ParseContext.Start) {
                    if (curr == '\'' || curr == '"') {
                        termChar = curr;
                        context = ParseContext.Standard;
                    } else if (char.IsWhiteSpace(curr)) {
                        // Eat character, as empty tokens are removed (unless double quoted.)
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
                        } else {
                            token.Append(curr);
                        }
                    }
                }
            }

            string remainder = token.ToString();
            if (!string.IsNullOrWhiteSpace(remainder)) {
                yield return remainder;
            }
        }
    }
}