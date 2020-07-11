using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public class PanelConfig {
		/// <summary>
		/// Used to reference the panels from other panels or do
		/// lookups in the panel dictionary.
		/// </summary>
		public string Key;
		public string DefaultSelectableKey;
		public bool HideMenuDecoration;

		public PanelObjectConfig[] PanelObjects;
		[HideInInspector]
		public GameObject[] SupplementalObjects;

		public PanelConfig(string key, string defaultSelectableKey, PanelObjectConfig[] panelObjects, GameObject[] supplementalObjects = null, bool hideMenuDecoration = false) {
			Key = key;
			DefaultSelectableKey = defaultSelectableKey;
			PanelObjects = panelObjects;
			HideMenuDecoration = hideMenuDecoration;
			SupplementalObjects = supplementalObjects;
		}
	}
}