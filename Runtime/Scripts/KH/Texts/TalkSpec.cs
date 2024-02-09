using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static KH.Interact.TalkableBase;

namespace KH.Texts {
	[CreateAssetMenu(menuName = "KH/Text/Talk Spec")]
	public class TalkSpec : ScriptableObject {

		public TalkCycleType TalkCycle = TalkCycleType.RepeatLast;
		[TextArea]
		public string[] TalkLines;

		public string GetTalkLine(int idx) {
			if (idx >= TalkLines.Length) {
				switch (TalkCycle) {
					case TalkCycleType.Repeat:
						return TalkLines[idx % TalkLines.Length];
					case TalkCycleType.RepeatLast:
						return TalkLines[TalkLines.Length - 1];
					default:
					case TalkCycleType.StopAfterLast:
						return null;
				}
			}
			return TalkLines[idx];
		}
	}
}