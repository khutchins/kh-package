using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public interface IInteractionLockController {
		int LookLocks { get; set; }
		int MoveLocks { get; set; }
		void StopInteracting();
	}

	public class Interactor {
		public readonly GameObject GameObject;
		private readonly IInteractionLockController MovementAndMouse;
		public bool Locked;

		public Interactor(GameObject obj, IInteractionLockController movementAndMouse) {
			GameObject = obj;
			MovementAndMouse = movementAndMouse;
			Locked = false;
		}

		public void LockMouse() {
			MovementAndMouse.LookLocks++;
		}

		public void UnlockMouse() {
			MovementAndMouse.LookLocks--;
		}

		public void LockMovement() {
			MovementAndMouse.MoveLocks++;
		}

		public void UnlockMovement() {
			MovementAndMouse.MoveLocks--;
		}

		public void StopInteracting() {
			MovementAndMouse.StopInteracting();
		}
	}
}