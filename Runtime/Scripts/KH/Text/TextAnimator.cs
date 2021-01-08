using KH.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace KH.Text {
	public delegate void TextFinishedHandler(bool shouldPlayNextText);
	public delegate void TextAnimateOutFinishedHandler();

	public class TextAnimator : MonoBehaviour {

		public enum AnimationTypes {
			SlideInOut,
			Instant
		}

		public AudioClip blipSound;
		public RectTransform rectToShake;
		public TextMeshProUGUI conversationText;
		public TextMeshProUGUI speaker;
		public GameObject activeTextBox;
		public GameObjectReference PlayerRef;
		public bool TextAnimating;
		public string AnimatorKey;
		public static Dictionary<string, TextAnimator> Animators = new Dictionary<string, TextAnimator>();
		public static TextAnimator SharedAnimator;
		public Transform SoundLocation;
		public bool PlayAtAudioListener;

		public RectTransform[] RectsToAnimate;
		private Vector2[] StandardRectPositions;
		public AnimationTypes AnimationType;

		public bool AllowSkipping = true;

		private TextPlayer _currentText;
		private Coroutine _textCoroutine;
		private Vector3 _textBoxBasePosition;

		private float _timeStarted;
		private bool _doneNextUpdate;

		public event TextFinishedHandler TextFinished;
		public event TextAnimateOutFinishedHandler TextAnimateOutFinished;

		public InputMediator InputMediator;

		void Awake() {
			Animators[AnimatorKey] = this;
			if (AnimatorKey == "Shared") {
				SharedAnimator = this;
			}
			TextAnimating = false;
			activeTextBox.SetActive(false);
		}

		void Start() {
			List<Vector2> poses = new List<Vector2>();
			for (int i = 0; i < RectsToAnimate.Length; i++) {
				Vector2 pos = RectsToAnimate[i].anchoredPosition;
				poses.Add(pos);
			}
			StandardRectPositions = poses.ToArray();
		}

		public void PlayText(string speakerName, Color nameColor, string rawText, float speedMod = .8F) {
			if (speaker) {
				speaker.text = speakerName;
				speaker.color = nameColor;
			}
			activeTextBox.SetActive(true);
			StopAllCoroutines();
			_timeStarted = Time.time;
			_textCoroutine = StartCoroutine(AnimateText(rawText, speedMod));
		}

		public void StopText() {
			StopCoroutine(_textCoroutine);
			if (_currentText != null) {
				conversationText.text = _currentText.GetFinalString();
			}
			rectToShake.anchoredPosition = _textBoxBasePosition;
			TextAnimating = false;
			_currentText = null;
		}

		public void RemoveText() {
			rectToShake.anchoredPosition = _textBoxBasePosition;
			StopAllCoroutines();
			StartCoroutine(AnimateOut());
		}

		IEnumerator AnimateIn() {
			if (RectsToAnimate.Length > 0) {

				List<Vector2> endPositions = new List<Vector2>();
				for (int i = 0; i < RectsToAnimate.Length; i++) {
					endPositions.Add(StandardRectPositions[i]);

					// Make sure they're offscreen
					RectsToAnimate[i].anchoredPosition = new Vector2(0, -1000);
				}

				// Have to yield here to make sure we get the correct text box heights.
				yield return null;

				if (AnimationType == AnimationTypes.SlideInOut) {
					float animLength = 0.2F;
					float animGap = 0.2F;

					float start = Time.time;
					List<Vector2> startPositions = new List<Vector2>();
					for (int i = 0; i < RectsToAnimate.Length; i++) {
						Vector2 pos = endPositions[i];
						startPositions.Add(new Vector2(pos.x, 0 - RectsToAnimate[i].rect.height));
					}
					while (Time.time - start < animLength + animGap * (RectsToAnimate.Length - 1)) {
						for (int i = 0; i < RectsToAnimate.Length; i++) {
							RectsToAnimate[i].anchoredPosition = Vector2.Lerp(startPositions[i], endPositions[i], AnimationCurves.CubicEaseOut(CoroutineHelpers.PercentRange(start + animGap * i, start + animGap * i + animLength, Time.time)));
						}
						yield return null;
					}
				}

				for (int i = 0; i < RectsToAnimate.Length; i++) {
					RectsToAnimate[i].anchoredPosition = endPositions[i];
				}
			}
		}

		IEnumerator AnimateOut() {
			if (RectsToAnimate.Length > 0) {

				List<Vector2> endPositions = new List<Vector2>();
				for (int i = 0; i < RectsToAnimate.Length; i++) {
					endPositions.Add(StandardRectPositions[i]);
				}

				// Have to yield here to make sure we get the correct text box heights.
				yield return null;

				List<Vector2> startPositions = new List<Vector2>();
				for (int i = 0; i < RectsToAnimate.Length; i++) {
					Vector2 pos = endPositions[i];
					startPositions.Add(new Vector2(pos.x, 0 - RectsToAnimate[i].rect.height));
				}

				if (AnimationType == AnimationTypes.SlideInOut) {
					float animLength = 0.2F;
					float animGap = 0.2F;
					float start = Time.time;
					
					while (Time.time - start < animLength + animGap * (RectsToAnimate.Length - 1)) {
						for (int i = 0; i < RectsToAnimate.Length; i++) {
							int offset = RectsToAnimate.Length - 1 - i;
							RectsToAnimate[i].anchoredPosition = Vector2.Lerp(endPositions[i], startPositions[i], AnimationCurves.CubicEaseIn(CoroutineHelpers.PercentRange(start + animGap * offset, start + animGap * offset + animLength, Time.time)));
						}
						yield return null;
					}
				}

				for (int i = 0; i < RectsToAnimate.Length; i++) {
					RectsToAnimate[i].anchoredPosition = startPositions[i];
				}
			}

			activeTextBox.SetActive(false);

			TextAnimateOutFinished?.Invoke();
		}

		IEnumerator AnimateText(string rawText, float baseSpeedMod) {
			activeTextBox.SetActive(true);
			TextAnimating = true;
			_currentText = new TextPlayer(rawText, baseSpeedMod);

			conversationText.text = "";

			yield return StartCoroutine(AnimateIn());

			_textBoxBasePosition = rectToShake.anchoredPosition;

			bool bypassKeypress = false;

			foreach (TextUpdate update in _currentText) {
				// Player has manually skipped text.
				if (_doneNextUpdate) break;

				conversationText.text = update.NewString;
				foreach (TextToken token in update.UnrecognizedTags) {
					switch (token.key) {
						case "shake":
							StartCoroutine(Shake(OptParse(token.value, .1F)));
							break;
					}
				}

				if (blipSound) {
					Transform position = null;
					if (PlayAtAudioListener && PlayerRef != null && PlayerRef.Value != null) {
						position = PlayerRef.Value.transform;
					} else if (SoundLocation) {
						position = SoundLocation;
					}
					if (position != null) {
						AudioSource.PlayClipAtPoint(blipSound, position.position);
					}
				}

				if (update.Delay > 0) {
					yield return new WaitForSeconds(update.Delay);
				}

				if (update.BypassKeypress) {
					bypassKeypress = true;
				}
			}

			StopText();
			TextFinished?.Invoke(bypassKeypress);
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

		IEnumerator Shake(float duration) {
			float startTime = Time.time;
			float magnitude = 5;
			while (Time.time - startTime < duration) {
				float rand = Random.Range(0F, (float)(2F * System.Math.PI));
				rectToShake.anchoredPosition = _textBoxBasePosition + new Vector3((float)(magnitude * System.Math.Sin(rand)), (float)(magnitude * System.Math.Cos(rand)), 0);
				yield return new WaitForSeconds(0.02F);
			}

			rectToShake.anchoredPosition = _textBoxBasePosition;
		}

		void Update() {
			// Don't process input if paused.
			if (Time.deltaTime == 0) return;

			// We don't want to trigger the start and stop on the same frame,
			// so disallow it.
			bool sameFrame = _timeStarted == Time.time;
			if (!_doneNextUpdate) {
				if (AllowSkipping && !sameFrame && InputMediator.Interact()) {
					if (TextAnimating) {
						_doneNextUpdate = true;
					}
				}
			} else {
				_doneNextUpdate = false;
				if (TextAnimating) {
					StopText();
					TextFinished?.Invoke(false);
				}
			}
		}
	}
}