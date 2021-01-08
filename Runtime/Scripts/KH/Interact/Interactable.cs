using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public delegate void FinishedInteractingHandler(Interactor interactor);

	public abstract class Interactable : MonoBehaviour {
		protected abstract bool StartInteractingInner(Interactor interactor);
		protected abstract void StopInteractingInner(Interactor interactor);

		public event FinishedInteractingHandler FinishedInteracting;

		public int MaxInteractionTimes = -1;

		private Shader _cachedShader;
		private Renderer _renderer;
		private Interactor _currentInteractor;
		private int _timesInteracted;

		void Awake() {
			_renderer = GetComponentInChildren<Renderer>();
		}

		public virtual bool LocksMouse() {
			return false;
		}

		public virtual bool LocksMovement() {
			return true;
		}

		public virtual bool ExclusiveInteraction() {
			return false;
		}

		public bool ForbidInteraction { get; set; }
		public bool IgnoreMouseover { get; set; }

		protected void ForceStopInteraction(Interactor interactor) {
			interactor.StopInteracting();
		}

		protected void ForceStopInteraction() {
			if (_currentInteractor != null) {
				_currentInteractor.StopInteracting();
			} else {
				Debug.LogWarning("Tried to stop interaction, but none in progress.");
			}
		}

		public bool StartInteracting(Interactor interactor) {
			if (ForbidInteraction) return false;
			if (interactor.Locked) return false;
			if (MaxInteractionTimes >= 0 && _timesInteracted >= MaxInteractionTimes) return false;
			bool allowInteract = StartInteractingInner(interactor);
			if (allowInteract) {
				if (LocksMouse()) {
					interactor.LockMouse();
				}
				if (LocksMovement()) {
					interactor.LockMovement();
				}
				if (ExclusiveInteraction()) {
					interactor.Locked = true;
				}
				_currentInteractor = interactor;
			}
			if (allowInteract) {
				_timesInteracted++;
			}
			return allowInteract;
		}

		public void StopInteracting(Interactor interactor) {
			_currentInteractor = null;
			if (LocksMouse()) {
				interactor.UnlockMouse();
			}
			if (LocksMovement()) {
				interactor.UnlockMovement();
			}
			if (ExclusiveInteraction()) {
				interactor.Locked = false;
			}
			StopInteractingInner(interactor);
			FinishedInteracting?.Invoke(interactor);
		}

		public virtual void OnGainFocus() {
			if (ForbidInteraction) return;
			if (FocusImage.Shared != null) {
				FocusImage.Shared.FocusGained();
			}
			//if (_renderer != null) {
			//	_cachedShader = _renderer.material.shader;
			//	_renderer.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
			//}
		}

		public virtual void OnLoseFocus() {
			if (ForbidInteraction) return;
			if (FocusImage.Shared != null) {
				FocusImage.Shared.FocusLost();
			}
			//if (_renderer) {
			//	_renderer.material.shader = _cachedShader;
			//}
		}
	}
}