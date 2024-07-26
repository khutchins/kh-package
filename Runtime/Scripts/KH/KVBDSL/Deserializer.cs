using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KH.KVBDSL {
    public class Deserializer {
        private enum ParseState {
            Default,
            InTextString,
            InTextBlock,
            SkippingArray,
            SkippingDict,
        };

        private enum StackType {
            None,
            Array,
            Dictionary
        }

        private const string TYPE_STRING = "s";
        private const string TYPE_INT = "i";
        private const string TYPE_FLOAT = "f";
        private const string TYPE_BOOL = "b";
        private const string TYPE_ARRAY = "[";
        private const string TYPE_DICT = "{";
        private const string MLS_START = "\"\"\"";
        private const string STR_START = "\"";

        public static Dictionary<string, object> ParseString(string input) {
            ParseState state = ParseState.Default;
            Stack<StackType> stack = new Stack<StackType>();
            Dictionary<string, object> output = new Dictionary<string, object>();

            int curr = 0;

            for (; curr < input.Length; ++curr) {
                if (state == ParseState.Default) {
                    curr = ReadToNextNonWhitespace(input, curr);
                    int lineStart = curr;

                    // Comment
                    if (input[curr] == '#') {
                        curr = ReadToNextLine(input, curr);
                        continue;
                    }
                    curr = ReadKey(input, curr, out string key);
                    if (key == null) {
                        curr = ReadToNextLineAndOutputError(input, lineStart, "Improperly formatted line. Should be comment or kvp");
                        continue;
                    }
                    curr = ReadToNextNonWhitespace(input, curr);
                    if (input.Length <= curr) {
                        curr = ReadToNextLineAndOutputError(input, lineStart, "Unexpected end of file");
                        continue;
                    }
                    if (input[curr] != ':') {
                        curr = ReadToNextLineAndOutputError(input, lineStart, "Expected ':' to end key, but other char found");
                        continue;
                    }
                    // Move past key separator.
                    ++curr;

                    curr = ReadToNextEndOfWord(input, curr, out string type);
                    if (type == null) {
                        curr = ReadToNextLineAndOutputError(input, lineStart, $"No type provided");
                        continue;
                    }
                    switch (type) {
                        case STR_START:
                            curr -= STR_START.Length;
                            curr = ReadString(input, curr, out string str);
                            output[key] = str;
                            break;
                        case MLS_START:
                            curr -= MLS_START.Length;
                            curr = ReadString(input, curr, out string strMLS);
                            output[key] = strMLS;
                            break;
                        case TYPE_STRING:
                            curr = ReadString(input, curr, out string strUnquote);
                            output[key] = strUnquote;
                            break;
                        case TYPE_BOOL: {
                                if (!bool.TryParse(ReadToEndOfLine(input, curr), out bool result)) {
                                    curr = ReadToNextLineAndOutputError(input, curr, "Invalid boolean format");
                                    continue;
                                }
                                output[key] = result;
                                break;
                            }
                        case TYPE_FLOAT: {
                                if (!float.TryParse(ReadToEndOfLine(input, curr), NumberStyles.Float, CultureInfo.InvariantCulture, out float result)) {
                                    curr = ReadToNextLineAndOutputError(input, curr, "Invalid float format");
                                    continue;
                                }
                                output[key] = result;
                                break;
                            }
                        case TYPE_INT: {
                                if (!int.TryParse(ReadToEndOfLine(input, curr), NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)) {
                                    curr = ReadToNextLineAndOutputError(input, curr, "Invalid int format");
                                    continue;
                                }
                                output[key] = result;
                                break;
                            }
                        case TYPE_ARRAY:
                            if (HasMoreContent(input, curr)) {
                                curr = GetCurrentLine(input, lineStart, out string line);
                                Debug.LogWarning($"Array has content beyond '['. Skipping array starting at '{line}'.");
                                state = ParseState.SkippingArray;
                                continue;
                            }
                            stack.Push(StackType.Array);
                            break;
                        case TYPE_DICT:
                            if (HasMoreContent(input, curr)) {
                                curr = GetCurrentLine(input, lineStart, out string line);
                                Debug.LogWarning($"Dict has content beyond '{{'. Skipping dict starting at '{line}'.");
                                state = ParseState.SkippingDict;
                                continue;
                            }
                            stack.Push(StackType.Dictionary);
                            break;
                        default:
                            curr = ReadToNextLineAndOutputError(input, curr, $"Unrecognized type '{type}'");
                            continue;
                    }
                } else if (state == ParseState.SkippingArray) {
                    // TODO:
                } else if (state == ParseState.SkippingDict) {
                    // TODO:
                }
            }
            return output;
        }

        private static int ReadToNextLineAndOutputError(string input, int lineStart, string msg) {
            int endIdx = GetCurrentLine(input, lineStart, out string line);
            Debug.LogWarning($"{msg}. Skipping line: '{line}'.");
            return endIdx;
        }

        private static int GetCurrentLine(string input, int lineStart, out string line) {
            int endIdx = input.IndexOf('\n', lineStart);
            if (endIdx < 0) endIdx = input.Length;
            line = input.Substring(lineStart, endIdx - lineStart);
            return endIdx;
        }

        private static bool HasMoreContent(string input, int start) {
            return input.Substring(start).Trim().Length > 0;
        }

        private static int ReadKey(string input, int start, out string key) {
            start = ReadToNextNonWhitespace(input, start);
            if (input.Length <= start) {
                key = null;
                return start;
            }
            if (input[start] == '"') {
                // This will end before or at the ':', so we should stay here.
                return ReadQuotedString(input, start, out key);
            } else {
                // This will one char past the :, so we need to move back.
                int curr = ReadUnquotedKeyString(input, start, out key);
                --curr;
                return curr;
            }
        }

        private static int ReadToNextLine(string input, int start) {
            while (input.Length > start && input[start] != '\n') ++start;
            return start;
        }

        private static string ReadToEndOfLine(string input, int start) {
            return input.Substring(start).Trim();
        }

        private static int ReadToNextEndOfWord(string input, int start, out string word) {
            StringBuilder strBuilder = new StringBuilder();
            start = ReadToNextNonWhitespace(input, start);
            while (input.Length > start && !char.IsWhiteSpace(input[start])) {
                strBuilder.Append(input[start]);
                ++start;
            }
            word = strBuilder.ToString();
            return start;
        }

        private static int ReadToNextNonWhitespace(string input, int start) {
            while (input.Length > start && char.IsWhiteSpace(input[start])) ++start;
            return start;
        }

        private static int ReadString(string input, int start, out string str) {
            start = ReadToNextNonWhitespace(input, start);
            if (IsAt(input, start, MLS_START)) {
                return ReadMultiLineString(input, start, out str);
            } else if (IsAt(input, start, STR_START)) {
                return ReadQuotedString(input, start, out str);
            } else {
                return ReadUnquotedString(input, start, out str);
            }
        }

        private static bool IsAt(string input, int start, string test) {
            int i = 0;
            for (; start < input.Length && i < test.Length; ++i, ++start) {
                if (input[start] != test[i]) return false;
            }
            return i == test.Length;
        }

        private static int ReadMultiLineString(string input, int start, out string str) {
            start = ReadToNextNonWhitespace(input, start);
            start += 3;
            str = null;
            return start;
        }

        private static int ReadQuotedString(string input, int start, out string str) {
            start += 1;
            return ReadAndUnescapeString(input, start, QUOTED_STRING_TERM_CHARS, ESCAPE_CHAR, NORMAL_ESCAPE_CHARS, NORMAL_STRING_ESCAPES, false, out str);
        }

        private static int ReadUnquotedString(string input, int start, out string str) {
            return ReadAndUnescapeString(input, start, UNQUOTED_STRING_TERM_CHARS, ESCAPE_CHAR, NORMAL_ESCAPE_CHARS, NORMAL_STRING_ESCAPES, true, out str);
        }

        private static int ReadUnquotedKeyString(string input, int start, out string str) {
            return ReadAndUnescapeString(input, start, UNQUOTED_KEY_STRING_TERM_CHARS, ESCAPE_CHAR, NORMAL_ESCAPE_CHARS, NORMAL_STRING_ESCAPES, true, out str);
        }

        private const char ESCAPE_CHAR = '\\';
        private static readonly char[] QUOTED_STRING_TERM_CHARS = new char[] { '"', '\n' };
        private static readonly char[] UNQUOTED_KEY_STRING_TERM_CHARS = new char[] { '\n', ':' };
        private static readonly char[] UNQUOTED_STRING_TERM_CHARS = new char[] { '\n' };
        private static readonly char[] NORMAL_ESCAPE_CHARS = new char[] { '\"', '\\', 'b', 'f', 'n', 'r', 't', 'v' };
        private static readonly char[] NORMAL_STRING_ESCAPES = new char[] { '\"', '\\', '\b', '\f', '\n', '\r', '\t', '\v' };

        enum StringParseContext {
            Standard,
            InEscape
        }

        private static int ReadAndUnescapeString(string input, int start, char[] terminatingChars, char escapeChar, char[] escapes, char[] escapeLiterals, bool removeTerminatingWhitespace, out string str) {
            StringBuilder sb = new StringBuilder();
            StringParseContext pc = StringParseContext.Standard;
            int curr = start;
            for (; input.Length > curr; ++curr) {
                char currChar = input[curr];
                if (Array.IndexOf(terminatingChars, currChar) != -1) {
                    ++curr;
                    break;
                }
                if (pc != StringParseContext.InEscape) {
                    if (currChar == escapeChar) {
                        pc = StringParseContext.InEscape;
                        continue;
                    }
                    sb.Append(currChar);
                } else {
                    int escapeCharIdx = Array.IndexOf(escapes, escapeChar);
                    if (escapeCharIdx >= 0) {
                        sb.Append(NORMAL_STRING_ESCAPES[escapeCharIdx]);
                    } else {
                        // Invalid escape code. Assume they meant literally a '\' and then this character.
                        Debug.LogWarning($"Attempted to escape invalid escape character {currChar}. Adding both the escape and this character literally.");
                        sb.Append(escapeChar);
                        sb.Append(currChar);
                    }
                    pc = StringParseContext.Standard;
                }
            }
            str = sb.ToString();
            if (removeTerminatingWhitespace) {
                str = str.TrimEnd();
            }
            return curr;
        }

        private static int ReadAndUnescapeMLString(string input, int start, char[] terminatingChars, char escapeChar, char[] escapes, char[] escapeLiterals, bool removeTerminatingWhitespace, out string str) {
            StringBuilder sb = new StringBuilder();
            StringParseContext pc = StringParseContext.Standard;
            int curr = start;
            for (; input.Length > curr; ++curr) {
                char currChar = input[curr];
                if (Array.IndexOf(terminatingChars, currChar) != -1) {
                    ++curr;
                    break;
                }
                if (pc != StringParseContext.InEscape) {
                    if (currChar == escapeChar) {
                        pc = StringParseContext.InEscape;
                        continue;
                    }
                    sb.Append(currChar);
                } else {
                    int escapeCharIdx = Array.IndexOf(escapes, escapeChar);
                    if (escapeCharIdx >= 0) {
                        sb.Append(NORMAL_STRING_ESCAPES[escapeCharIdx]);
                    } else {
                        // Invalid escape code. Assume they meant literally a '\' and then this character.
                        Debug.LogWarning($"Attempted to escape invalid escape character {currChar}. Adding both the escape and this character literally.");
                        sb.Append(escapeChar);
                        sb.Append(currChar);
                    }
                    pc = StringParseContext.Standard;
                }
            }
            str = sb.ToString();
            if (removeTerminatingWhitespace) {
                str = str.TrimEnd();
            }
            return curr;
        }
    }
}