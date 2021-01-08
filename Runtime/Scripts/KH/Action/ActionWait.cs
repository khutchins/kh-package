using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionWait : Action {

		public float Duration = 0F;

		public override void Begin() {
			StartCoroutine(Wait());
		}

		IEnumerator Wait() {
			yield return new WaitForSeconds(Duration);
			Finished();
		}
	}
}