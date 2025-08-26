using KH;
using KH.Audio;
using KH.Console;
using KH.Script;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

[DefaultExecutionOrder(-99)]
public class ScriptingEngine : MonoBehaviour {
    public static ScriptingEngine INSTANCE;
    [SerializeField] CommandChannel Channel;

    [Header("Output")]
    [Tooltip("Echo command output to Debug.Log.")]
    [SerializeField] bool LogToDebug = true;

    private ScriptRunner _runner = new ScriptRunner();

    private void Awake() {
        RegisterBuiltins();
        INSTANCE = this;
    }

    [KVBSCommand("wait", "Waits the given amount of time (scaled).")]
    [KVBSArg("float", "duration", "The time to wait in seconds.")]
    private static void _KVBSWaitDefinition() { }

    [KVBSCommand("waitrt", "Waits the given amount of time (unscaled).")]
    [KVBSArg("float", "duration", "The time to wait in seconds.")]
    private static void _KVBSWaitRTDefinition() { }

    [KVBSCommand("log", "Logs the messages to the console.")]
    [KVBSArg("string", "message", "The message to log", true)]
    private static void _KVBSLog() { }

    void RegisterBuiltins() {
        IEnumerator WaitForSeconds(float amt, bool realtime) {
            if (realtime) yield return new WaitForSecondsRealtime(amt);
            else yield return new WaitForSeconds(amt);
        }
        _runner.RegisterHandler(new Command {
            Name = "wait",
            Description = "wait <seconds> in scaled time.",
            RunCallbackAsync = (invocation) => WaitForSeconds(invocation.ExpectFloat(0), false)
        });

        _runner.RegisterHandler(new Command {
            Name = "waitrt",
            Description = "waitrt <seconds> in unscaled time.",
            RunCallbackAsync = (invocation) => WaitForSeconds(invocation.ExpectFloat(0), true)
        });

        _runner.RegisterHandler(new Command {
            Name = "log",
            Description = "Log messages.",
            RunCallback = (invocation) => {
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i < invocation.ArgCount; i++) {
                    stringBuilder.Append(invocation.ExpectString(0)).Append(" ");
                }
            }
        });
    }

    private void OnEnable() {
        if (Channel != null) {
            Channel.OnRegister += HandleRegister;
            Channel.OnUnregister += HandleUnregister;
            Channel.OnUnregisterRegistrar += HandleUnregisterRegistrar;
        }
    }

    private void OnDisable() {
        if (Channel != null) {
            Channel.OnRegister -= HandleRegister;
            Channel.OnUnregister -= HandleUnregister;
            Channel.OnUnregisterRegistrar -= HandleUnregisterRegistrar;
        }
    }

    private void HandleRegister(Command cmd) => _runner.RegisterHandler(cmd);
    private void HandleUnregister(Command cmd) => _runner.UnregisterHandler(cmd);
    private void HandleUnregisterRegistrar(object registrar) => _runner.UnregisterRegistrar(registrar);

    public void Run(string script) {
        StartCoroutine(RunAndAwait(script));
    }

    /// <summary>
    /// Coroutine that runs a newline-delimited script.
    /// </summary>
    public IEnumerator RunAndAwait(string script) {
        if (string.IsNullOrWhiteSpace(script)) yield break;

        var lines = script
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim())
            .Where(l => !string.IsNullOrEmpty(l))
            .ToList();

        int i = 0;
        while (i < lines.Count) {
            string raw = lines[i];

            // Background lines run immediately.
            if (IsBg(raw, out var bgCmd)) {
                StartCoroutine(RunSingleLine(bgCmd));
                i++;
                continue;
            }

            // Otherwise, build a concurrent group: current line + any following "and " lines.
            var group = new List<string>();
            string baseCmd;
            bool isAndFirst = raw.StartsWith("and ");

            if (isAndFirst) {
                // This is weird behavior, but I don't want to break if they start a group with "and".
                baseCmd = raw[4..].TrimStart();
            } else {
                baseCmd = raw;
            }
            group.Add(baseCmd);
            i++;

            // Consume subsequent "and " lines into the same group.
            while (i < lines.Count && lines[i].StartsWith("and ")) {
                string andCmd = lines[i][4..].TrimStart();
                // "and bg" case is illegal and not handled. This does mean that if the "bg"
                // command is registered, it will behave semantically differently here, but
                // I'm okay with that.
                group.Add(andCmd);
                i++;
            }

            // Run this group concurrently and wait until all in the group complete.
            if (group.Count == 0) {
                continue;
            } else if (group.Count == 1) {
                yield return RunSingleLine(group[0]);
            } else {
                var enumerators = new List<IEnumerator>(group.Count);
                foreach (var line in group) enumerators.Add(RunSingleLine(line));
                yield return CoroutineCoordinator.RunAll(this, enumerators);
            }
        }
    }

    private static bool IsBg(string raw, out string command) {
        command = null;
        if (!raw.StartsWith("bg ")) return false;
        // Keep the original text after 'bg ' to preserve user quoting exactly.
        command = raw.Substring(3).TrimStart();
        return !string.IsNullOrWhiteSpace(command);
    }


    /// <summary>
    /// Wraps ScriptRunner.ExecuteAsync so we can add a one-time 'cmd: line' header
    /// and route output to UnityEvent/Debug.Log.
    /// </summary>
    private IEnumerator RunSingleLine(string line) {
        bool firstOutput = true;
        // Note: ExecuteAsync handles both async and sync commands (fallback).
        yield return _runner.ExecuteAsync(
            line,
            setOutput: (result) => {
                // First callback: include a command header.
                string msg = firstOutput ? $"cmd: {line}\n{result ?? ""}" : (result ?? "");
                firstOutput = false;
                if (LogToDebug) Debug.Log(msg);
            }
        );
    }
}
