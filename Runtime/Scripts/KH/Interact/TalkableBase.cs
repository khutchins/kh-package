using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Texts;
using KH.Input;

namespace KH.Interact {
	public abstract class TalkableBase : Interactable {

		public SingleInputMediator InputMediator;
		public string TextAnimatorKey = "Shared";
		private bool _textFinished = false;
		private Interactor _interactor;
		private bool _allowClose = false;
		protected TextAnimator textAnimator;

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
			textAnimator = TextAnimator.Animators.ContainsKey(TextAnimatorKey) ? TextAnimator.Animators[TextAnimatorKey] : null;
			if (textAnimator == null) {
				Debug.Log("No text animator exists for key " + TextAnimatorKey);
			}
			ForbidInteraction = TalkLines.Length == 0;
		}

		void LateUpdate() {
			// Don't process input if paused.
			if (Time.deltaTime == 0) return;

			if (_interactor != null && InputMediator.InputJustDown() && _textFinished && _allowClose) {
				textAnimator.TextAnimateOutFinished += TextAnimator_TextAnimateOutFinished;
				_allowClose = false;
				textAnimator.RemoveText();
			}
		}

		private void TextAnimator_TextAnimateOutFinished() {
			textAnimator.TextAnimateOutFinished -= TextAnimator_TextAnimateOutFinished;
			ForceStopInteraction(_interactor);
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
			_textFinished = false;
			_allowClose = false;
			_interactor = interactor;

			string talkLine = GetTalkLine();
			if (talkLine == null) {
				ForceStopInteraction();
				return;
			}
			textAnimator.SoundLocation = this.transform;
			textAnimator.PlayText(null, Color.white, talkLine);
			textAnimator.TextFinished += TextFinishedListener;
		}

		protected override void StopInteractingInner(Interactor interactor) {
			_interactor = null;
			textAnimator.RemoveText();
			textAnimator.TextFinished -= TextFinishedListener;
		}

		public void TextFinishedListener(bool shouldPlayNextText) {
			_textFinished = true;
			_allowClose = true;
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