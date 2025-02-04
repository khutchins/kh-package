using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.KVBDSL {
    public static class Consts {
        public const string TYPE_STRING = "s";
        public const string TYPE_INT = "i";
        public const string TYPE_FLOAT = "f";
        public const string TYPE_BOOL = "b";
        public const string TYPE_RGB = "_rgb";
        public const string TYPE_RGBA = "_rgba";
        public const string TYPE_ARRAY = "[";
        public const char TYPE_ARRAY_END = ']';
        public const string TYPE_DICT = "{";
        public const char TYPE_DICT_END = '}';
        public const string MLS_START = "\"\"\"";
        public const string STR_START = "\"";

        public const char ESCAPE_CHAR = '\\';
        public static readonly char[] QUOTED_STRING_TERM_CHARS = new char[] { '"', '\n' };
        public static readonly char[] UNQUOTED_KEY_STRING_TERM_CHARS = new char[] { '\n', ':' };
        public static readonly char[] UNQUOTED_STRING_TERM_CHARS = new char[] { '\n' };
        // It is important for the \ to remain at the start of both of these.
        public static readonly char[] NORMAL_ESCAPE_CHARS = new char[] { '\\', '\"', 'b', 'f', 'n', 'r', 't', 'v', };
        public static readonly char[] NORMAL_STRING_ESCAPES = new char[] { '\\', '\"', '\b', '\f', '\n', '\r', '\t', '\v'};
        public static readonly char[] UNQUOTED_ESCAPE_CHARS = new char[] { };
        public static readonly char[] UNQUOTED_STRING_ESCAPES = new char[] { };
        public static readonly string WHITESPACE_PRESERVATION_ESCAPE = "\\p";
    }
}