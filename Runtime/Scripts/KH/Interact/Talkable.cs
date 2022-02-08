using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Input;
using KH.Texts;

namespace KH.Interact {
	public class Talkable : TalkableBase {

		public TalkCycleType CycleType = TalkCycleType.RepeatLast;
		[TextArea]
		public string[] Lines;

		public override TalkCycleType TalkCycle => CycleType;

		public override string[] TalkLines => Lines;
	}
}