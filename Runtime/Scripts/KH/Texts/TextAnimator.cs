﻿using KH.Input;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Ratferences;
using KH.Audio;
using UnityEngine.Events;
using System.Globalization;

namespace KH.Texts {
	public delegate void TextFinishedHandler(bool shouldPlayNextText);
	public delegate void TextAnimateOutFinishedHandler();

	/// <summary>
	/// Handles the animation of text and related behaviors in the conversation box.
	/// </summary>
	public class TextAnimator : MonoBehaviour {

		public enum AnimationTypes {
			SlideInOut,
			Instant
		}

		public static Dictionary<string, TextAnimator> Animators = new Dictionary<string, TextAnimator>();
		public static TextAnimator SharedAnimator;

		[Header("Components")]
		public TMP_Text conversationText;
		public TMP_Text speaker;
		public GameObject activeTextBox;
		public SingleInputMediator InteractMediator;

		[Header("Config")]
		public string AnimatorKey;
		[Tooltip("Whether or not the player can skip text by pressing the interact button again.")]
		public bool AllowSkipping = true;

		[Header("Audio")]
		[Tooltip("If true, audio will play at the player's location.")]
		public bool PlayAtLocation;
		[Tooltip("The location at which the audio will be played, if PlayAtAudioListener is false.")]
		public Transform SoundLocation;
		[Tooltip("Audio event specifying the sound to use playing text.")]
		public AudioEvent blipSound;
		[Tooltip("Minimum time since last text animation time to trigger another sound.")]
		public float MinSoundInterval = 0f;

		[Header("Animation")]
		[Tooltip("The rect to shake when a shake command is in the text. If unset, will not shake.")]
		public RectTransform rectToShake;
		[Tooltip("Rects that will be animated in/out.")]
		public RectTransform[] RectsToAnimate;
		public AnimationTypes AnimationType;

		[Header("Misc")]
		[Tooltip("Reference to the string being animated WITHOUT markup. Changing this value won't change the string being animated, but it will make other people using it sad.")]
		public StringReference CurrentlyPlayingLine;
		[Tooltip("Reference to the speaker's name. Changing this value won't change the name, but it will make other people using it sad.")]
		public StringReference CurrentSpeaker;
		public BoolReference TextBoxVisible;
		[SerializeField] TextTagHandler[] TagHandlers;

		public bool TextAnimating => _textAnimating;

		private Vector2[] _standardRectPositions;
		private TextPlayer _currentText;
		private Coroutine _textCoroutine;
		private Vector3 _textBoxBasePosition;

		private float _timeStarted;
		private bool _doneNextUpdate;
		private bool _textAnimating;
		private float _lastBlip;
		private bool _requiresCallbackOnDismiss;

		public event TextFinishedHandler TextFinished;
		public event TextAnimateOutFinishedHandler TextAnimateOutFinished;

		void Awake() {
			Animators[AnimatorKey] = this;
			if (AnimatorKey == "Shared") {
				SharedAnimator = this;
			}
			_textAnimating = false;
			SetTextBoxUp(false);
			UpdateTextReference(null);
		}

		void Start() {
			List<Vector2> poses = new List<Vector2>();
			for (int i = 0; i < RectsToAnimate.Length; i++) {
				Vector2 pos = RectsToAnimate[i].anchoredPosition;
				poses.Add(pos);
			}
			_standardRectPositions = poses.ToArray();
		}

		private void SetTextBoxUp(bool isUp) {
			activeTextBox.SetActive(isUp);
			if (TextBoxVisible != null && TextBoxVisible.Value != isUp) TextBoxVisible.Value = isUp;
		}

		public void PlayText(string speakerName, Color nameColor, string rawText, float speedMod = .8F) {
			if (CurrentSpeaker != null) CurrentSpeaker.Value = speakerName;
			if (speaker) {
				speaker.text = speakerName;
				speaker.color = nameColor;
			}
			SetTextBoxUp(true);
			StopAllCoroutines();
			_timeStarted = Time.time;
			_textCoroutine = StartCoroutine(AnimateText(rawText, speedMod));
		}

		public void StopText() {
			StopCoroutine(_textCoroutine);
			if (_currentText != null) {
				string finalString = _currentText.GetFinalString();
				string finalStringNoMarkup = _currentText.GetFinalStringWithoutMarkup();
				conversationText.text = finalString;
				foreach (TextTagHandler handler in TagHandlers) {
					handler.TextCompleted(finalString, finalStringNoMarkup);
				}
				_requiresCallbackOnDismiss = true;
			}
			if (rectToShake != null) {
				rectToShake.anchoredPosition = _textBoxBasePosition;
			}
			_textAnimating = false;
			_currentText = null;
		}

