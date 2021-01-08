using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms.BaseAtoms;

namespace KH.Actions {
	public class ActionUpdateInt : Action {

		public IntVariable reference;
		public int newValue;

		public override void Begin() {
			reference.Value = newValue;
			Finished();
		}
	}
}