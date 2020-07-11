using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public class SliderConfig : PanelObjectConfig {
		public string DisplayText;
		public float MinValue;
		public float MaxValue;
		public float DefaultValue;
		public SliderUpdatedHandler Handler;

		public SliderConfig(string key, string displayText, float minValue, float maxValue, float defaultValue, System.Action<GameObject> creationCallback, SliderUpdatedHandler handler) : base(key, creationCallback) {
			DisplayText = displayText;
			MinValue = minValue;
			MaxValue = maxValue;
			DefaultValue = defaultValue;
			Handler = handler;
		}
	}
}