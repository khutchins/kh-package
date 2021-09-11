using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
	/// <summary>
	/// Value reference that resets to the initial value on scene change.
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