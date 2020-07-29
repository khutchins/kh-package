using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
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

		public new class Builder : PanelObjectConfig.Builder {
			private string _displayText;
			private List<string> _optionStrings = new List<string>();
			private int _defaultIndex;
			private DropdownChosenHandler _handler;

			public Builder(string key) : base(key) {
			}

			public Builder SetDisplayText(string displayText) {
				_displayText = displayText;
				return this;
			}

			public Builder SetDropdownChosenHandler(DropdownChosenHandler handler) {
				_handler = handler;
				return this;
			}

			public Builder AddOptionStrings(IEnumerable<string> options) {
				_optionStrings.AddRange(options);
				return this;
			}

			public Builder AddOptionString(string option, bool defaultOption = false) {
				int newIdx = _optionStrings.Count;
				_optionStrings.Add(option);
				if (defaultOption) {
					_defaultIndex = newIdx;
				}
				return this;
			}

			public Builder SetDefaultOptionIndex(int idx) {
				_defaultIndex = idx;
				return this;
			}

			public override PanelObjectConfig Build() {
				return new DropdownConfig(_key, _displayText, _optionStrings.ToArray(), _defaultIndex, _creationCallback, _handler);
			}
		}
	}
}