using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
	public class Signal : ScriptableObject {

		public delegate void SignalRaised();

		public SignalRaised OnSignalRaised;

		public void Raise() {
			OnSignalRaised?.Invoke();
		}
	}
}