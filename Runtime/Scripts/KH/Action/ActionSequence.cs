using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionSequence : Action {
		public List<Action> Actions;

		private int idx = 0;

		public override void Begin() {
			idx = 0;

			BeginIndex();
		}

		private void BeginIndex() {
			if (idx >= Actions.Count) {
				//Debug.Log("Done with actions");
				Finished();
			} else {
				//Debug.Log("Beginning action " + idx + " (" + Actions[idx] + ") at " + Time.time);
				Actions[idx].FinishedAction += ActionFinished;
				Actions[idx].Begin();
			}
		}

		private void ActionFinished(Action action) {
			action.FinishedAction -= ActionFinished;
			idx++;
			BeginIndex();
		}
	}
}