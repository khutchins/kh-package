using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KH.References {
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