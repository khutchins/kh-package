using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace KH.References {
	/// <summary>
	/// Class that triggers a UnityEvent based on a signal being raised.
	/// </summary>
	public class SignalEvent : MonoBehaviour {
		[SerializeField]
		private Signal _reference;

		public UnityEvent Event;

		protected virtual void SignalRaised() {
			Event?.Invoke();
		}

		private void OnEnable() {
			if (_reference != null) {
				_reference.OnSignalRaised += SignalRaised;
			}
		}

		private void OnDisable() {
			if (_reference != null) {
				_reference.OnSignalRaised -= SignalRaised;
			}
		}
	}
}