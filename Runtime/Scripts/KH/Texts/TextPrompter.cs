using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Input;

namespace KH.Texts {
    /// <summary>
	/// Reads LineSpecs from a ScriptableObject and passes them along to a TextAnimator on the same object.
    /// Only one TextPrompter should service a TextAnimator, or else race conditions may occur.
	/// </summary>
	[RequireComponent(typeof(TextAnimator))]
    public class TextPrompter : MonoBehaviour {
		[Tooltip("The line spec queue the text prompter will read from. Must match that of the line spec sources.")]
        public LineSpecQueue LineQueue;
		[Tooltip("An input mediator that has the Interact function set.")]
		public SingleInputMediator InteractMediator;

        private TextAnimator _textAnimator;
		private LineSpec _current;
		private bool _waitingForInput;

        // Start is called before the first frame update
        void Awake() {
            _textAnimator = GetComponent<TextAnimator>();
			LineQueue.Clear();
        }

		private void OnEnable() {
			LineQueue.FirstLineAdded += LineQueue_FirstLineAdded;
			_textAnimator.TextFinished += _textAnimator_TextFinished;
		}

		private void OnDisable() {
			LineQueue.FirstLineAdded -= LineQueue_FirstLineAdded;
			_textAnimator.TextFinished += _textAnimator_TextFinished;
		}

		private void TryPlayNextLine() {
			if (_textAnimator.TextAnimating) return;
			LineSpec nextLine = LineQueue.Dequeue();
			if (nextLine == null) return;
			if (_waitingForInput) return;

			_current = nextLine;
			_textAnimator.PlayText(nextLine.Speaker, Color.white, nextLine.Line);
		}

		private void _textAnimator_TextFinished(bool shouldPlayNextText) {
			if (shouldPlayNextText) {
				DoneWithLine();
			} else {
				_waitingForInput = true;
			}
		}

		private void Update() {
			if (_waitingForInput && InteractMediator != null && InteractMediator.InputJustDown()) {
				DoneWithLine();
			}
		}

		private void DoneWithLine() {
			_textAnimator.RemoveText();
			_current?.Callback?.Invoke();
			_current = null;
			_waitingForInput = false;
			TryPlayNextLine();
		}

		private void LineQueue_FirstLineAdded() {
			TryPlayNextLine();
		}
	}
}