using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KH.Actions;

namespace KH.Interact {
	public class InteractableAction : Interactable {

		public Action Action;

		public enum LockSpec {
			None = 0,
			LockMovement = 1 << 0,
			LockLook = 1 << 1,
			LockBoth = LockMovement | LockLook
		}

		[Header("Config")]
		[Tooltip("Whether or not this interactable should lock movement, look, or both on the interacting player.")]
		[SerializeField] LockSpec LockState = LockSpec.LockBoth;
		[Tooltip("Whether or not this is the only interaction that can occur.")]
		[SerializeField] bool Exclusive = true;

		public override bool ExclusiveInteraction() {
			return Exclusive;
		}

		public override bool LocksMouse() {
			return (LockState & LockSpec.LockLook) > 0;
		}

		public override bool LocksMovement() {
			return (LockState & LockSpec.LockMovement) > 0;
		}

		public override bool ShouldAllowInteraction(Interactor interator) {
			return Action != null;
		}

		protected override void StartInteractingInner(Interactor interactor) {
			if (Action != null) {
				Action.FinishedAction += Action_FinishedAction;
				Action.Begin();
			} else {
				ForceStopInteraction();
			}
		}

		private void Action_FinishedAction(Action action) {
			Action.FinishedAction -= Action_FinishedAction;
			ForceStopInteraction();
		}

		protected override void StopInteractingInner(Interactor interactor) {

		}
	}
}