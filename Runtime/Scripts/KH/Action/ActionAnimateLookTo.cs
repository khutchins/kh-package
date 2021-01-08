using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionAnimateLookTo : Action {

		public float Duration = 0.5F;
		public Transform TransformToRotate;
		public Transform LookTarget;
		public bool Blocking = false;

		public override void Begin() {
			StartCoroutine(LookTo());
			if (!Blocking) {
				Finished();
			}
		}

		IEnumerator LookTo() {
			Quaternion start = TransformToRotate.rotation;
			Quaternion end = Quaternion.LookRotation(LookTarget.position - TransformToRotate.position, new Vector3(0, 1, 0));

			float startTime = Time.time;

			while (Time.time < startTime + Duration) {
				float percent = (Time.time - startTime) / Duration;
				TransformToRotate.rotation = Quaternion.Lerp(start, end, AnimationCurves.CubicEaseInOut(percent));

				yield return null;
			}
			TransformToRotate.rotation = end;

			if (Blocking) {
				Finished();
			}
		}
	}
}