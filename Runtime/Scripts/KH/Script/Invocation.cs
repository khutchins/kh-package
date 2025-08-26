using KH.Console;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace KH.Script {
    public class Invocation {
        /// <summary>
        /// A reference to the ScriptRunner executing this command, for edge cases.
        /// </summary>
        public readonly ScriptRunner Runner;

        /// <summary>
        /// The parsed command-line arguments, with the command name at index 0.
        /// </summary>
        private readonly string[] Args;

        /// <summary>
        /// The output callback for asynchronous commands to log messages.
        /// </summary>
        public readonly Action<string> SetOutput;

        /// <summary>
        /// The command being run.
        /// </summary>
        public readonly Command Command;

        public Invocation(ScriptRunner runner, Command command, string[] args, Action<string> setOutput) {
            Runner = runner;
            Command = command;
            Args = args;
            SetOutput = setOutput;
        }

        public int ArgCount {
            get => Args.Length - 1;
        }

        public string ExpectString(int idx) {
            return GetArg(idx);
        }

        public int ExpectInt(int idx) {
            var arg = GetArg(idx);
            if (int.TryParse(arg, out int result)) {
                return result;
            } else {
                throw new Exception($"Argument {idx} ({arg}) was expected to be an int, but it wasn't.");
            }
        }

        public float ExpectFloat(int idx) {
            var arg = GetArg(idx);
            try {
                return float.Parse(arg, CultureInfo.InvariantCulture);
            } catch (Exception) {
                throw new Exception($"Argument {idx} ({arg}) was expected to be a float, but it wasn't.");
            }
        }

        private static readonly HashSet<string> BOOL_TRUE = new(StringComparer.OrdinalIgnoreCase) { "true", "on", "yes", "1" };

        private static readonly HashSet<string> BOOL_FALSE = new(StringComparer.OrdinalIgnoreCase) { "false", "off", "no", "0" };

        public bool ExpectBool(int idx) {
            var arg = GetArg(idx);
            if (BOOL_TRUE.Contains(arg)) return true;
            if (BOOL_FALSE.Contains(arg)) return false;
            throw new Exception($"Argument {idx} ({arg}) was expected to be a bool, but it wasn't.");
        }

        private string GetArg(int idx) {
            if (idx + 1 >= Args.Length) {
                throw new Exception($"Expected at least {idx + 1} arguments.");
            }
            return Args[idx + 1];
        }
    }
}