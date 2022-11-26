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

		[Header("Config")]
		[Tooltip("Whether or not this interactable should lock movement, look, or both on the interacting player.")]
		[SerializeField] LockSpec LockState = LockSpec.LockBoth;
		[Tooltip("Whether or not this is the only interaction that can occur.")]
		[SerializeField] bool Exclusive = true;
		[Tooltip("Whether the interaction should immediately end (useful for one-offs, like playing a sound.")]
		[SerializeField] bool EndImmediately = false;

		[Header("Interaction")]
		/// <summary>
		/// Whether the player can cancel the interaction by pressing the interact key again.
		/// </summary>
		[Tooltip("Whether the player can cancel the interaction by pressing the interact key again.")]
		[SerializeField] bool CancelOnInteract = false;

		[Tooltip("Mediator that detects interaction. Only necessary if CancelOnInteract is true.")]
		[ConditionalHide("CancelOnInteract")]
		[SerializeField] SingleInputMediator InteractMediator;

		[Header("Events")]
		public UnityEvent OnInteractStart;
		public UnityEvent OnInteractStop;
		public UnityEvent OnFocusGain;
		public UnityEvent OnFocusLoss;

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

        public override void OnGainFocus() {
            base.OnGainFocus();
			OnFocusGain?.Invoke();
        }

        public override void OnLoseFocus() {
            base.OnLoseFocus();
			OnFocusLoss?.Invoke();
        }

        private void Update() {
			if (IsBeingInteractedWith && CancelOnInteract && InteractMediator.InputJustDown()) {
				// Only stop interacting if it's not focused on another interactable (or interactions
				// are not exclusive).
				if (ExclusiveInteraction() || !CurrentInteractor.HasFocusedInteractable) {
					ForceStopInteraction();
				}
			}
		}

		public void StopInteracting() {
			ForceStopInteraction();
		}
	}
}