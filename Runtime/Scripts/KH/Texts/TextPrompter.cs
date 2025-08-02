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
		[Tooltip("What percent speed to play the text at.")]
		public float SpeedModifier = 1f;
        [Tooltip("Whether or not it should automatically skip to the next line.")]
        [SerializeField] bool Autoplay = false;

        private TextAnimator _textAnimator;
		private LineSpec _current;
		private bool _waitingForInput;
		private SingleCoroutineManager _coroutineManager;

        // Start is called before the first frame update
        void Awake() {
            _textAnimator = GetComponent<TextAnimator>();
			_coroutineManager = new SingleCoroutineManager(this);
			LineQueue.Clear();
        }

		private void OnEnable() {
			LineQueue.OnFirstItemAdded += LineQueue_FirstLineAdded;
			_textAnimator.TextFinished += _textAnimator_TextFinished;
		}

		private void OnDisable() {
			LineQueue.OnFirstItemAdded -= LineQueue_FirstLineAdded;
			_textAnimator.TextFinished += _textAnimator_TextFinished;
		}

		private void TryPlayNextLine() {
			if (_textAnimator.TextAnimating) return;
			LineSpec nextLine = LineQueue.Dequeue();
			if (nextLine == null) return;
			if (_waitingForInput) return;

			_current = nextLine;
			_textAnimator.PlayText(nextLine.Speaker, nextLine.SpeakerColor, nextLine.Line, SpeedModifier);
		}

		private void _textAnimator_TextFinished(bool shouldPlayNextText) {
			if (shouldPlayNextText) {
				DoneWithLine();
			} else {
				_waitingForInput = true;
				if (Autoplay) {
					_coroutineManager.StartCoroutine(WaitForLine(_current.Line.Length * 0.05f + 2f)); ;
				}
			}
		}

		private IEnumerator WaitForLine(float duration) {
			yield return new WaitForSeconds(duration);
			DoneWithLine();
		}

		private void Update() {
			if (_waitingForInput && InteractMediator != null && InteractMediator.InputJustDown()) {
				DoneWithLine();
			}
		}

		private void DoneWithLine() {
			_coroutineManager.StopCoroutine();
			_textAnimator.RemoveText();
			_current.LineFinished();
			_current = null;
			_waitingForInput = false;
			TryPlayNextLine();
		}

		private void LineQueue_FirstLineAdded() {
			TryPlayNextLine();
		}
	}
}