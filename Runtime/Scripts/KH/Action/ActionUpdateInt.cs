using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Ratferences;

namespace KH.Actions {
	public class ActionUpdateInt : Action {

		public IntReference reference;
		public int newValue;

		public override void Begin() {
			reference.Value = newValue;
			Finished();
		}
	}
}