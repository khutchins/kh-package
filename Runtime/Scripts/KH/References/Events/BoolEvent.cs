using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KH.References {
	public class BoolEvent : ValueEvent<bool, BoolReference> {

		public UnityEvent OnTrue;
		public UnityEvent OnFalse;

		protected override void ReferenceValueChanged(bool newValue) {
			base.ReferenceValueChanged(newValue);
			if (newValue) OnTrue?.Invoke();
			else OnFalse?.Invoke();
		}
	}
}