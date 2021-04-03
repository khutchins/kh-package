using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.References;

namespace KH.Actions {
	public class ActionFader : Action {

		public enum FaderAction {
			FadeIn,
			FadeOut
		}

		public FloatReference FaderRef;
		public FaderAction FaderActionToTake;
		public float FadeDuration = 1F;

		public override void Begin() {
			if (FaderRef == null) {
				Debug.LogWarning("No Fader present for ActionFadeOutFader");
				Finished();
				return;
			}

			StartCoroutine(ActionCoroutineWithFinished());
		}

		private IEnumerator ActionCoroutineWithFinished() {
			yield return StartCoroutine(ActionCoroutine(this, FaderRef, FaderActionToTake, FadeDuration));
			Finished();
		}

		public IEnumerator ActionCoroutine() {
			yield return StartCoroutine(ActionCoroutine(this, FaderRef, FaderActionToTake, FadeDuration));
		}

		public static IEnumerator ActionCoroutine(MonoBehaviour coroutineObject, FloatReference fader, FaderAction action, float duration) {
			if (fader == null) {
				Debug.LogWarning("No Fader present for ActionFadeOutFader");
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
