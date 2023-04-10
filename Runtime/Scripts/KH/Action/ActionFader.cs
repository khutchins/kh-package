using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

namespace KH.Actions {
	public class ActionFader : Action {

		public enum FaderAction {
			FadeIn,
			FadeOut,
			SnapIn,
			SnapOut,
		}

		public FloatReference FaderRef;
		[Tooltip("Reference to color used on the fader. If null, will not set the fader's color.")]
		public ColorReference FaderColorRef;
		public Color Color;
		public FaderAction FaderActionToTake;
		public float FadeDuration = 1F;

		public override void Begin() {
			if (FaderRef == null) {
				Debug.LogWarning("No Fader present for ActionFadeOutFader");
				Finished();
				return;
			}

			FaderColorRef?.SetValue(Color);

			StartCoroutine(ActionCoroutineWithFinished());
		}

		private IEnumerator ActionCoroutineWithFinished() {
			yield return StartCoroutine(ActionCoroutine(FaderRef, FaderActionToTake, FadeDuration));
			Finished();
		}

		public static IEnumerator ActionCoroutine(FloatReference fader, FaderAction action, float duration) {
			if (fader == null) {
				Debug.LogWarning("No Fader present for ActionFadeOutFader");
				yield break;
			} else if (action == FaderAction.SnapIn) {
				fader.Value = 1;
				yield break;
            } else if (action == FaderAction.SnapOut) {
				fader.Value = 0;
				yield break;
            }

			float from = action == FaderAction.FadeIn ? 0 : 1;
			float to = action == FaderAction.FadeIn ? 1 : 0;
			float start = Time.time;

			while (start + duration > Time.time) {
				fader.Value = from + ((Time.time - start) / duration) * (to - from);
				yield return null;
			}
			fader.Value = to;
		}
	}
}
