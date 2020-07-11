using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public class DropdownConfig : PanelObjectConfig {
		public readonly string DisplayText;
		public readonly string[] OptionStrings;
		public readonly int DefaultIndex;
		public readonly DropdownChosenHandler Handler;

		public DropdownConfig(string key, string displayText, string[] optionStrings, int defaultIndex, System.Action<GameObject> creationCallback, DropdownChosenHandler handler) : base(key, creationCallback) {
			DisplayText = displayText;
			OptionStrings = optionStrings;
			DefaultIndex = defaultIndex;
			Handler = handler;
		}
	}
}