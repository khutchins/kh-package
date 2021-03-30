using KH.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KH.UI {
	public class MenuGenerator : MonoBehaviour {

		[Tooltip("Canvas element to be used as the container for panels.")]
		public GameObject Parent;
		[Tooltip("Panel prefab that menu prefabs are placed in.")]
		public GameObject MenuObjectPrefab;
		public GameObject ButtonPrefab;
		public GameObject SliderPefab;
		public GameObject TogglePrefab;
		public GameObject DropdownPrefab;

		[Tooltip("Palette to be used for overriding selected, highlighted, etc. elements states.")]
		public PaletteConfig PaletteConfig;

		public Dictionary<string, GameObject> PanelDictionary = new Dictionary<string, GameObject>();
		public Dictionary<string, Dictionary<string, GameObject>> PanelObjectDictionary = new Dictionary<string, Dictionary<string, GameObject>>();

		public void CreateMenu(MenuHelper helper, MenuConfig menuConfig) {
			helper.MenuConfig = menuConfig;
			List<PanelManager> panels = new List<PanelManager>();
			foreach (PanelConfig panel in menuConfig.PanelConfigs) {
				PanelManager manager = CreatePanel(Parent, panel, menuConfig);
				panels.Add(manager);
			}
			helper.Panels = panels.ToArray();
		}

		public PanelManager CreatePanel(GameObject parent, PanelConfig config, MenuConfig menuConfig) {
			GameObject prefab = config.PrefabOverride == null ? MenuObjectPrefab : config.PrefabOverride;
			GameObject obj = Instantiate(prefab, parent.transform);
			obj.name = config.Key;
			PanelManager manager = obj.AddComponent<PanelManager>();
			manager.Key = config.Key;
			PanelDictionary.Add(config.Key, obj);

			if (config.SupplementalObjects != null) {
				List<GameObject> supplementalObjects = new List<GameObject>();
				foreach (GameObject supPrefab in config.SupplementalObjects) {
					GameObject supObj = Instantiate(supPrefab, parent.transform);
					supplementalObjects.Add(supObj);
				}
				manager.OtherObjects = supplementalObjects.ToArray();
			}

			Dictionary<string, GameObject> dict = new Dictionary<string, GameObject>();

			// Use this to set up nav menu. Right now it assumes layout is vertical.
			List<Selectable> selectableObjects = new List<Selectable>();

			foreach (PanelObjectConfig objConfig in config.PanelObjects) {
				GameObject go = CreatePanelObject(obj, objConfig);
				UIElementManager elementManager = go.GetComponentInChildren<UIElementManager>();
				elementManager.SetColors(menuConfig.PaletteConfig);
				if (elementManager.SelectableObject != null && elementManager.SelectableObject.GetComponent<Selectable>() != null) {
					selectableObjects.Add(elementManager.SelectableObject.GetComponent<Selectable>());
				}
				objConfig.CreationCallback?.Invoke(go);
				dict[objConfig.Key] = go;
				if (objConfig.Key == config.DefaultSelectableKey) {
					manager.DefaultInput = elementManager;
				}
			}

			// Hook up navigation with elements with selectable objects.
			for (int i = 0; i < selectableObjects.Count; i++) {
				// Make new one to avoid potential property strangeness.
				Navigation navigation = new Navigation();
				navigation.mode = Navigation.Mode.Explicit;
				if (config.HorizontalMenu) {
					navigation.selectOnLeft = i > 0 ? selectableObjects[i - 1] : null;
					navigation.selectOnRight = i < selectableObjects.Count - 1 ? selectableObjects[i + 1] : null;
					navigation.selectOnUp = null;
					navigation.selectOnDown = null;
				} else {
					navigation.selectOnUp = i > 0 ? selectableObjects[i - 1] : null;
					navigation.selectOnDown = i < selectableObjects.Count - 1 ? selectableObjects[i + 1] : null;
					navigation.selectOnLeft = null;
					navigation.selectOnRight = null;
				}
				selectableObjects[i].navigation = navigation;
			}

			PanelObjectDictionary[config.Key] = dict;
			return manager;
		}

		protected GameObject CreatePanelObject(GameObject panel, PanelObjectConfig config) {
			if (config is ButtonConfig) {
				return CreateButton(panel, (ButtonConfig) config);
			} else if (config is SliderConfig) {
				return CreateSlider(panel, (SliderConfig) config);
			} else if (config is ToggleConfig) {
				return CreateToggle(panel, (ToggleConfig) config);
			} else if (config is DropdownConfig) {
				return CreateDropdown(panel, (DropdownConfig)config);
			}
			return null;
		}

		public GameObject CreateButton(GameObject parent, ButtonConfig config) {
			GameObject prefab = config.PrefabOverride == null ? ButtonPrefab : config.PrefabOverride;
			GameObject go = Instantiate(prefab, parent.transform);
			go.name = config.Key;
			ButtonManager manager = go.GetComponent<ButtonManager>();
			if (manager == null) {
				Debug.LogWarning("Button prefab does not contain ButtonManager. Menu generation will not proceed normally!");
			} else {
				manager.SetText(config.DisplayText);
				manager.ButtonPressed += config.Handler;
			}
			return go;
		}

		public GameObject CreateSlider(GameObject parent, SliderConfig config) {
			GameObject prefab = config.PrefabOverride == null ? SliderPefab : config.PrefabOverride;
			GameObject go = Instantiate(prefab, parent.transform);
			go.name = config.Key;
			SliderManager manager = go.GetComponent<SliderManager>();
			if (manager == null) {
				Debug.LogWarning("Slider prefab does not contain SliderManager. Menu generation will not proceed normally!");
			} else {
				manager.SetRange(config.MinValue, config.MaxValue);
				manager.SetValue(config.DefaultValue);
				manager.SetText(config.DisplayText);
				manager.SliderUpdated += config.Handler;
			}
			return go;
		}

		public GameObject CreateToggle(GameObject parent, ToggleConfig config) {
			GameObject prefab = config.PrefabOverride == null ? TogglePrefab : config.PrefabOverride;
			GameObject go = Instantiate(prefab, parent.transform);
			go.name = config.Key;
			ToggleManager manager = go.GetComponent<ToggleManager>();
			if (manager == null) {
				Debug.LogWarning("Toggle prefab does not contain ToggleManager. Menu generation will not proceed normally!");
			} else {
				manager.SetToggled(config.IsOn);
				manager.SetText(config.DisplayText);
				manager.TogglePressed += config.Handler;
			}
			return go;
		}

		public GameObject CreateDropdown(GameObject parent, DropdownConfig config) {
			GameObject prefab = config.PrefabOverride == null ? DropdownPrefab : config.PrefabOverride;
			GameObject go = Instantiate(prefab, parent.transform);
			go.name = config.Key;
			DropdownManager manager = go.GetComponent<DropdownManager>();
			if (manager == null) {
				Debug.LogWarning("Dropdown prefab does not contain DropdownManager. Menu generation will not proceed normally!");
			} else {
				manager.SetText(config.DisplayText);
				manager.SetOptions(config.OptionStrings, config.DefaultIndex);
				manager.DropdownChosen += config.Handler;
			}
			return go;
		}
	}
}