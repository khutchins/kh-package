using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionParallel : Action {

		public List<Action> Actions;

		private List<Action> awaitingActions;

		public override void Begin() {
			awaitingActions = new List<Action>(Actions);
			foreach (Action action in Actions) {
				action.FinishedAction += ActionFinished;
			}
		}

		private void ActionFinished(Action action) {
			awaitingActions.Remove(action);
			action.FinishedAction -= ActionFinished;
			CheckForFinish();
		}

		private void CheckForFinish() {
			if (awaitingActions.Count == 0) {
				Finished();
			}
		}
	}
}