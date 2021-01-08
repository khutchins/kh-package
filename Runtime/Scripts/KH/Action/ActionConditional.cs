using KH;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionConditional : Action {

		[System.Serializable]
		public struct Group {
			public Conditional Conditional;
			public Action Action;
		}

		public Group If;
		public Group[] ElseIfs;
		public Action Else;

		public override void Begin() {
			if (If.Conditional.isTrue()) {
				BeginAndListenToAction(If.Action);
				return;
			}
			foreach (Group elseIf in ElseIfs) {
				if (elseIf.Conditional.isTrue()) {
					BeginAndListenToAction(elseIf.Action);
					return;
				}
			}
			if (Else != null) {
				BeginAndListenToAction(Else);
			}
		}

		private void BeginAndListenToAction(Action action) {
			action.FinishedAction += Action_FinishedAction;
			action.Begin();
		}

		private void Action_FinishedAction(Action action) {
			action.FinishedAction -= Action_FinishedAction;
			Finished();
		}
	}
}