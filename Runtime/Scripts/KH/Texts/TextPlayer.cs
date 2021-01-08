﻿using System.Collections;
using System.Collections.Generic;

public delegate void TextFinishedHandler(bool shouldPlayNextText);
public delegate void TextAnimateOutFinishedHandler();

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
	}

	public class TextPlayer : IEnumerable<TextUpdate> {

		private readonly TokenizedText _parser;
		private readonly float _baseSpeedMod;

		public TextPlayer(string rawText, float baseSpeedMod) {
			_parser = new TokenizedText(rawText);
			_baseSpeedMod = baseSpeedMod;
		}

		public string GetFinalString() {
			return _parser.GetString();
		}

		public IEnumerator<TextUpdate> GetEnumerator() {
			int stripIdx = 0;

			bool shouldNotWaitForKeypress = false;
			string invisibleMarkup = "<color=#00000000>";
			string invisibleMarkupEnd = "</color>";
			string strippedText = _parser.GetTextWithoutMarkup();

			yield return new TextUpdate(_parser.GetStringInsertingTagOpenAtIndex(stripIdx, invisibleMarkup, invisibleMarkupEnd), 0, false, new List<TextToken>());

			while (stripIdx < strippedText.Length + 1) {
				string newText = _parser.GetStringInsertingTagOpenAtIndex(stripIdx, invisibleMarkup, invisibleMarkupEnd);

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

				char prev = stripIdx - 2 < strippedText.Length && stripIdx >= 2 ? strippedText[stripIdx - 2] : '\0';
				char curr = stripIdx - 1 < strippedText.Length && stripIdx >= 1 ? strippedText[stripIdx - 1] : '\0';
				char next = stripIdx < strippedText.Length ? strippedText[stripIdx] : '\0';
				// TODO: Hook into settings for modifier
				float delay = TimeForCharacter(uniform, prev, curr, next) * (1F / SettingsWrapper.TextSpeed);
				float actualDelay = delay / percentMod + addMod;
				stripIdx++;
				if (actualDelay > 0F) {
					bool blip = curr != '\0' && delay > 0 && ShouldPlaySound(curr);
					yield return new TextUpdate(newText, actualDelay, blip, unrecognizedTokens);
				}
			}

			if (shouldNotWaitForKeypress) {
				yield return new TextUpdate(_parser.GetString(), 1.5f, false, new List<TextToken>(), true);
			}
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
			if (curr == ')' || curr == '"') {
				return TimeForCharacter(uniform, '\0', prev, ' ');
			}
			switch (next) {
				case '\0':
					return 0F;
				case ')':
					return 0F;
				case '"':
					return 0F;
			}
			switch (curr) {
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