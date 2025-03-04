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

        [Header("History")]
        [Tooltip("How many commands to store in the buffer.")]
        [SerializeField] int CommandHistory = 20;
        [Tooltip("If true, a command will not be added to the history if it exactly matches the one before.")]
        [SerializeField] bool OmitDuplicatesFromHistory = true;

        private Dictionary<string, Command> _registeredCmds = new Dictionary<string, Command>();
        private Trie _trie = new Trie();
        private bool _isUp = false;
        private Canvas _canvas;
        private string _currentText = "";
        private bool _blinkOn = false;

        /// Command Handling
        private FixedArray<string> _commandHistory;
        private int _historyIndex;
        private string _tempString;

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

        void Start() {
            _commandHistory = new FixedArray<string>(CommandHistory);
            _historyIndex = -1;
            _tempString = "";
        }

        public void RegisterHandler(Command command) {
            if (command.RunCallback == null || string.IsNullOrWhiteSpace(command.Name)) {
                Debug.LogWarning($"Commands must have a valid Name and RunCallback specified. This will not be added.");
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
            string[] tokens = CommandParser.ParseText(_currentText, true).ToArray();
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
            } else if (IsDown(KeyCode.DownArrow)) {
                UpdateCommandFromHistory(_historyIndex - 1);
            } else if (IsDown(KeyCode.UpArrow)) {
                UpdateCommandFromHistory(_historyIndex + 1);
            }
            return false;
        }

        private void UpdateCommandFromHistory(int idx) {
            _historyIndex = Mathf.Clamp(idx, -1, _commandHistory.Count - 1);
            if (_historyIndex < 0) {
                SetCurrentText(_tempString);
            } else {
                SetCurrentText(_commandHistory[_historyIndex]);
            }
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

            // We're not reading through the command buffer.
            if (_historyIndex == -1) {
                _tempString = _currentText;
            }
        }

        private string GetDebugOutput() {
            string[] cmd = CommandParser.ParseText(_currentText).ToArray();
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < cmd.Length; i++) {
                text.Append($"{i}: {cmd[i]}\n");
            }
            return text.ToString();
        }

        void HandleInput(string str) {
            string[] cmd = CommandParser.ParseText(str).ToArray();
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

            if (!OmitDuplicatesFromHistory || str != _commandHistory.Last) {
                _commandHistory.Add(str);
            }
            _historyIndex = -1;
            _tempString = "";
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
            if (_historyIndex == -1) {
                _tempString = text;
            }
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
                throw new System.Exception($"Expected at least {idx + 1} arguments.");
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

    public struct Command {
        /// <summary>
        /// Name of the command.
        /// </summary>
        public string Name;
        /// <summary>
        /// If true, does not autocomplete the base command. If autocomplete is provided,
        /// it will still perform autocompletion once the command is fully entered.
        /// </summary>
        public bool HideCommand;
        /// <summary>
        /// Description of the command. Optional.
        /// </summary>
        public string Description;
        /// <summary>
        /// The registrar responsible for the Command. Optional. Used only for bulk unregistering.
        /// </summary>
        public object Registrar;
        /// <summary>
        /// Callback for running a command. Takes in tokenized cmd invocation, with the first entry being
        /// the command name. Returns the text to be output on running the command.
        /// </summary>
        public System.Func<string[], string> RunCallback;
        /// <summary>
        /// Callback for supporting autocomplete. Optional. Takes in the current tokenization (with 0 being the cmd
        /// name and the final entry being the current partial string and returns an enumerable of all possible 
        /// autocomplete values for the command's current state. 
        /// 
        /// NOTE: You do not need to filter based on the partial final string, as the handler will do substring matching for you.
        /// NOTE 2: Base command name autocomplete will also be done for you.
        /// </summary>
        public System.Func<string[], IEnumerable<string>> Autocomplete;
    }
}