using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionLockViewTo : Action {

		public float Duration;
		public bool Blocking;
		public Transform TransformToLock;
		public Transform LookTarget;
		public bool LockX = false;
		public bool LockY = false;
		public bool LockZ = false;

		public override void Begin() {
			if (!Blocking) {
				Finished();
			}

			StartCoroutine(LockView());
		}


		IEnumerator LockView() {
			float startTime = Time.time;

			while (Time.time < startTime + Duration) {

				Quaternion start = TransformToLock.rotation;
				Quaternion end = Quaternion.LookRotation(LookTarget.position - TransformToLock.position, new Vector3(0, 1, 0));

				Quaternion current = Quaternion.Lerp(start, end, 2f * Time.deltaTime);
				Vector3 euler = current.eulerAngles;
				if (LockX) {
					euler.x = 0;
				}
				if (LockY) {
					euler.y = 0;
				}
				if (LockZ) {
					euler.z = 0;
				}
				TransformToLock.eulerAngles = euler;

				yield return null;
			}

			if (Blocking) {
				Finished();
			}
			Debug.Log("Stopped locking");
		}
	}
}