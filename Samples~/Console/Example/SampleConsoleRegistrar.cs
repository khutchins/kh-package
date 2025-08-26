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
            RunCallback = (invocation) => {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < invocation.ArgCount; i++) {
                    text.Append($"{i}: {invocation.ExpectString(i)}\n");
                }
                invocation.SetOutput(text.ToString());
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
            RunCallback = (invocation) => {
                int total = 0;
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < invocation.ArgCount; i++) {
                    int val = invocation.ExpectInt(i);
                    total += val;
                    if (i == 1) text.Append(val);
                    else text.Append($" + {val}");
                }
                text.Append($" = {total}");
                invocation.SetOutput(text.ToString());
            }
        });

        Channel.Register(new Command() {
            Name = "add_floats",
            Description = "Adds together all the floats provided and returns the result.",
            Registrar = this,
            RunCallback = (invocation) => {
                float total = 0;
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < invocation.ArgCount; i++) {
                    float val = invocation.ExpectFloat(i);
                    total += val;
                    if (i == 1) text.Append(val);
                    else text.Append($" + {val}");
                }
                text.Append($" = {total}");
                invocation.SetOutput(text.ToString());
            }
        });

        // Feel free to throw exceptions here: it will be caught and the text will be shown in the console output.
        Channel.Register(new Command() {
            Name = "throw",
            Description = "Throws a real exception.",
            Registrar = this,
            RunCallback = (invocation) => {
                throw new System.Exception("This message was the exception text!");
            }
        });

        IEnumerator AsyncThrow(Invocation invocation) {
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
            RunCallback = (Invocation invocation) => {
                StringBuilder text = new StringBuilder();
                for (int i = 0; i < invocation.ArgCount; i++) {
                    text.Append(invocation.ExpectString(i)).Append(' ');
                }
                invocation.SetOutput(text.ToString());
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