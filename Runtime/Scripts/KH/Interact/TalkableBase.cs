using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Texts;
using KH.Input;

namespace KH.Interact {
	public abstract class TalkableBase : Interactable {

		public LineSpecQueue LineSpecQueue;

		private int _currentTalkLine = 0;

		public abstract TalkCycleType TalkCycle { get; }
		public abstract string[] TalkLines { get; }

		public override bool LocksMouse() {
			return true;
		}

		public override bool LocksMovement() {
			return true;
		}

		public override bool ExclusiveInteraction() {
			return true;
		}

		public void Start() {
			ForbidInteraction = TalkLines.Length == 0;
		}

		void LinesFinished() {
			ForceStopInteraction();
		}

		protected virtual string GetTalkLine() {
			string line = GetTalkLine(_currentTalkLine);
			_currentTalkLine++;
			return line;
		}

		public override bool ShouldAllowInteraction(Interactor interactor) {
			return GetTalkLine(_currentTalkLine) != null;
		}

		protected override void StartInteractingInner(Interactor interactor) {
			string talkLine = GetTalkLine();
			if (talkLine == null) {
				ForceStopInteraction();
				return;
			}

			LineSpecQueue.Enqueue(new LineSpec("", talkLine, LinesFinished));
		}

		protected override void StopInteractingInner(Interactor interactor) {
			
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