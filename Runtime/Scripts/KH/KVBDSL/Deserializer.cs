using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KH.KVBDSL {
    public class Deserializer {
        public class TypeHandler {
            public readonly string Type;
            /// <summary>
            /// Whether or not whitespace is required after the type identifier to begin parsing.
            /// e.g. foo: "bar" should begin matching once it sees '"' and not require a space.
            /// </summary>
            public readonly bool MatchesWithoutWhitespace = false;
            /// <summary>
            /// Line parser. 
            /// For the result parameter: if valid, return the parsed object. Otherwise return null.
            /// For the idx: In both success or failure, return the index of the EOL newline or the start of the next line. Either is fine.
            /// </summary>
            public readonly Func<Deserializer, string, int, (int idx, object result)> Parse;

            public TypeHandler(string type, Func<Deserializer, string, int, (int idx, object result)> parse, bool matchesWithoutWhitespace = false) {
                Type = type;
                MatchesWithoutWhitespace = matchesWithoutWhitespace;
                Parse = parse;
            }
        }
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

        private static readonly TypeHandler IntHandler = new TypeHandler(Consts.TYPE_INT, (Deserializer ds, string input, int start) => {
            int curr = ReadToEndOfLine(input, start, out string str);
            object output = null;
            if (int.TryParse(str, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result)) {
                output = result;
            } else {
                curr = ReadToNextLineAndOutputError(input, start, "Invalid int format");
            }
            return (curr, output);
        });
        private static readonly TypeHandler FloatHandler = new TypeHandler(Consts.TYPE_FLOAT, (Deserializer ds, string input, int start) => {
            int curr = ReadToEndOfLine(input, start, out string str);
            object output = null;
            if (float.TryParse(str, NumberStyles.Float, CultureInfo.InvariantCulture, out float result)) {
                output = result;
            } else {
                curr = ReadToNextLineAndOutputError(input, start, "Invalid float format");
            }
            return (curr, output);
        });
        private static readonly TypeHandler BoolHandler = new TypeHandler(Consts.TYPE_BOOL, (Deserializer ds, string input, int start) => {
            int curr = ReadToEndOfLine(input, start, out string str);
            object output = null;
            if (bool.TryParse(str, out bool result)) {
                output = result;
            } else {
                curr = ReadToNextLineAndOutputError(input, start, "Invalid bool format");
            }
            return (curr, output);
        });
        private static readonly TypeHandler ArrayHandler = new TypeHandler(Consts.TYPE_ARRAY, (Deserializer ds, string input, int start) => {
            bool hadError = false;
            int curr = ReadToNextLineOrNonWhitespace(input, start);
            if (curr >= input.Length) {
                Debug.LogWarning($"Unexpected EOF reading array.");
            } else if (input[curr] != '\n') {
                // If the check above didn't read to whitespace, it means that there is text on the same line.
                curr = ReadToNextLineAndOutputError(input, start, "Array has content beyond '['.");
                hadError = true;
            }

            List<object> array = new List<object>();
            
            // We need to read to the next non-whitespace, even though ParseValue already does that,
            // as we're looking for the closing ']'.
            curr = ReadToNextNonWhitespace(input, curr);
            while (curr < input.Length && input[curr] != Consts.TYPE_ARRAY_END) {
                curr = ds.ParseValue(input, curr, out object value);
                if (!hadError && value != null) {
                    array.Add(value);
                }
                curr = ReadToNextNonWhitespace(input, curr);
            }
            // Reached EOF without closing ']'.
            if (curr >= input.Length) {
                ReadToNextLineAndOutputError(input, start, curr, "Array has no closing ']'.");
                hadError = true;
            } else {
                // Skip past the ']'.
                ++curr;
                // Skip past the ']' line.
                curr = ReadToNextLineOrNonWhitespace(input, curr);
                // If there's more text on the line, fail the parse because honestly, how hard is it?
                // This one I went back and forth on accepting, as while I can see the case for the parser having
                // text after the '[' (for singlely typed arrays), I don't see a use case here. Regardless, it's
                // probably an error unless it was a comment, which I may allow later.
                if (curr < input.Length && input[curr] != '\n') {
                    curr = ReadToNextLineAndOutputError(input, start, curr, "Array has content beyond ']'.");
                    hadError = true;
                }
            }

            // If they got an error here, return nothing. Probably something deeply wrong occurred,
            // or they were just too lazy to close a terminal array. Either way, teach them the error
            // of their ways.
            if (hadError) return (curr, null);
            else return (curr, array);
        });
        private static readonly TypeHandler DictionaryHandler = new TypeHandler(Consts.TYPE_DICT, (Deserializer ds, string input, int start) => {
            bool hadError = false;
            int curr = ReadToNextLineOrNonWhitespace(input, start);
            if (curr >= input.Length) {
                Debug.LogWarning($"Unexpected EOF reading array.");
            } else if (input[curr] != '\n') {
                // If the check above didn't read to whitespace, it means that there is text on the same line.
                curr = ReadToNextLineAndOutputError(input, start, "Dictionary has content beyond '{'.");
                hadError = true;
            }

            Dictionary<string, object> dict = new Dictionary<string, object>();

            // We need to read to the next non-whitespace, even though ParseValue already does that,
            // as we're looking for the closing '}'.
            curr = ReadToNextNonWhitespace(input, curr);
            while (curr < input.Length && input[curr] != Consts.TYPE_DICT_END) {
                curr = ds.ParseEntry(input, curr, out string key, out object value);
                if (!hadError && key != null && value != null) {
                    dict[key] = value;
                }
                curr = ReadToNextNonWhitespace(input, curr);
            }
            // Reached EOF without closing '}'.
            if (curr >= input.Length) {
                ReadToNextLineAndOutputError(input, start, curr, "Dictionary has no closing '}'.");
                hadError = true;
            } else {
                // Skip past the '}'.
                ++curr;
                // Skip past the '}' line.
                curr = ReadToNextLineOrNonWhitespace(input, curr);
                // If there's more text on the line, fail the parse because honestly, how hard is it?
                // This one I went back and forth on accepting, as while I can see the case for the parser having
                // text after the '{' (for singlely typed dictionaries), I don't see a use case here. Regardless, it's
                // probably an error unless it was a comment, which I may allow later.
                if (curr < input.Length && input[curr] != '\n') {
                    curr = ReadToNextLineAndOutputError(input, start, curr, "Dictionary has content beyond '}'.");
                    hadError = true;
                }
            }

            // If they got an error here, return nothing. Probably something deeply wrong occurred,
            // or they were just too lazy to close a terminal dictionary. Either way, teach them the error
            // of their ways.
            if (hadError) return (curr, null);
            else return (curr, dict);
        });

        private static readonly Func<Deserializer, string, int, (int idx, object result)> StringHandlerInternals = (Deserializer ds, string input, int start) => {
            int curr = ReadString(input, start, out string str);
            return (curr, str);
        };

        private static readonly TypeHandler StringHandler = new TypeHandler("s", StringHandlerInternals);
        private static readonly TypeHandler StringHandlerSingleQuote = new TypeHandler(Consts.STR_START, StringHandlerInternals, true);
        private static readonly TypeHandler StringHandlerTripleQuote = new TypeHandler(Consts.MLS_START, StringHandlerInternals, true);


        public static Dictionary<string, TypeHandler> Handlers = new Dictionary<string, TypeHandler>() {
            { IntHandler.Type, IntHandler },
            { FloatHandler.Type, FloatHandler },
            { BoolHandler.Type, BoolHandler },
            { StringHandler.Type, StringHandler },
            { StringHandlerSingleQuote.Type, StringHandlerSingleQuote },
            { StringHandlerTripleQuote.Type, StringHandlerTripleQuote },
            { ArrayHandler.Type, ArrayHandler },
            { DictionaryHandler.Type, DictionaryHandler },
        };

        private List<TypeHandler> _midStringMatchList;

        public Deserializer() { 
            _midStringMatchList = Handlers.Where(x => x.Value.MatchesWithoutWhitespace).Select(x => x.Value).OrderBy(x => x.Type.Length).ToList();
        }

        public Dictionary<string, object> Parse(string input) {
            return ParseString(input);
        }

        bool CheckNoWhitespaceMatch(string input, out TypeHandler typeHandler) {
            // _midStringMatchList is guaranteed to be ordered in descending string length order,
            // so it will never mat6ch a shorter one over a longer one.
            // For instance, it will look for """ before " when searching for an untyped string literal.
            foreach (var handler in _midStringMatchList) {
                if (input.StartsWith(handler.Type)) {
                    typeHandler = handler;
                    return true;
                }
            }
            typeHandler = null;
            return false;
        }

        int ParseValue(string input, int start, out object value) {
            int curr = ReadToNextEndOfWord(input, start, out string type);
            value = null;
            if (type == null || type.Length == 0) {
                curr = ReadToNextLineAndOutputError(input, start, $"No type provided");
                value = null;
                return curr;
            } else if (CheckNoWhitespaceMatch(type, out TypeHandler handler)) {
                // Handlers that mark themself as not requiring whitespace to match must include the type
                // in the parse handling, so the start index is passed in instead of the curr index. 
                var result = handler.Parse(this, input, start);
                value = result.result;
                return result.idx;
            } else if (Handlers.TryGetValue(type, out handler)) {
                var result = handler.Parse(this, input, curr);
                value = result.result;
                return result.idx;
            } else {
                value = null;
                return ReadToNextLineAndOutputError(input, curr, $"Unrecognized type '{type}'");
            }
        }

        public int ParseArrayValue(string input, int curr, out object value) {
            value = null;
            curr = ReadToNextNonWhitespace(input, curr);
            if (curr >= input.Length) { // EOF
                return curr;
            }

            // Comment
            if (input[curr] == '#') {
                curr = ReadToNextLine(input, curr);
                return curr;
            }

            return ParseValue(input, curr, out value);
        }

        public int ParseEntry(string input, int curr, out string key, out object value) {
            key = null;
            value = null;
            curr = ReadToNextNonWhitespace(input, curr);
            if (curr >= input.Length) { // EOF
                return curr; 
            }
            int lineStart = curr;

            // Comment
            if (input[curr] == '#') {
                curr = ReadToNextLine(input, curr);
                return curr;
            }
            curr = ReadKey(input, curr, out key);
            if (key == null) {
                curr = ReadToNextLineAndOutputError(input, lineStart, "Improperly formatted line. Should be comment or kvp");
                return curr;
            }
            curr = ReadToNextNonWhitespace(input, curr);
            if (input.Length <= curr) {
                curr = ReadToNextLineAndOutputError(input, lineStart, "Unexpected end of file");
                key = null;
                return curr;
            }
            if (input[curr] != ':') {
                curr = ReadToNextLineAndOutputError(input, lineStart, "Expected ':' to end key, but other char found");
                key = null;
                return curr;
            }
            // Move past key separator.
            ++curr;

            return ParseValue(input, curr, out value);
        }

        private Dictionary<string, object> ParseString(string input) {
            Dictionary<string, object> output = new Dictionary<string, object>();

            int curr = 0;

            while (curr < input.Length) {
                curr = ParseEntry(input, curr, out string key, out object value);
                // Theoretically either null check is sufficient
                if (key != null && value != null) {
                    output[key] = value;
                }
            }
            return output;
        }

        private static int ReadToNextLineAndOutputError(string input, int lineStart, string msg) {
            int endIdx = GetCurrentLine(input, lineStart, out string line);
            Debug.LogWarning($"{msg}. Skipping line at index {lineStart}: '{line}'.");
            return endIdx;
        }

        private static int ReadToNextLineAndOutputError(string input, int lineStart, int curr, string msg) {
            int endIdx = GetCurrentLine(input, curr, out string line);
            GetCurrentLine(input, lineStart, out line);
            
            Debug.LogWarning($"{msg}. Skipping line at index {lineStart}: '{line}'.");
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
                // This will end one char past the :, so we need to move back.
                int curr = ReadUnquotedKeyString(input, start, out key);
                --curr;
                return curr;
            }
        }

        private static int ReadToNextLine(string input, int start) {
            while (input.Length > start && input[start] != '\n') ++start;
            return start;
        }

        private static int GetToNextLine(string input, int start) {
            while (input.Length > start && input[start] != '\n') ++start;
            return start;
        }

        private static int ReadToEndOfLine(string input, int start, out string remainder) {
            int curr = start;
            while (input.Length > curr && input[curr] != '\n') ++curr;
            remainder = input.Substring(start, curr - start);
            return curr;
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

        private static int ReadPastNextLineOrToNonWhitespace(string input, int start) {
            while (input.Length > start) {
                if (input[start] == '\n') return start + 1;
                if (!char.IsWhiteSpace(input[start])) return start;
                ++start;
            }
            return start;
        }

        private static int ReadToNextLineOrNonWhitespace(string input, int start) {
            while (input.Length > start && input[start] != '\n' && char.IsWhiteSpace(input[start])) ++start;
            return start;
        }

        private static int ReadToNextNonWhitespace(string input, int start) {
            while (input.Length > start && char.IsWhiteSpace(input[start])) ++start;
            return start;
        }

        private static int ReadString(string input, int start, out string str) {
            start = ReadToNextNonWhitespace(input, start);
            if (IsAt(input, start, Consts.MLS_START)) {
                return ReadMultiLineString(input, start, out str);
            } else if (IsAt(input, start, Consts.STR_START)) {
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

        private static int MLSStringEnd(string input, int start, string check) {
            StringParseContext pc = StringParseContext.Standard;
            int curr = start;
            int matched = 0;

            int Backtrack(int idx) {
                while (idx > 0 && char.IsWhiteSpace(input[idx])) {
                    if (input[idx] == '\n') return idx;
                    --idx;
                }
                return idx + 1;
            }

            bool CheckProgress(char c) {
                if (c == check[matched]) {
                    ++matched;
                    if (matched == check.Length) {
                        curr -= check.Length;
                        return true;
                    }
                } else {
                    matched = 0;
                }
                return false;
            }

            for (; curr < input.Length; curr++) {
                char currChar = input[curr];
                if (pc != StringParseContext.InEscape) {
                    if (currChar == Consts.ESCAPE_CHAR) {
                        pc = StringParseContext.InEscape;
                        continue;
                    }
                    if (CheckProgress(currChar)) return Backtrack(curr);
                } else {
                    // If we're in an escape, we're not matching against the quote.
                    matched = 0;
                    pc = StringParseContext.Standard;
                }

            }
            return -1;
        }

        private static int ReadMultiLineString(string input, int start, out string str) {
            int curr = ReadToNextNonWhitespace(input, start);
            curr += Consts.MLS_START.Length;

            int textStart = curr;
            str = null;

            // Determine the end so that we don't have to worry about overshooting. This is an additional
            // scan through the whole text. Could optimize later.
            int end = MLSStringEnd(input, curr, Consts.MLS_START);
            if (end == -1) {
                ReadToNextLineAndOutputError(input, start, "Multi-line string is not terminated.");
                return input.Length;
            }

            // Move past opening line.
            textStart = curr = ReadToNextLineOrNonWhitespace(input, curr);
            if (curr >= input.Length) {
                curr = ReadToNextLineAndOutputError(input, start, "Unexpected EOF reading multi-line string.");
                return curr;
            } else if (input[curr] != '\n') {
                // If the check above didn't read to whitespace, it means that there is text on the same line.
                curr = ReadToNextLineAndOutputError(input, start, "Multi-line string has content on opening line beyond '\"\"\"'.");
                return end + Consts.MLS_START.Length + 1;
            }

            StringBuilder sb = new StringBuilder();
            StringParseContext pc = StringParseContext.Standard;

            // First find leading whitespace amount and character.
            char whitespaceChar = '\0';
            int whitespaceAmt = int.MaxValue;

            while (curr < end && whitespaceAmt > 0) {
                int check = ReadPastNextLineOrToNonWhitespace(input, curr);
                // Only compute starting whitespace if there are characters on the line.
                // This adds an additional scan through each line, but if performance is an
                // issue, I can try to fix it later.
                if (check >= end || input[check] != '\n') {
                    int wsL = 0;
                    for (; check < end && wsL < whitespaceAmt; wsL++, check++) {
                        char currChar = input[check];
                        if (currChar != '\t' && currChar != ' ') {
                            break;
                        }
                        if (whitespaceChar == '\0') {
                            whitespaceChar = currChar;
                        } else if (currChar != whitespaceChar) {
                            break;
                        }
                    }
                    whitespaceAmt = Mathf.Min(whitespaceAmt, wsL);
                }
                curr = ReadToNextLine(input, curr + 1);
            }
            if (whitespaceAmt == int.MaxValue) whitespaceAmt = 0;

            // Move past opening newline.
            curr = textStart + 1;
            if (curr < end && curr > 0 && input[curr-1] == '\n') curr += whitespaceAmt;

            // Then parse the lines and construct a string.
            for (int lastNWS = curr; curr < end; curr++) {
                char currChar = input[curr];

                void AddBeforeEscape() {
                    sb.Append(input, lastNWS, curr - 1 - lastNWS);
                    lastNWS = curr + 1;
                }

                if (pc != StringParseContext.InEscape) {
                    if (currChar == Consts.ESCAPE_CHAR) {
                        pc = StringParseContext.InEscape;
                        continue;
                    }

                    if (!char.IsWhiteSpace(currChar)) {
                        sb.Append(input, lastNWS, curr + 1 - lastNWS);
                        lastNWS = curr + 1;
                    } else if (currChar == '\n') {
                        // Skip leading whitespace.
                        sb.Append(currChar);
                        curr += whitespaceAmt;
                        // Skip trailing whitespace.
                        lastNWS = curr + 1;
                    }
                } else {
                    int escapeCharIdx = Array.IndexOf(Consts.NORMAL_ESCAPE_CHARS, currChar);
                    if (escapeCharIdx >= 0) {
                        AddBeforeEscape();
                        sb.Append(Consts.NORMAL_STRING_ESCAPES[escapeCharIdx]);

                    } else if (currChar == 'p') {
                        AddBeforeEscape();
                        // There's nothing to output here.
                    } else {
                        // Invalid escape code. Assume they meant literally a '\' and then this character.
                        Debug.LogWarning($"Attempted to escape invalid escape character {currChar}. Adding both the escape and this character literally.");
                        sb.Append(Consts.ESCAPE_CHAR);
                        sb.Append(currChar);
                    }
                    pc = StringParseContext.Standard;
                }
            }

            str = sb.ToString();
            return end + Consts.MLS_START.Length + 1;
        }

        private static int ReadQuotedString(string input, int start, out string str) {
            start += 1;
            return ReadAndUnescapeString(input, start, Consts.QUOTED_STRING_TERM_CHARS, Consts.ESCAPE_CHAR, Consts.NORMAL_ESCAPE_CHARS, Consts.NORMAL_STRING_ESCAPES, false, out str);
        }

        private static int ReadUnquotedString(string input, int start, out string str) {
            return ReadAndUnescapeString(input, start, Consts.UNQUOTED_STRING_TERM_CHARS, Consts.ESCAPE_CHAR, Consts.NORMAL_ESCAPE_CHARS, Consts.NORMAL_STRING_ESCAPES, true, out str);
        }

        private static int ReadUnquotedKeyString(string input, int start, out string str) {
            return ReadAndUnescapeString(input, start, Consts.UNQUOTED_KEY_STRING_TERM_CHARS, Consts.ESCAPE_CHAR, Consts.NORMAL_ESCAPE_CHARS, Consts.NORMAL_STRING_ESCAPES, true, out str);
        }

        

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
                if (terminatingChars.Contains(currChar)) {
                    if (escapeLiterals.Contains(currChar)) {
                        // If it's an escapable terminal, make sure that
                        // it isn't currently being escaped.
                        if (pc == StringParseContext.Standard) {
                            ++curr;
                            break;
                        }
                    } else {
                        ++curr;
                        break;
                    }
                }
                if (pc != StringParseContext.InEscape) {
                    if (currChar == escapeChar) {
                        pc = StringParseContext.InEscape;
                        continue;
                    }
                    sb.Append(currChar);
                } else {
                    int escapeCharIdx = Array.IndexOf(escapes, currChar);
                    if (escapeCharIdx >= 0) {
                        sb.Append(Consts.NORMAL_STRING_ESCAPES[escapeCharIdx]);
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
                        sb.Append(Consts.NORMAL_STRING_ESCAPES[escapeCharIdx]);
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