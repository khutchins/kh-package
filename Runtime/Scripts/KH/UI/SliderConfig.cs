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

		public new class Builder : PanelObjectConfig.Builder {
			private string _displayText;
			private SliderUpdatedHandler _handler;
			private float _minValue;
			private float _maxValue;
			private float _defaultValue;

			public Builder(string key, float minValue, float maxValue, float defaultValue) : base(key) {
				_minValue = minValue;
				_maxValue = maxValue;
				_defaultValue = defaultValue;
			}

			public Builder SetDisplayText(string displayText) {
				_displayText = displayText;
				return this;
			}

			public Builder SetSliderUpdatedHandler(SliderUpdatedHandler handler) {
				_handler = handler;
				return this;
			}

			public override PanelObjectConfig Build() {
				return new SliderConfig(_key, _displayText, _minValue, _maxValue, _defaultValue, _creationCallback, _handler);
			}
		}
	}
}