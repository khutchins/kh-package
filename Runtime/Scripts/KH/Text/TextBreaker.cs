using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KH.Text {
	public class TextBreaker {

		public readonly int TotalLines;
		public readonly int LineWidth;
		public readonly List<string> AllSections;

		public TextBreaker(string text) : this(3, 32, text) { }

		public TextBreaker(int numberOfLines, int lineWidth, string text) {
			TotalLines = numberOfLines;
			LineWidth = lineWidth;
			AllSections = GetAllSections(text);
		}

		/// <summary>
		/// Gets all sets of lines that should be shown in the text field.
		/// </summary>
		/// <param name="text">The text to split</param>
		/// <returns>A list of text box contents</returns>
		public List<string> GetAllSections(string text) {
			List<string> lines = new List<string>();
			string remainder = text;
			do {
				string[] output = GetNextLines(remainder);
				remainder = output[0];
				lines.Add(output[1]);
			} while (remainder.Length > 0);
			return lines;
		}

		/// <summary>
		/// Gets the next set of lines that should be shown in the text field.
		/// </summary>
		/// <param name="text">The text to be split</param>
		/// <returns>String array, containing the line to show as the first element, and
		/// the second returns the remaining lines.</returns>
		public string[] GetNextLines(string text) {
			string[] lines = text.Split('\n');
			List<string> currentLines = new List<string>();
			int numberOfLines = 0;
			int i = 0;
			string remainder = "";
			for (; i < lines.Length && numberOfLines < TotalLines; i++) {
				string sentence = lines[i].Trim();
				TokenizedText tokenizedText = new TokenizedText(sentence);
				int[] indices = LineBreakIndices(tokenizedText.GetTextWithoutMarkup());

				// This line of text goes beyond the number of lines, and is the first
				// line in the box. Add as much as we can.
				if (indices.Length > TotalLines && numberOfLines == 0) {
					int lastIdx = 0;
					int idx = 0;
					for (int j = 0; i < indices.Length && numberOfLines < TotalLines; j++) {
						idx = indices[j];
						currentLines.Add(tokenizedText.Substring(lastIdx, idx - lastIdx).GetStringWithAllTokens());
						lastIdx = idx;
						numberOfLines++;
					}
					int end = tokenizedText.GetTextWithoutMarkup().Length;
					remainder = tokenizedText.Substring(lastIdx, end - lastIdx).GetStringWithAllTokens();
					i++;
					break;
				}
				// This line of text is within the number of lines, including previous lines.
				// Include all of it.
				else if (indices.Length <= TotalLines - numberOfLines) {
					int lastIdx = 0;
					foreach (int idx in indices) {
						currentLines.Add(tokenizedText.Substring(lastIdx, idx - lastIdx).GetStringWithAllTokens());
						lastIdx = idx;
						numberOfLines++;
					}
				}
				// This line of text goes beyond the number of lines, but there are existing lines.
				// Add this to the next box so we can break it later.
				else {
					break;
				}
			}
			if (remainder.Length > 0) {
				remainder += "\n";
			}
			remainder += string.Join("\n", lines, i, lines.Length - i);
			return new string[] { remainder, string.Join("\n", currentLines.ToArray()) };
		}

		/// <summary>
		/// Returns the index at which a string should be broken for wrapping all lines.
		/// </summary>
		/// <param name="str">String to break up. All non-output characters should be removed.</param>
		/// <returns></returns>
		public int[] LineBreakIndices(string str) {
			string sentence = str;
			List<int> lineBreaks = new List<int>();
			int lastIdx = 0;
			while (sentence.Length > 0) {
				int idx = LineBreakIndex(sentence);
				lineBreaks.Add(idx + lastIdx);
				sentence = sentence.Substring(idx);
				lastIdx += idx;
			}
			return lineBreaks.ToArray();
		}

		/// <summary>
		/// Returns the index at which a string should be broken for wrapping the first line.
		/// </summary>
		/// <param name="str">Index at which this line should be broken. All non-output characters should be removed.</param>
		/// <returns>Returns the index at which the string should be broken.</returns>
		public int LineBreakIndex(string str) {
			List<int> splitIndices = new List<int>();
			char[] splitChars = new char[] { ' ' };
			char[] splitNextChars = new char[] { '-', '—', '–' };
			for (int i = str.IndexOfAny(splitChars); i > -1; i = str.IndexOfAny(splitChars, i + 1)) {
				splitIndices.Add(i);
			}
			for (int i = str.IndexOfAny(splitNextChars); i > -1; i = str.IndexOfAny(splitNextChars, i + 1)) {
				splitIndices.Add(i + 1);
			}
			splitIndices.Add(str.Length);
			splitIndices.Sort();
			splitIndices = splitIndices.Distinct().ToList();

			int lastIdx = 0;
			foreach (int idx in splitIndices) {
				if (idx > LineWidth) {
					// If the next word is longer than a line anyway, break the mid-word.
					if (idx - lastIdx > LineWidth) {
						return LineWidth;
					}
					// Return to the last split character.
					return lastIdx;
				}
				lastIdx = idx;
			}
			// Return the last index if it went to the end.
			return lastIdx;
		}

		public int NumberOfLines(string str) {
			str = Regex.Replace(str, "<[^>]*>", "");
			List<int> splitIndices = new List<int>();
			char[] splitChars = new char[] { ' ', '-' };
			for (int i = str.IndexOfAny(splitChars); i > -1; i = str.IndexOfAny(splitChars, i + 1)) {
				splitIndices.Add(i);
			}
			splitIndices.Add(str.Length);

			int lineBreak = 0;
			int lines = 1;
			for (int i = 0; i < splitIndices.Count; i++) {
				int idx = splitIndices[i];
				int lastIdx = i > 0 ? splitIndices[i - 1] : 0;
				// New line
				if (idx - lineBreak > LineWidth) {
					// This word by itself is longer than the boundary.
					if (idx - lastIdx >= LineWidth) {
						lineBreak += LineWidth;
						lines++;
						i--;
						continue;
					}

					lineBreak = lastIdx;
					lines++;
				}
			}
			return lines;
		}
	}
}
