using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace KH.KVBDSL {
    public class Serializer {
        public class TypeHandler {
            public readonly Func<object, bool> Matches;
            public readonly Action<object, StringBuilder> Serialize;

            public TypeHandler(Func<object, bool> matches, Action<object, StringBuilder> serialize) {
                Matches = matches;
                Serialize = serialize;
            }
        }

        private static readonly TypeHandler IntHandler = new TypeHandler((object obj) => obj is int, (object obj, StringBuilder sb) => {
            sb.Append($"{Consts.TYPE_INT} ");
            sb.Append((int)obj);
        });

        private static readonly TypeHandler FloatHandler = new TypeHandler((object obj) => obj is float, (object obj, StringBuilder sb) => {
            sb.Append($"{Consts.TYPE_FLOAT} ");
            sb.Append(((float)obj).ToString(CultureInfo.InvariantCulture));
        });

        private static readonly TypeHandler BoolHandler = new TypeHandler((object obj) => obj is bool, (object obj, StringBuilder sb) => {
            sb.Append($"{Consts.TYPE_BOOL} ");
            sb.Append(((bool)obj) ? "true" : "false");
        });

        private static readonly TypeHandler RGBHandler = new TypeHandler((object obj) => obj is Color c && c.a == 1, (object obj, StringBuilder sb) => {
            string col = ColorUtility.ToHtmlStringRGB((Color)obj);
            sb.Append($"{Consts.TYPE_RGB} #{col}");
        });

        private static readonly TypeHandler RGBAHandler = new TypeHandler((object obj) => obj is Color c, (object obj, StringBuilder sb) => {
            string col = ColorUtility.ToHtmlStringRGBA((Color)obj);
            sb.Append($"{Consts.TYPE_RGBA} #{col}");
        });

        public static List<TypeHandler> Handlers = new List<TypeHandler> {
            IntHandler, FloatHandler, BoolHandler, RGBHandler, RGBAHandler
        };

        [Flags]
        public enum Options {
            None = 0,
            Indent = 0b0001,
            DisableMLS = 0b0010,
        }

        private Options _options;
        private int _indents;

        public Serializer(Options options = Options.None) {
            _options = options;
        }

        public string Serialize<T>(Dictionary<string, T> objToSerialize) {
            StringBuilder output = new StringBuilder();
            foreach (var entry in objToSerialize) {
                WriteEntry(output, entry.Key, entry.Value);
            }
            return output.ToString();
        }

        private void WriteEntry(StringBuilder sb, string key, object obj, int indent = 0) {
            if (!SupportedObject(obj)) {
                Debug.LogWarning($"Does not support serializing type {obj.GetType()} for key \"{key}\". Skipping");
                return;
            }

            AppendIndent(sb, indent);
            WriteKey(sb, key);
            sb.Append(": ");
            WriteValue(sb, obj, indent);
        }

        private void AppendIndent(StringBuilder sb, int count) {
            if ((_options & Options.Indent) > 0) {
                sb.Append(' ', count * 2);
            }
        }

        private void WriteKey(StringBuilder sb, string key) {
            if (ShouldUseQuotedStringKey(key)) {
                sb.Append(GetEscapedQuotedString(key));
            } else {
                sb.Append(key);
            }
        }

        private void WriteStringValue(StringBuilder sb, string value, int indent) {
            if (ShouldUseMLS(value)) {
                sb.Append(Consts.MLS_START);
                sb.Append('\n');
                AppendIndent(sb, indent + 1);
                foreach (var str in value.Split('\n')) {
                    if (str.Length > 0) {
                        AppendIndent(sb, indent + 1);
                        sb.Append(str.Replace("\\", "\\\\").Replace(Consts.MLS_START, "\\\"\\\"\\\""));
                        if (char.IsWhiteSpace(str[str.Length - 1])) sb.Append(Consts.WHITESPACE_PRESERVATION_ESCAPE);
                    }
                    sb.Append('\n');
                }
                AppendIndent(sb, indent + 1);
                sb.Append(Consts.MLS_START);
            } else if (ShouldUseQuotedString(value)) {
                sb.Append(GetEscapedQuotedString(value));
            } else {
                sb.Append("s ");
                sb.Append(value);
            }
        }

        private string GetEscapedQuotedString(string value) {
            StringBuilder sb = new StringBuilder(value);

            for (int i = 0; i < Consts.NORMAL_STRING_ESCAPES.Length; i++) {
                sb.Replace(Consts.NORMAL_STRING_ESCAPES[i].ToString(), $"\\{Consts.NORMAL_STRING_ESCAPES[i]}");
            }
            sb.Append(Consts.STR_START);
            sb.Insert(0, Consts.STR_START);
            return sb.ToString();
        }

        private bool ShouldUseMLS(string value) {
            return (_options & Options.DisableMLS) == 0 && value.Contains('\n');
        }

        private bool ShouldUseQuotedString(string value) {
            if (value.Length == 0) return true;
            if (value.StartsWith(Consts.STR_START)) return true;
            if (char.IsWhiteSpace(value[0]) || char.IsWhiteSpace(value[value.Length-1])) return true;
            foreach (var ch in Consts.UNQUOTED_STRING_TERM_CHARS) {
                if (value.Contains(ch)) return true;
            }
            return false;
        }

        private bool ShouldUseQuotedStringKey(string value) {
            if (value.Length == 0) return true;
            if (value.StartsWith(Consts.STR_START)) return true;
            foreach (var ch in Consts.UNQUOTED_KEY_STRING_TERM_CHARS) {
                if (value.Contains(ch)) return true;
            }
            return false;
        }

        private void WriteValue(StringBuilder sb, object value, int indent) {
            if (!SupportedObject(value)) {
                Debug.LogWarning($"Does not support serializing type {value.GetType()} in list. Skipping");
                return;
            }

            bool HandlerHandled() {
                foreach (TypeHandler handler in Handlers) {
                    if (handler.Matches(value)) {
                        handler.Serialize(value, sb);
                        return true;
                    }
                }
                return false;
            }

            if (!HandlerHandled()) {
                sb.Append(TypeStringForObject(value));

                if (value is string s) {
                    WriteStringValue(sb, s, indent);
                } else if (IsDictionary(value)) {
                    sb.Append(Consts.TYPE_DICT);
                    sb.Append('\n');
                    foreach (DictionaryEntry entry in value as IDictionary) {
                        WriteEntry(sb, entry.Key as string, entry.Value, indent + 1);
                    }
                    AppendIndent(sb, indent);
                    sb.Append(Consts.TYPE_DICT_END);
                } else if (IsList(value)) {
                    sb.Append(Consts.TYPE_ARRAY);
                    sb.Append('\n');
                    foreach (var entry in value as IList) {
                        AppendIndent(sb, indent + 1);
                        WriteValue(sb, entry, indent + 1);
                    }
                    AppendIndent(sb, indent);
                    sb.Append(Consts.TYPE_ARRAY_END);
                }
            }
            sb.Append("\n");
        }

        private string TypeStringForObject(object obj) {
            if (obj is string) return "";
            else if (obj is Color col && col.a == 1) return "rgb ";
            else if (obj is Color) return "rgba ";
            else if (IsDictionary(obj)) return "";
            else if (IsList(obj)) return "";
            // Shouldn't happen, output bad type.
            return "_ ";
        }

        private bool IsDictionary(object obj) {
            // Generic object of Dictionary type with first parameter as string.
            return obj.GetType().IsGenericType
                && obj.GetType().GetGenericTypeDefinition() == typeof(Dictionary<string, object>).GetGenericTypeDefinition()
                && obj.GetType().GetGenericArguments()[0] == typeof(string);
        }

        private bool IsList(object obj) {
            // Generic object of List type.
            return obj.GetType().IsGenericType
                && obj.GetType().GetGenericTypeDefinition() == typeof(List<object>).GetGenericTypeDefinition();
        }

        private bool IsColor(object obj) {
            return obj is Color;
        }

        private bool SupportedObject(object obj) {
            foreach (TypeHandler handler in Handlers) {
                if (handler.Matches(obj)) return true;
            }
            return obj is string || IsDictionary(obj) || IsList(obj);
        }
    }
}