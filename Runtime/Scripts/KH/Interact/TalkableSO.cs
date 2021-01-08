using Rewired;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public class TalkableSO : TalkableBase {

		public TalkSpec TalkSpec;

		public override TalkCycleType TalkCycle => TalkSpec.TalkCycle;

		public override string[] TalkLines => TalkSpec.TalkLines;

		public new void Start() {
			if (TalkSpec == null) {
				Debug.LogWarning("No TalkSpec set on " + gameObject.name + "!");
			}
			base.Start();
		}
	}
}