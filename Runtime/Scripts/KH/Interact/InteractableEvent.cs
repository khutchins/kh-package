using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using KH.Actions;
using KH.Input;

namespace KH.Interact {
	public class InteractableEvent : Interactable {

		public enum LockSpec {
			None = 0,
			LockMovement = 1 << 0,
			LockLook = 1 << 1,
			LockBoth = LockMovement | LockLook
        }

		[Tooltip("Whether or not this interactable should lock movement, look, or both on the interacting player.")]
		[SerializeField] LockSpec LockState = LockSpec.LockBoth;
		[Tooltip("Whether or not this is the only interaction that can occur.")]
		[SerializeField] bool Exclusive = true;
		/// <summary>
		/// Whether the player can cancel the interaction by pressing the interact key again.
		/// </summary>
		[Tooltip("Whether the player can cancel the interaction by pressing the interact key again.")]
		[SerializeField] bool CancelOnInteract = false;

		[Tooltip("Mediator that detects interaction. Only necessary if CancelOnInteract is true.")]
		[SerializeField] SingleInputMediator InteractMediator;
		[Tooltip("Whether the interaction should immediately end (useful for one-offs, like playing a sound.")]
		[SerializeField] bool EndImmediately = false;

		public UnityEvent OnInteractStart;
		public UnityEvent OnInteractStop;

		public override bool ExclusiveInteraction() {
			return Exclusive;
		}

		public override bool LocksMouse() {
			return (LockState & LockSpec.LockLook) > 0;
		}

		public override bool LocksMovement() {
			return (LockState & LockSpec.LockMovement) > 0;
		}

		protected override void StartInteractingInner(Interactor interactor) {
			OnInteractStart?.Invoke();
			if (EndImmediately) ForceStopInteraction();
		}

		protected override void StopInteractingInner(Interactor interactor) {
			OnInteractStop?.Invoke();
		}

		private void Update() {
			if (IsBeingInteractedWith && CancelOnInteract && InteractMediator.InputJustDown()) {
				ForceStopInteraction();
			}
		}

		public void StopInteracting() {
			ForceStopInteraction();
		}
	}
}