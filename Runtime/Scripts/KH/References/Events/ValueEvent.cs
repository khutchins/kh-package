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
		[SerializeField]
		private U _reference;
		[Tooltip("Calls event when the script starts. Useful if you want to propagate the initial value.")]
		public bool TriggerOnStart = false;
		[Tooltip("Calls event when the script is enabled. Useful if this script or the object its on is often inactive or disabled. Since this call can happen before other objects get Awake(), it can cause race conditions. If possible, just don't disable this script.")]
		public bool TriggerOnEnable = false;

		public U Reference {
			get => _reference;
			set {
				UpdateReference(value);
			}
		}

		public UnityEvent<T> Event;

		private void Start() {
			if (TriggerOnStart && _reference != null) {
				ReferenceValueChanged(_reference.Value);
			}
		}

		private void UpdateReference(U newReference) {
			if (_reference != null) {
				_reference.ValueChanged -= ReferenceValueChanged;
			}
			_reference = newReference;
			if (_reference != null) {
				if (TriggerOnStart) {
					ReferenceValueChanged(_reference.Value);
				}
				_reference.ValueChanged += ReferenceValueChanged;
			}
		}

		protected virtual void ReferenceValueChanged(T newValue) {
			Event?.Invoke(newValue);
		}

		private void OnEnable() {
			if (_reference != null) {
				if (TriggerOnEnable) {
					ReferenceValueChanged(_reference.Value);
				}
				_reference.ValueChanged += ReferenceValueChanged;
			}
		}

		private void OnDisable() {
			if (_reference != null) {
				_reference.ValueChanged -= ReferenceValueChanged;
			}
		}
	}
}