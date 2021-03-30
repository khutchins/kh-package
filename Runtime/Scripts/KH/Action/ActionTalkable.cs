using KH.Texts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
	public class ActionTalkable : ActionTalkableBase {
		public TalkCycleType CycleType = TalkCycleType.RepeatLast;
		[TextArea]
		public string[] Lines;

		public override TalkCycleType TalkCycle => TalkCycle;

		public override string[] TalkLines => Lines;
	}
}