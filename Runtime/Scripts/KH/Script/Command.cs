using System.Collections.Generic;

namespace KH.Script {
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