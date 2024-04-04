using KH.Console;
using System.Text;
using UnityEngine;

public class SampleConsoleRegistrar : MonoBehaviour {
    private void Start() {
        var instance = ConsoleManager.INSTANCE;
        if (instance == null) {
            Debug.LogWarning($"Console does not exist. Cannot register.");
            return;
        }

        // Registration should generally happen on the same scope as the ConsoleManager itself.
        // If it's persistent, this should be persistent. If it isn't, this isn't.
        // However, if you clean up the handlers using UnregisterHandler, it's probably fine for
        // ConsoleManager to be persistent without persistent registrars. Not sure what the point
        // would be though.
        instance.RegisterHandler(new Command() {
            Name = "read_args",
            Description = "Reads all the arguments provided and shows how they're tokenized.",
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
        instance.RegisterHandler(new Command() {
            Name = "add_ints",
            Description = "Adds together all the integers provided and returns the result.",
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

        instance.RegisterHandler(new Command() {
            Name = "add_floats",
            Description = "Adds together all the floats provided and returns the result.",
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
        instance.RegisterHandler(new Command() {
            Name = "throw",
            Description = "Throws a real exception.",
            RunCallback = (string[] cmd) => {
                throw new System.Exception("This message was the exception text!");
            }
        });

        // You can do your own autocomplete.
        instance.RegisterHandler(new Command() {
            Name = "autocomplete",
            Description = "Example of using autocomplete.",
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
}