using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Input {
	[CreateAssetMenu(menuName = "KH/Input/Unity Single Input")]
	public class UnitySingleInputMediator : SingleInputMediator {
		public string Input = "";

		public override bool InputDown() {
			return UnityEngine.Input.GetButton(Input);
		}

		public override bool InputJustDown() {
			return UnityEngine.Input.GetButtonDown(Input);
		}
	}
}
