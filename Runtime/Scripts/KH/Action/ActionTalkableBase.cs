using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Texts;

namespace KH.Actions {
	public abstract class ActionTalkableBase : Action {

		public LineSpecQueue LineSpecQueue;

		public abstract TalkCycleType TalkCycle { get; }
		public abstract string[] TalkLines { get; }

		private int _currentTalkLine = 0;

		public override void Begin() {
			string talkLine = GetTalkLine();
			// No dialogue left.
			if (talkLine == null) {
				Finished();
				return;
			}

			LineSpec spec = new LineSpec("", talkLine, LinesFinished);
			LineSpecQueue.Enqueue(spec);
		}

		void LinesFinished() {
			Finished();
		}

		protected virtual string GetTalkLine() {
			string line = GetTalkLine(_currentTalkLine);
			_currentTalkLine++;
			return line;
		}

		public string GetTalkLine(int idx) {
			string[] talkLines = TalkLines;
			TalkCycleType type = TalkCycle;
			if (idx >= talkLines.Length) {
				switch (type) {
					case TalkCycleType.Repeat:
						return talkLines[idx % talkLines.Length];
					case TalkCycleType.RepeatLast:
						return talkLines[talkLines.Length - 1];
					default:
					case TalkCycleType.StopAfterLast:
						return null;
				}
			}
			return talkLines[idx];
		}
	}
}