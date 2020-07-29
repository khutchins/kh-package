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

		public new class Builder : PanelObjectConfig.Builder {
			private string _displayText;
			private ButtonPressedHandler _handler;

			public Builder(string key) : base(key) {
			}

			public Builder SetDisplayText(string displayText) {
				_displayText = displayText;
				return this;
			}

			public Builder SetButtonPressedHandler(ButtonPressedHandler handler) {
				_handler = handler;
				return this;
			}

			public override PanelObjectConfig Build() {
				return new ButtonConfig(_key, _displayText, _creationCallback, _handler);
			}
		}
	}
}