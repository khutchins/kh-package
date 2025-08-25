using System;

namespace KH.Script {
    /// <summary>
    /// Defines a command's signature for the KVBS definition generator.
    /// Place this on a static method within the class that registers the command.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class KVBSCommandAttribute : Attribute {
        public string CommandName { get; }
        public object[] Arguments { get; }

        /// <summary>
        /// Declares a KVBS command signature.
        /// </summary>
        /// <param name="commandName">The name of the command.</param>
        /// <param name="arguments">
        /// A list of argument descriptors.
        /// - Use strings for basic types: "string", "int", "float", "bool".
        /// - Use typeof(YourAssetType) to generate a union of all asset names of that type.
        /// - Use a string literal like 'my_literal' for a specific value.
        /// </param>
        public KVBSCommandAttribute(string commandName, params object[] arguments) {
            CommandName = commandName;
            Arguments = arguments;
        }
    }
}