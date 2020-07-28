using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	public abstract class PanelObjectConfig {
		/// <summary>
		/// Key that uniquely identifies an object within
		/// a panel. Should be unique within that scope.
		/// </summary>
		public string Key;
		public Action<GameObject> CreationCallback;
		public GameObject PrefabOverride;

		public PanelObjectConfig(string key, Action<GameObject> creationCallback, GameObject prefabOverride = null) {
			Key = key;
			CreationCallback = creationCallback;
			PrefabOverride = prefabOverride;
		}

		public abstract class Builder {
			protected string _key;
			protected System.Action<GameObject> _creationCallback;
			protected GameObject _prefabOverride;

			public Builder(string key) {
				_key = key;
			}

			public Builder SetPrefabOverride(GameObject prefabOverride) {
				_prefabOverride = prefabOverride;
				return this;
			}

			public Builder SetCreationCallback(System.Action<GameObject> creationCallback) {
				_creationCallback = creationCallback;
				return this;
			}
		}
	}
}