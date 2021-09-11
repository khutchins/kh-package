using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KH.References {
	/// <summary>
	/// Class that triggers a UnityEvent based on changes to a ValueReference scriptable object.
	/// Subclass for any ValueReference subclass you want to support.
	/// </summary>
	/// <typeparam name="T">Underlying type, e.g. int in IntReference (or ValueReference\<int\>).</typeparam>
	/// <typeparam name="U">Value reference type, e.g. IntReference</typeparam>
	public class ValueEvent<T, U> : MonoBehaviour where U : ValueReference<T> {
		public U Reference;
		public bool TriggerOnStart = false;

		public UnityEvent<T> Event;

		private void Start() {
			if (TriggerOnStart) {
				ReferenceValueChanged(Reference.Value);
			}
		}

		private void ReferenceValueChanged(T newValue) {
			Event?.Invoke(newValue);
		}

		private void OnEnable() {
			Reference.ValueChanged += ReferenceValueChanged;
		}

		private void OnDisable() {
			Reference.ValueChanged -= ReferenceValueChanged;
		}
	}
}