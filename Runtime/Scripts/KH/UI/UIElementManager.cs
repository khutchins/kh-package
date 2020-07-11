using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KH.UI {
	public class UIElementManager : MonoBehaviour {
		public GameObject SelectableObject;

		public virtual void SetColors(PaletteConfig config) {
			if (config != null) {
				foreach (Selectable select in GetComponentsInChildren<Selectable>()) {
					config.ApplyToSelectable(select);
				}
				foreach (HighlightTextWhenSelected highlight in GetComponentsInChildren<HighlightTextWhenSelected>()) {
					highlight.SelectColor = config.SelectedColor;
				}
			}
		}
	}
}
