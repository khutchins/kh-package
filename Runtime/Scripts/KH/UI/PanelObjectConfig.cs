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

		public PanelObjectConfig(string key, Action<GameObject> creationCallback) {
			Key = key;
			CreationCallback = creationCallback;
		}
	}
}