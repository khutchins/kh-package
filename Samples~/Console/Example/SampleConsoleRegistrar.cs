using KH.Console;
using KH.Script;
using System.Collections;
using System.Text;
using UnityEngine;

public class SampleConsoleRegistrar : MonoBehaviour {
    [SerializeField] CommandChannel Channel;

    private void OnEnable() {
        if (Channel == null) {
            Debug.LogWarning($"Console does not exist. Cannot register.");
            return;
        }

        // Registration should generally happen on the same scope as the ConsoleManager itself.
        // If it's persistent, this should be persistent. If it isn't, this isn't.
        // However, if you clean up the handlers using UnregisterHandler, it's probably fine for
        // ConsoleManager to be persistent without persistent registrars. Not sure what the point
        // would be though.
        Channel.Register(new Command() {
            Name = "read_args",
            Description = "Reads all the arguments provided and shows how they're tokenized.",
            Registrar = this,
            RunCallback = (string[] cmd) => {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < cmd.Length; i++) {
                    text.Append($"{i}: {cmd[i]}\n");
                }
                return text.ToString();
            }
        });

        // The cmd array's 0 position is the command itself. However, the Expect* commands account for this,
        // so to read the first positional argument using those, do ConsoleManager.ExpectString(cmd, 0). Generally
        // you shouldn't access them directly, as the Expect commands will handle array bounds and type coercion
        // for you. 
        Channel.Register(new Command() {
            Name = "add_ints",
            Description = "Adds together all the integers provided and returns the result.",
            Registrar = this,
            RunCallback = (string[] cmd) => {
                int total = 0;
                StringBuilder text = new StringBuilder();
                for (int i = 1; i < cmd.Length; i++) {
                    int val = ConsoleManager.ExpectInt(cmd, i - 1);
                    total += val;
                    if (i == 1) text.Append(val);
                    else text.Append($" + {val}");
                }
                text.Append($" = {total}");
                return text.ToString();
            }
        });

        Channel.Register(new Command() {
            Name = "add_floats",
            Description = "Adds together all the floats provided and returns the result.",
            Registrar = this,
            RunCallback = (string[] cmd) => {
                float total = 0;
                StringBuilder text = new StringBuilder();
                for (int i = 1; i < cmd.Length; i++) {
                    float val = ConsoleManager.ExpectFloat(cmd, i - 1);
                    total += val;
                    if (i == 1) text.Append(val);
                    else text.Append($" + {val}");
                }
                text.Append($" = {total}");
                return text.ToString();
            }
        });

        // Feel free to throw exceptions here: it will be caught and the text will be shown in the console output.
        Channel.Register(new Command() {
            Name = "throw",
            Description = "Throws a real exception.",
            Registrar = this,
            RunCallback = (string[] cmd) => {
                throw new System.Exception("This message was the exception text!");
            }
        });

        IEnumerator AsyncThrow(string[] cmd, System.Action<string> _) {
            yield return null;
            throw new System.Exception("This message will not be seen in the console because it's async!");
        }

        Channel.Register(new Command() {
            Name = "throw_async",
            Description = "Throws a real exception.",
            Registrar = this,
            RunCallbackAsync = AsyncThrow,
        });

        // You can do your own autocomplete.
        Channel.Register(new Command() {
            Name = "autocomplete",
            Description = "Example of using autocomplete.",
            Registrar = this,
            RunCallback = (string[] cmd) => {
                return string.Join(' ', cmd, 1, cmd.Length - 1);
            },
            Autocomplete = (string[] cmd) => {
                // Remember cmd[0] is the command name (useful if you want to share these).
                // You'll never actually get in here if cmd.Length < 2, but it's worth checking
                // anyway.
                int count = cmd.Length;
                if (count == 2) return new string[] { "one", "two", "three", "not a number" };
                if (count == 3) return new string[] { "four", "five", "six" };
                if (count == 4) return new string[] { "seven", "eight", "nine" };
                return null;
            },
        });
    }

    private void OnDisable() {
        ConsoleManager.INSTANCE.UnregisterRegistrar(this);
    }
}