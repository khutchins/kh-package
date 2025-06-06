﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KH.Texts {
	/// <summary>
	/// Parses text into its components with tags to more easily insert tags
	/// into the text without breaking Unity's rich text formatting. This
	/// assumes the base string is in a format Unity's parser will accept.
	/// </summary>
	public class TokenizedText {

		private List<TextToken> tokens;
		private string CachedTextWithoutMarkup;

		private int _length = -1;

		/// <summary>
		/// The length of the text without tags.
		/// </summary>
		public int Length {
			get {
				if (_length < 0) {
					int len = 0;
					for (int i = 0; i < tokens.Count; i++) {
						if (!tokens[i].isTag) {
							len += tokens[i].text.Length;
						}
					}
					_length = len;
				}
				return _length;
			}
		}

		public TokenizedText(List<TextToken> tokens) {
			this.tokens = tokens;
		}

		public TokenizedText(string str) {
			ParseString(str);
		}

		/// <summary>
		/// TEST ONLY. Returns all tokens. Modifying this list could do strange
		/// things. But hey, do what you want, you'll only break your own code.
		/// </summary>
		public List<TextToken> AllTokens() {
			return tokens;
		}

		private void ParseString(string str) {
			tokens = new List<TextToken>();
			Stack<TextToken> tags = new Stack<TextToken>();
			while (str.Length > 0) {
				TextToken token;
				if (str[0] == '<') {
					int endIdx = str.IndexOf('>');
					if (endIdx == -1) {
						token = new TextToken(str);
						str = "";
					} else {
						token = new TextToken(str.Substring(0, endIdx + 1));
						str = endIdx == str.Length - 1 ? "" : str.Substring(endIdx + 1);
					}
				} else {
					int endIdx = str.IndexOf('<');
					if (endIdx == -1) {
						token = new TextToken(str);
						str = "";
					} else {
						token = new TextToken(str.Substring(0, endIdx));
						str = str.Substring(endIdx);
					}
				}

				if (token.isTag && token.requiresMatch && !token.isClosingTag) {
					// Open tag
					tags.Push(token);
					tokens.Add(token);
				} else if (token.isTag && token.requiresMatch && token.isClosingTag) {
					// Close tag
					if (tags.Count == 0 || tags.Peek().key != token.key) {
						// Bad close tag, ignore.
					} else {
						tags.Pop();
						tokens.Add(token);
					}
				} else {
					tokens.Add(token);
				}
			}

			// Close any open tags
			while (tags.Count > 0) {
				TextToken openTag = tags.Pop();
				tokens.Add(TextToken.CloseTagForTag(openTag));
			}
		}

		public string GetStringWithAllTokens() {
			return GetString(tokens, true);
		}

		public string GetString() {
			return GetString(tokens);
		}

		public List<TextToken> TokensForIndex(int idx) {
			Stack<TextToken> currentTokens = new Stack<TextToken>();
			List<TextToken> currentIndexTokens = new List<TextToken>();

			int currentIdx = 0;
			foreach (TextToken token in tokens) {
				if (!token.isTag) {
					if (currentIdx == idx) {
						break;
					}
					currentIndexTokens.Clear();
					currentIdx += token.text.Length;
					if (currentIdx > idx) {
						break;
					}
				} else { // isTag
					if (!token.requiresMatch) { // Tokens without closing tag should only occur if right before idx
						currentIndexTokens.Add(token);
					} else if (token.isClosingTag) { // We can clear any opening tag because of the assumptions we make with Unity's rich text parser
						currentTokens.Pop();
					} else {
						currentTokens.Push(token);
					}
				}
			}
			return currentIndexTokens.Concat(currentTokens).Reverse().ToList();
		}

		public TokenizedText Substring(int start) {
			return Substring(start, this.Length - start);
		}

		public TokenizedText Substring(int start, int length) {
			List<TextToken> tokensToInclude = new List<TextToken>();
			int currentIdx = 0;

			List<TextToken> openTags = new List<TextToken>();
			for (int i = 0; i < tokens.Count; i++) {
				TextToken token = tokens[i];

				if (token.isTag && currentIdx <= start + length) {
					if (!token.requiresMatch) {
					} else if (token.isClosingTag) {
						if (openTags.Count == 0 || openTags.Last().key != token.key) {
							// Incorrectly formatted tag. Does not mirror an open tag. Just ignore it.
						} else {
							openTags.RemoveAt(openTags.Count - 1);
						}
					} else { // is Opening Tag
						openTags.Add(token);
					}
					if (currentIdx > start && currentIdx <= start + length) {
						tokensToInclude.Add(token);
					}
				} else { // Is String
					if (currentIdx + token.text.Length < start || currentIdx >= start + length) { // Outside of range
																								  // Do nothing
					} else if (currentIdx <= start && currentIdx + token.text.Length > start) { // Starts before or at start
																								// Include unclosed tags
						tokensToInclude.AddRange(openTags);

						// Include all within range
						int amtBeforeIdx = start - currentIdx;
						int amtToInclude = Math.Min(length, token.text.Length - amtBeforeIdx);
						if (amtToInclude == token.text.Length) {
							tokensToInclude.Add(token);
						} else {
							tokensToInclude.Add(new TextToken(token.text.Substring(amtBeforeIdx, amtToInclude)));
						}
					} else if (currentIdx > start && currentIdx < start + length) { // Starts before start + length
						int amtToInclude = Math.Min(length - (currentIdx - start), token.text.Length);

						if (amtToInclude == token.text.Length) {
							tokensToInclude.Add(token);
						} else {
							tokensToInclude.Add(new TextToken(token.text.Substring(0, amtToInclude)));
						}
					}
					currentIdx += token.text.Length;
				}
			}
			openTags.Reverse();
			foreach (TextToken token in openTags) {
				tokensToInclude.Add(new TextToken(token.closingTag));
			}
			return new TokenizedText(tokensToInclude);
		}

		public TokenizedText AppendedText(TokenizedText textToAppend) {
			return new TokenizedText(this.tokens.Concat(textToAppend.tokens).ToList());
		}

		public string GetStringInsertingTagOpenBeforeIndex(int idx, string open, string close) {
			List<TextToken> tokensWithTag = new List<TextToken>();
			int currentIdx = 0;
			bool insertedTag = false;
			Stack<TextToken> tagStack = new Stack<TextToken>();
			for (int i = 0; i < tokens.Count; i++) {
				TextToken token = tokens[i];
				if (insertedTag) {
					if (token.allowAfterInsert) {
						tokensWithTag.Add(token);
					}
					continue;
				}
				if (token.isTag) {
					if (!token.requiresMatch) {
					} else if (token.isClosingTag) {
						tagStack.Pop();
					} else { // is Opening Tag
						tagStack.Push(token);
					}
					tokensWithTag.Add(token);
				} else { // Is String
					if (currentIdx + token.text.Length <= idx) {
						tokensWithTag.Add(token);
						currentIdx += token.text.Length;
					} else {
						int amtBeforeIdx = idx - currentIdx;
						tokensWithTag.Add(new TextToken(token.text.Substring(0, amtBeforeIdx)));
						Stack<TextToken> reverseStack = new Stack<TextToken>();
						while (tagStack.Count > 0) {
							TextToken stackToken = tagStack.Pop();
							tokensWithTag.Add(new TextToken(stackToken.closingTag));
							reverseStack.Push(stackToken);
						}
						tokensWithTag.Add(new TextToken(open));
						while (reverseStack.Count > 0) {
							TextToken stackToken = reverseStack.Pop();
							if (stackToken.allowAfterInsert) {
								tokensWithTag.Add(stackToken);
							}
							tagStack.Push(stackToken);
						}
						int amtAfterIdx = (currentIdx + token.text.Length) - idx;
						tokensWithTag.Add(new TextToken(token.text.Substring(amtBeforeIdx, amtAfterIdx)));
						insertedTag = true;
					}
				}
			}
			if (insertedTag) {
				tokensWithTag.Add(new TextToken(close));
			}
			return GetString(tokensWithTag);
		}

		private string GetString(List<TextToken> tokensToPrint, bool printAll = false) {
			StringBuilder builder = new StringBuilder();
			foreach (TextToken token in tokensToPrint) {
				if (!token.isTag || token.outputText || printAll) {
					builder.Append(token.text);
				}
			}
			return builder.ToString();
		}

		public string GetTextWithoutMarkup() {
			if (CachedTextWithoutMarkup == null) {
				StringBuilder builder = new StringBuilder();
				foreach (TextToken token in tokens) {
					if (!token.isTag) {
						builder.Append(token.text);
					}
				}
				CachedTextWithoutMarkup = builder.ToString();
			}
			return CachedTextWithoutMarkup;
		}
	}

	public class TextToken : IEquatable<TextToken> {
		public readonly string text;
		public readonly bool isTag = false;
		public readonly bool isClosingTag = false;
		public readonly bool requiresMatch = true;
		public readonly bool allowAfterInsert = true;
		public readonly string closingTag = "";
		public readonly string key = "";
		public readonly string value = "";
		public readonly bool hasValue = false;
		public readonly bool outputText = true;

		public static readonly HashSet<string> TMPRO_TAGS = new HashSet<string>() {
			"align", "allcaps", "alpha", "b", "br", "color", "cspace", "font", "font-weight",
			"gradient", "i", "ident", "line-height", "line-indent", "link", "lowercase", "margin",
			"mark", "mspace", "nobr", "noparse", "page", "pos", "rotate", "s", "size", "smallcaps",
			"space", "sprite", "strikethrough", "style", "sub", "sup", "u", "uppercase", "voffset",
			"width"
		};

		public static TextToken CloseTagForTag(TextToken tag) {
			return new TextToken(tag.closingTag);
		}

		public TextToken(string str) {
			text = str;
			if (str.Length == 0) {
				return;
			}
			if (str.Length > 0 && str[0] == '<' && str[str.Length - 1] == '>') {
				isTag = true;

				string clearString = str.Replace("</", "");
				clearString = clearString.Replace("<", "");
				clearString = clearString.Replace("/>", "");
				clearString = clearString.Replace(">", "");

				string[] components = clearString.Split('=');
				key = components[0];
				if (components.Length > 1) {
					value = components[1];
					hasValue = true;
				}

				if (key == "color") {
					allowAfterInsert = false;
				}

				if (str.Length > 1) {
					if (str[1] == '/') {
						isClosingTag = true;
					}
					if (str[str.Length - 2] == '/') {
						requiresMatch = false;
					}
				}

				if (isTag && !isClosingTag && requiresMatch) {
					int closeIdx = text.IndexOf('>');
					int equalsIdx = text.IndexOf('=');
					int endIdx = equalsIdx != -1 ? equalsIdx : closeIdx;
					closingTag = "</" + text.Substring(1, endIdx - 1) + ">";
				}

				outputText = TMPRO_TAGS.Contains(key);
			}
		}

		public override string ToString() {
			return text;
		}

		public override bool Equals(object obj) {
			return Equals(obj as TextToken);
		}

		public bool Equals(TextToken other) {
			return other != null &&
				   text == other.text &&
				   isTag == other.isTag &&
				   isClosingTag == other.isClosingTag &&
				   requiresMatch == other.requiresMatch &&
				   allowAfterInsert == other.allowAfterInsert &&
				   closingTag == other.closingTag &&
				   key == other.key &&
				   value == other.value &&
				   hasValue == other.hasValue &&
				   outputText == other.outputText;
		}

		public override int GetHashCode() {
			int hashCode = -979268291;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(text);
			hashCode = hashCode * -1521134295 + isTag.GetHashCode();
			hashCode = hashCode * -1521134295 + isClosingTag.GetHashCode();
			hashCode = hashCode * -1521134295 + requiresMatch.GetHashCode();
			hashCode = hashCode * -1521134295 + allowAfterInsert.GetHashCode();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(closingTag);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(key);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(value);
			hashCode = hashCode * -1521134295 + hasValue.GetHashCode();
			hashCode = hashCode * -1521134295 + outputText.GetHashCode();
			return hashCode;
		}
	}
}