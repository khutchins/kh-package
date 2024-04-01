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
        instance.RegisterHandler("read_args", (cmd) => {
            StringBuilder text = new StringBuilder();
            for (int i = 0; i < cmd.Length; i++) {
                text.Append($"{i}: {cmd[i]}\n");
            }
            return text.ToString();
        });

        // The cmd array's 0 position is the command itself. However, the Expect* commands account for this,
        // so to read the first positional argument using those, do ConsoleManager.ExpectString(cmd, 0). Generally
        // you shouldn't access them directly, as the Expect commands will handle array bounds and type coercion
        // for you. 
        instance.RegisterHandler("add_ints", (cmd) => {
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
        });

        instance.RegisterHandler("add_floats", (cmd) => {
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
        });

        // Feel free to throw exceptions here: it will be caught and the text will be shown in the console output.
        instance.RegisterHandler("throw", (cmd) => {
            throw new System.Exception("This message was the exception text!");
        });
    }
}