		public void RemoveText() {
			if (_requiresCallbackOnDismiss) {
				foreach (TextTagHandler handler in TagHandlers) {
					handler.TextDismissed();
				}
				_requiresCallbackOnDismiss = false;
			}
			if (rectToShake != null) {
				rectToShake.anchoredPosition = _textBoxBasePosition;
			}
			UpdateTextReference(null);
			StopAllCoroutines();
			StartCoroutine(AnimateOut());
		}

		IEnumerator AnimateIn() {
			if (RectsToAnimate.Length > 0) {

				List<Vector2> endPositions = new List<Vector2>();
				for (int i = 0; i < RectsToAnimate.Length; i++) {
					endPositions.Add(_standardRectPositions[i]);

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
					endPositions.Add(_standardRectPositions[i]);
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

			SetTextBoxUp(false);

			TextAnimateOutFinished?.Invoke();
		}

		private void UpdateTextReference(string text) {
			if (CurrentlyPlayingLine != null) {
				CurrentlyPlayingLine.SetValue(text);
			}
		}

		IEnumerator AnimateText(string rawText, float baseSpeedMod) {
			SetTextBoxUp(true);
			_textAnimating = true;
			_currentText = new TextPlayer(rawText, baseSpeedMod);

			conversationText.text = "";
			string textWithoutMarkup = _currentText.GetFinalStringWithoutMarkup();
			UpdateTextReference(textWithoutMarkup);
			foreach (TextTagHandler handler in TagHandlers) {
				handler.TextStarted(_currentText.GetFinalString(), textWithoutMarkup);
			}

			// This is to make sure that the text box doesn't jump in size if
			// animating in and conversation box is using a content size fitter.
			TextUpdate first = _currentText.FirstOrDefault();
			if (first != null) {
				conversationText.text = first.NewString;
			}

			yield return StartCoroutine(AnimateIn());

			if (rectToShake != null) {
				_textBoxBasePosition = rectToShake.anchoredPosition;
			}

			bool bypassKeypress = false;
			List<TextUpdate> skippedTokens = new List<TextUpdate>();

			foreach (TextUpdate update in _currentText) {
				// Player has manually skipped text.
				if (_doneNextUpdate) {
					skippedTokens.Add(update);
					continue;
				}

				conversationText.text = update.NewString;
				float pitch = 1f;
				foreach (TextToken token in update.UnrecognizedTags) {
					switch (token.key) {
						case "shake":
							StartCoroutine(Shake(OptParse(token.value, .1F)));
							break;

						case "pitch":
							pitch = OptParse(token.value, 1f);
							break;
					}
				}
				foreach (TextTagHandler handler in TagHandlers) {
					handler.TextProgressed(update);
				}

				if (update.PlayBlip) {
					MaybePlayBlip(pitch);
				}

				if (update.Delay > 0) {
					yield return new WaitForSeconds(update.Delay);
				}

				if (update.BypassKeypress) {
					bypassKeypress = true;
				}
			}

			if (skippedTokens.Count > 0) {
				foreach (TextTagHandler handler in TagHandlers) {
					handler.TextSkipped(skippedTokens.ToArray());
				}
			}

			StopText();
			TextFinished?.Invoke(bypassKeypress);
		}

		private void MaybePlayBlip(float pitch = 1f) {
			if (_lastBlip + MinSoundInterval > Time.unscaledTime) return;

			if (PlayAtLocation && SoundLocation) {
				blipSound?.PlayClipAtPoint(SoundLocation.position, 1, pitch);
			} else {
				blipSound?.PlayOneShot(1, pitch);
			}
			_lastBlip = Time.unscaledTime;
		}

		static float OptParse(string str, float def) {
			if (str == null)
				return def;

			if (!float.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out float fl)) {
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
			if (rectToShake != null) {
				float startTime = Time.time;
				float magnitude = 5;
				while (Time.time - startTime < duration) {
					float rand = Random.Range(0F, (float)(2F * System.Math.PI));
					rectToShake.anchoredPosition = _textBoxBasePosition + new Vector3((float)(magnitude * System.Math.Sin(rand)), (float)(magnitude * System.Math.Cos(rand)), 0);
					yield return new WaitForSeconds(0.02F);
				}

				rectToShake.anchoredPosition = _textBoxBasePosition;
			}
		}

		void Update() {
			// Don't process input if paused.
			if (Time.deltaTime == 0) return;

			// We don't want to trigger the start and stop on the same frame,
			// so disallow it.
			bool sameFrame = _timeStarted == Time.time;
			if (!_doneNextUpdate) {
				if (AllowSkipping && !sameFrame && InteractMediator.InputJustDown()) {
					if (_textAnimating) {
						_doneNextUpdate = true;
					}
				}
			} else {
				_doneNextUpdate = false;
				// TODO: I don't know when this would happen.
				// Coroutines run after Update() so the text
				// animating flag should be cleared when we 
				// call StopText in the coroutine.
				if (_textAnimating) {
					StopText();
					TextFinished?.Invoke(false);
				}
			}
		}
	}
}