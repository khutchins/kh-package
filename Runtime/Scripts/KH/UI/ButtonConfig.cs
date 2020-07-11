using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public class ButtonConfig : PanelObjectConfig {
		public string DisplayText;
		public ButtonPressedHandler Handler;

		public ButtonConfig(string key, string displayText, System.Action<GameObject> creationCallback, ButtonPressedHandler handler) : base(key, creationCallback) {
			DisplayText = displayText;
			Handler = handler;
		}
	}
}