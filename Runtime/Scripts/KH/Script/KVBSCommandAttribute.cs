using System;
using System.Collections.Generic;
using System.Linq;

namespace KH.Script {
    public class Cmd {
        public readonly string Name;
        public readonly string Documentation;

        public Cmd(string name, string documentation = null) {
            Name = name;
            Documentation = documentation;
        }
    }

    public class Arg {
        public readonly string Name;
        public readonly object Type;
        public readonly string Documentation;
        public readonly bool IsVariadic;

        public Arg(string name, object type, string documentation = null, bool isVariadic = false) {
            Name = name;
            Type = type;
            Documentation = documentation;
            IsVariadic = isVariadic;
        }
    }

    /// <summary>
    /// Defines a KVBS command's name and documentation.
    /// This should be placed on a static method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class KVBSCommandAttribute : Attribute {
        public readonly string Name;
        public readonly string Documentation;

        public KVBSCommandAttribute(string name, string documentation) {
            Name = name;
            Documentation = documentation;
        }
    }

    /// <summary>
    /// Defines a single argument for a KVBS command.
    /// Stack this below a [KVBSCommand] attribute to add arguments in order.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
    public class KVBSArgAttribute : Attribute {
        public readonly object Type;
        public readonly string Name;
        public readonly string Documentation;
        public readonly bool IsVariadic;

        public KVBSArgAttribute(object type, string name, string documentation, bool isVariadic = false) {
            Type = type;
            Name = name;
            Documentation = documentation;
            IsVariadic = isVariadic;
        }
    }

    /// <summary>
    /// Defines a KVBS alias of all the assets of a ScriptableObject. Alias is named after
    /// the scriptable object class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum | AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class KVBSAliasAttribute : Attribute {
    }
}