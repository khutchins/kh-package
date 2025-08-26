using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Menutee;
using System.Globalization;
using System.Text;
using KH.Script;

namespace KH.Console {
    [DefaultExecutionOrder(-100)]
    [RequireComponent(typeof(Canvas))]
    public class ConsoleManager : MonoBehaviour, IMenu {
        public static ConsoleManager INSTANCE;
        [SerializeField] CommandChannel Channel;
        [SerializeField] TMP_Text InputText;
        [SerializeField] TMP_Text OutputText;
        [Tooltip("Whether ` + shift will toggle console and ESC will close it. Otherwise, do it yourself (it implements IMenu, so you can use that).")]
        [SerializeField] bool UseDefaultDisplayControls = true;

        [Header("History")]
        [Tooltip("How many commands to store in the buffer.")]
        [SerializeField] int CommandHistory = 20;
        [Tooltip("If true, a command will not be added to the history if it exactly matches the one before.")]
        [SerializeField] bool OmitDuplicatesFromHistory = true;

        private ScriptRunner _runner = new ScriptRunner();

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
            _runner.RegisterHandler(new Command() {
                Name = "getver",
                Description = "Gets the current build version.",
                RunCallback = (invocation) => {
                    invocation.SetOutput(Application.version);
                }
            });

            _runner.RegisterHandler(new Command() {
                Name = "help",
                Description = "Prints out the list of commands if no argument is given, prints out description if one is given. You know this, as you can only get this description by using it.",
                RunCallback = (invocation) => {
                    if (invocation.ArgCount == 0) {
                        string commands = string.Join(' ', _runner.GetCommandNames().OrderBy(a => a));
                        invocation.SetOutput($"Available commands: {commands}");
                    } else {
                        string cmdName = invocation.ExpectString(0);
                        if (_runner.TryGetCommand(cmdName, out Command value)) {
                            if (string.IsNullOrWhiteSpace(value.Description)) {
                                invocation.SetOutput($"No description for command: {value.Name}");
                            } else {
                                invocation.SetOutput($"{value.Name}: {value.Description}");
                            }
                        } else {
                            invocation.SetOutput($"No command with name: {cmdName}");
                        }
                    }
                },
                Autocomplete = (string[] cmd) => {
                    if (cmd.Length > 2) {
                        return new string[0];
                    } else {
                        return _runner.GetCommandNames().ToArray();
                    }
                }
            });
        }

        void Start() {
            _commandHistory = new FixedArray<string>(CommandHistory);
            _historyIndex = -1;
            _tempString = "";
        }

        void OnEnable() {
            if (Channel != null) {
                Channel.OnRegister += RegisterHandler;
                Channel.OnUnregister += UnregisterHandler;
                Channel.OnUnregisterRegistrar += UnregisterRegistrar;
            }
        }

        void OnDisable() {
            if (Channel != null) {
                Channel.OnRegister -= RegisterHandler;
                Channel.OnUnregister -= UnregisterHandler;
                Channel.OnUnregisterRegistrar -= UnregisterRegistrar;
            }
        }

        public void RegisterHandler(Command command) => _runner.RegisterHandler(command);
        public void UnregisterRegistrar(object registrar) => _runner.UnregisterRegistrar(registrar);
        public void UnregisterHandler(Command command) => _runner.UnregisterHandler(command);
        public void UnregisterHandler(string command) => _runner.UnregisterHandler(command);

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
            var (newText, output) = _runner.Autocomplete(_currentText);
            if (newText != null) {
                SetCurrentText(newText);
            }
            if (!string.IsNullOrEmpty(output)) {
                OutputText.text = output;
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

        void HandleInput(string str) {
            string execResult = _runner.RunOrStart(this, str);
            string output = string.IsNullOrEmpty(str) ? "" : $"cmd: {str}\n";
            output += execResult;
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