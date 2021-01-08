using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {

	public delegate void ActionFinishedHandler(Action action);

	public abstract class Action : MonoBehaviour {
		public event ActionFinishedHandler FinishedAction;

		public abstract void Begin();

		protected void Finished() {
			FinishedAction?.Invoke(this);
		}
	}
}