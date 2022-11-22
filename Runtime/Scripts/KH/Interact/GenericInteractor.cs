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
		[Tooltip("How far to check for interactables.")]
		public float RaycastDistance = 1.5f;
		[Tooltip("If true, will ignore delta y for determining raycast distance if looking down.")]
		public bool IgnoreDownDistanceForRaycasts = true;
		[Tooltip("Maximum raycast distance to check for interactables.")]
		public float MaxRaycastDistance = 3f;

		private Interactor _interactor;
		private Interactable _focusedInteractable;
		private Interactable _interactTarget;

		public int LookLocks { get; set; }
		public int MoveLocks { get; set; }

		public bool LookLocked { get => LookLocks > 0; }
		public bool MovementLocked { get => MoveLocks > 0; }

		void Awake() {
			_interactor = new Interactor(this.gameObject, this);
		}

		private float AdjustedDistance(Vector3 forward) {
			if (!IgnoreDownDistanceForRaycasts) return RaycastDistance;
			if (forward.y >= 0) return RaycastDistance;
			Vector2 xz = new Vector2(forward.x, forward.z);
			float magnitude = Mathf.Max(0.01f, xz.magnitude);
			float scalar = RaycastDistance / magnitude;
			return Mathf.Min(MaxRaycastDistance, RaycastDistance * scalar);
		}

		public void CheckInteract(Transform cameraTransform) {
			// Don't process input if paused.
			if (Time.deltaTime == 0) return;

			RaycastHit hit;
			if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, AdjustedDistance(cameraTransform.forward))) {
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

		private void OnTriggerEnter(Collider other) {
			Trigger trigger = other.gameObject.GetComponent<Trigger>();
			if (trigger != null) {
				trigger.PlayerEntered(this);
			}
		}

		private void OnTriggerExit(Collider other) {
			Trigger trigger = other.gameObject.GetComponent<Trigger>();
			if (trigger != null) {
				trigger.PlayerLeft(this);
			}
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