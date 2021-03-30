using KH.Texts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionTalkableSO : ActionTalkableBase {
		public TalkSpec TalkSpec;

		public override TalkCycleType TalkCycle => TalkSpec.TalkCycle;

		public override string[] TalkLines => TalkSpec.TalkLines;

		public void OnEnable() {
			if (TalkSpec == null) {
				Debug.LogWarning("No TalkSpec set on " + gameObject.name + "!");
			}
		}
	}
}