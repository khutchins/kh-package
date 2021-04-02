using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
	/// <summary>
	/// Value reference that resets to the default value on entering the scene.
	/// </summary>
	public class ResettingValueReference<T> : ValueReference<T>, ISerializationCallbackReceiver {
		public T InitialValue;

		public void OnAfterDeserialize() {
			Value = InitialValue;
		}

		public void OnBeforeSerialize() {
		}
	}
}