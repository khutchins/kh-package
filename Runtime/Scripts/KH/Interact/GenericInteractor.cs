using KH.Input;
using KH.Interact;
using KH.References;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public class GenericInteractor : MonoBehaviour, IInteractionLockController {

		public BoolReference CanInteract;
		public SingleInputMediator InteractMediator;

		private Interactor _interactor;
		private Interactable _focusedInteractable;
		private Interactable _interactTarget;

		public int LookLocks { get; set; }
		public int MoveLocks { get; set; }

		public bool LookLocked { get => LookLocks > 0; }
		public bool MovementLocked { get => MoveLocks > 0; }

		public void Awake() {
			_interactor = new Interactor(this.gameObject, this);
		}

		public void CheckInteract(Transform cameraTransform) {
			// Don't process input if paused.
			if (Time.deltaTime == 0) return;

			RaycastHit hit;
			if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, 1.5F)) {
				Interactable interactable = hit.transform.gameObject.GetComponentInChildren<Interactable>();
				if (interactable != null) {
					UpdateFocusedInteractable(interactable);
				} else {
					UpdateFocusedInteractable(null);
				}
			} else {
				UpdateFocusedInteractable(null);
			}

			if (!_interactor.Locked && InteractMediator.InputJustDown() && _focusedInteractable != null) {
				StartInteracting(_focusedInteractable);
				return;
			}
		}

		private void UpdateFocusedInteractable(Interactable target) {
			if (target == _focusedInteractable) {
				return;
			}
			if (_focusedInteractable) {
				_focusedInteractable.OnLoseFocus();
				_focusedInteractable = null;
			}
			if (target) {
				_focusedInteractable = target;
				_focusedInteractable.OnGainFocus();
			}
			CanInteract?.SetValue(target != null);
		}

		public void StartInteracting(Interactable target) {
			StopInteracting();

			if (target.CanInteract(_interactor)) {
				_interactTarget = target;
				_interactTarget.StartInteracting(_interactor);
			} else {
				Debug.Log("Target refused interaction");
			}
		}

		public void StopInteracting() {
			if (_interactTarget != null) {
				// We use this temp variable, otherwise
				// listeners on what StopInteracting calls
				// down to could call StopInteracting and
				// cause a loop.
				Interactable temp = _interactTarget;
				_interactTarget = null;
				temp.StopInteracting(_interactor);
			}
		}
	}
}