using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Input;
using KH.Text;

namespace KH.Interact {
	public class Talkable : TalkableBase {

		public TalkCycleType CycleType = TalkCycleType.RepeatLast;
		[TextArea]
		public string[] Lines;

		public override TalkCycleType TalkCycle => TalkCycle;

		public override string[] TalkLines => Lines;
	}
}