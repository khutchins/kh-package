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

		public new class Builder : PanelObjectConfig.Builder {
			private string _displayText;
			private TogglePressedHandler _handler;
			private bool _isOn;

			public Builder(string key) : base(key) {
			}

			public Builder SetDisplayText(string displayText) {
				_displayText = displayText;
				return this;
			}

			public Builder SetIsOn(bool isOn) {
				_isOn = isOn;
				return this;
			}

			public Builder SetTogglePressedHandler(TogglePressedHandler handler) {
				_handler = handler;
				return this;
			}

			public override PanelObjectConfig Build() {
				return new ToggleConfig(_key, _displayText, _isOn, _creationCallback, _handler);
			}
		}
	}
}