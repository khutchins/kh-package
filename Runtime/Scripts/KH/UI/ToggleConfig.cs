using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public class ToggleConfig : PanelObjectConfig {
		public readonly string DisplayText;
		public readonly bool IsOn;
		public readonly TogglePressedHandler Handler;

		public ToggleConfig(string key, string displayText, bool isOn, System.Action<GameObject> creationCallback, TogglePressedHandler handler) : base(key, creationCallback) {
			DisplayText = displayText;
			IsOn = isOn;
			Handler = handler;
		}
	}
}