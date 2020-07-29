using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using UnityEngine;

namespace KH.UI {
	public class PanelConfig {
		/// <summary>
		/// Used to reference the panels from other panels or do
		/// lookups in the panel dictionary.
		/// </summary>
		public readonly string Key;
		public readonly string DefaultSelectableKey;
		public readonly bool HideMenuDecoration;
		public readonly bool HorizontalMenu;
		public readonly GameObject PrefabOverride;

		public readonly PanelObjectConfig[] PanelObjects;
		[HideInInspector]
		public readonly GameObject[] SupplementalObjects;

		public PanelConfig(string key, string defaultSelectableKey, PanelObjectConfig[] panelObjects, GameObject[] supplementalObjects = null, bool hideMenuDecoration = false, bool horizontalMenu = false, GameObject prefabOverride = null) {
			Key = key;
			DefaultSelectableKey = defaultSelectableKey;
			PanelObjects = panelObjects;
			HideMenuDecoration = hideMenuDecoration;
			SupplementalObjects = supplementalObjects;
			HorizontalMenu = horizontalMenu;
			PrefabOverride = prefabOverride;
		}

		public class Builder {
			private List<PanelObjectConfig> _panelObjectConfigs = new List<PanelObjectConfig>();
			private List<GameObject> _supplementalObjects = new List<GameObject>();
			private GameObject _prefabOverride;
			private string _key;
			private string _defaultSelectableKey;
			private bool _isHorizontal;
			private bool _hideMenuDecoration;

			public Builder(string key) {
				_key = key;
			}

			public Builder AddSupplementalObject(GameObject obj) {
				_supplementalObjects.Add(obj);
				return this;
			}

			public Builder AddSupplementalObjects(IEnumerable<GameObject> objs) {
				_supplementalObjects.AddRange(objs);
				return this;
			}

			public Builder AddPanelObject(PanelObjectConfig config, bool defaultObject = false) {
				_panelObjectConfigs.Add(config);
				if (defaultObject) {
					_defaultSelectableKey = config.Key;
				}
				return this;
			}

			public Builder AddPanelObject(PanelObjectConfig.Builder configBuilder, bool defaultObject = false) {
				PanelObjectConfig config = configBuilder.Build();
				_panelObjectConfigs.Add(config);
				if (defaultObject) {
					_defaultSelectableKey = config.Key;
				}
				return this;
			}

			public Builder InsertPanelObject(PanelObjectConfig config, int index, bool defaultObject = false) {
				_panelObjectConfigs.Insert(index, config);
				if (defaultObject) {
					_defaultSelectableKey = config.Key;
				}
				return this;
			}

			public Builder InsertPanelObject(PanelObjectConfig.Builder configBuilder, int index, bool defaultObject = false) {
				PanelObjectConfig config = configBuilder.Build();
				_panelObjectConfigs.Insert(index, config);
				if (defaultObject) {
					_defaultSelectableKey = config.Key;
				}
				return this;
			}

			public Builder SetPrefabOverride(GameObject prefabOverride) {
				_prefabOverride = prefabOverride;
				return this;
			}

			public Builder SetIsHorizontalMenu(bool isHorizontal) {
				_isHorizontal = isHorizontal;
				return this;
			}

			public Builder SetHideMenuDecoration(bool hideMenuDecoration) {
				_hideMenuDecoration = hideMenuDecoration;
				return this;
			}

			public PanelConfig Build() {
				if (_defaultSelectableKey == null) {
					_defaultSelectableKey = _panelObjectConfigs[0].Key;
				}
				return new PanelConfig(_key, _defaultSelectableKey, 
					_panelObjectConfigs.ToArray(), _supplementalObjects.ToArray(), 
					_hideMenuDecoration, _isHorizontal, _prefabOverride);
			}
		}
	}
}