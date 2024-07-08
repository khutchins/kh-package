using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace KH.Console {
    public static class CommandParser {
        enum ParseContext {
            Standard,
            Start,
            InEscape
        }
        enum TermChar {
            Whitespace,
            SingleQuotes,
            DoubleQuotes
        }

        public static IEnumerable<string> ParseText(string text, bool includeTrailingWhitespace = false) {
            if (string.IsNullOrWhiteSpace(text)) yield break;

            char termChar = '\0';
            StringBuilder token = new StringBuilder();
            var context = ParseContext.Start;
            bool hadWhitespace = false;

            void Reset() {
                token.Clear();
                termChar = '\0';
                context = ParseContext.Start;
                hadWhitespace = false;
            }

            for (int i = 0; i < text.Length; i++) {
                char curr = text[i];

                if (context == ParseContext.Start) {
                    if (curr == '\'' || curr == '"') {
                        termChar = curr;
                        context = ParseContext.Standard;
                    } else if (char.IsWhiteSpace(curr)) {
                        // Eat character, as empty tokens are removed (unless double quoted.)
                        hadWhitespace = true;
                        context = ParseContext.Start;
                    } else {
                        // Replay character in standard context, since we know an escape isn't relevant.
                        i--;
                        context = ParseContext.Standard;
                    }
                } else if (context == ParseContext.InEscape) {
                    if (curr == '\\') {
                        token.Append(curr);
                    } else if (termChar != '\0' && curr == termChar) {
                        token.Append(termChar);
                    } else {
                        Debug.LogWarning($"Unexpected escape character: '{curr}'. Assuming that escaping was not intended. Don't do this, as escape characters can be added.");
                        token.Append('\\');
                        token.Append(curr);
                    }
                    context = ParseContext.Standard;
                } else {
                    // String started with " or ', putting it in complex mode.
                    if (termChar != '\0') {
                        if (curr == '\\') {
                            context = ParseContext.InEscape;
                        } else if (curr == termChar) {
                            yield return token.ToString();
                            Reset();
                        } else {
                            token.Append(curr);
                        }
                    } else { // Just a normal string, terminated by whitespace.
                        if (char.IsWhiteSpace(curr)) {
                            yield return token.ToString();
                            Reset();
                            hadWhitespace = true;
                        } else {
                            token.Append(curr);
                        }
                    }
                }
            }

            string remainder = token.ToString();
            if (!string.IsNullOrWhiteSpace(remainder)) {
                yield return remainder;
            } else if (hadWhitespace && includeTrailingWhitespace) {
                yield return "";
            }
        }
    }
}