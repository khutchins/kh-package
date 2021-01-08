using KH.Interact;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public abstract class Trigger : MonoBehaviour {
		public int TimesToTrigger = 1;

		private int _timesTriggered = 0;
		private bool _triggered = false;

		public void PlayerEntered(IInteractionLockController mouseLook) {
			if (TimesToTrigger >= 0 && _timesTriggered < TimesToTrigger) {
				PlayerEnteredInternal(mouseLook);
				_timesTriggered++;
				_triggered = true;
			}
		}
		public void PlayerLeft(IInteractionLockController mouseLook) {
			if (_triggered) {
				PlayerLeftInternal(mouseLook);
				_triggered = false;
			}
		}

		protected virtual void PlayerEnteredInternal(IInteractionLockController mouseLook) { }
		protected virtual void PlayerLeftInternal(IInteractionLockController mouseLook) { }
	}
}