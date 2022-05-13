using System.Collections;
using System.Collections.Generic;

namespace KH.Texts {
	public class TextUpdate {
		public string NewString;
		public float Delay;
		public bool PlayBlip;
		public List<TextToken> UnrecognizedTags;
		public bool BypassKeypress;

		public TextUpdate(string newString, float delay, bool playBlip, List<TextToken> unrecognizedTags, bool bypassKeypress = false) {
			NewString = newString;
			Delay = delay;
			PlayBlip = playBlip;
			UnrecognizedTags = unrecognizedTags;
			BypassKeypress = bypassKeypress;
		}

		public override string ToString() {
			return NewString;
		}
	}

	/// <summary>
	/// A class that, given a line of text, enumerates all of the states that it should be in.
	/// </summary>
	public class TextPlayer : IEnumerable<TextUpdate> {

		private readonly TokenizedText _parser;
		private readonly float _baseSpeedMod;

		public TextPlayer(string rawText, float baseSpeedMod = 1f) {
			_parser = new TokenizedText(rawText);
			_baseSpeedMod = baseSpeedMod;
		}

		public string GetFinalString() {
			return _parser.GetString();
		}

		public string GetFinalStringWithoutMarkup() {
			return _parser.GetTextWithoutMarkup();
		}

		public IEnumerator<TextUpdate> GetEnumerator() {
			bool shouldNotWaitForKeypress = false;
			string invisibleMarkup = "<color=#00000000>";
			string invisibleMarkupEnd = "</color>";
			string strippedText = _parser.GetTextWithoutMarkup();

			yield return new TextUpdate(_parser.GetStringInsertingTagOpenBeforeIndex(0, invisibleMarkup, invisibleMarkupEnd), 0, false, new List<TextToken>());

			// This goes to length + 1 as tags can occur after the last character, and we will miss them otherwise.
			for (int stripIdx = 0; stripIdx < strippedText.Length + 1; stripIdx++) {
				string newText = _parser.GetStringInsertingTagOpenBeforeIndex(stripIdx + 1, invisibleMarkup, invisibleMarkupEnd);

				List<TextToken> tokens = _parser.TokensForIndex(stripIdx);
				float percentMod = _baseSpeedMod;
				float addMod = 0F;
				bool uniform = false;
				List<TextToken> unrecognizedTokens = new List<TextToken>();
				foreach (TextToken token in tokens) {
					switch (token.key) {
						case "pause":
							addMod += OptParse(token.value, 0.5F);
							break;
						case "speed":
							percentMod = OptParse(token.value, 1F);
							break;
						case "bypass":
							shouldNotWaitForKeypress = OptParse(token.value, true);
							break;
						case "uniform":
							uniform = OptParse(token.value, true);
							break;
						default:
							unrecognizedTokens.Add(token);
							break;
					}
				}

				char prev = GetCharAtIndex(strippedText, stripIdx - 1, '\0');
				char curr = GetCharAtIndex(strippedText, stripIdx, '\0');
				char next = GetCharAtIndex(strippedText, stripIdx + 1, '\0');

				float delay = TimeForCharacter(uniform, prev, curr, next);
				float actualDelay = delay / percentMod + addMod;

				if (actualDelay > 0F) {
					bool blip = curr != '\0' && delay > 0 && ShouldPlaySound(curr);
					yield return new TextUpdate(newText, actualDelay, blip, unrecognizedTokens);
				}
			}

			if (shouldNotWaitForKeypress) {
				yield return new TextUpdate(_parser.GetString(), 1.5f, false, new List<TextToken>(), true);
			}
		}

		private char GetCharAtIndex(string text, int idx, char defaultValue) {
			return idx < text.Length && idx >= 0 ? text[idx] : defaultValue;
		}

		IEnumerator IEnumerable.GetEnumerator() {
			throw new System.NotImplementedException();
		}


		static float OptParse(string str, float def) {
			if (str == null)
				return def;

			if (!float.TryParse(str, out float fl)) {
				fl = def;
			}
			return fl;
		}

		static bool OptParse(string str, bool def) {
			if (str == null)
				return def;

			if (!bool.TryParse(str, out bool b)) {
				b = def;
			}
			return b;
		}

		private static bool ShouldPlaySound(char curr) {
			switch (curr) {
				case '\n':
					return false;
				default:
					return true;
			}
		}

		private static float TimeForCharacter(bool uniform, char prev, char curr, char next) {
			if (curr == '\n' && next == '\n') {
				return .1F;
			}
			if (uniform) {
				switch (next) {
					case ' ':
						return 0F;
					default:
						return 0.015F;
				}
			}
			// It looks odd to have extra time for a closing brace or quote,
			// so apply the previous time (which was skipped) to play them
			// together.
			if (curr == ')' || curr == '"') {
				return TimeForCharacter(uniform, '\0', prev, ' ');
			}
			switch (next) {
				case ')':
					return 0F;
				case '"':
					return 0F;
				case '\0':
					return 0F;
			}
			switch (curr) {
				case '\0':
				case ' ':
				case '(':
				case '\n':
					return 0F;
				case '.':
				case '!':
				case '?':
				case '—':
				case ':':
				case ';':
				case ')':
				case '–':
					return 0.2F;
				case ',':
					return 0.1F;
				default:
					return 0.015F;
			}
		}
	}
}