using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Interact {
	public delegate void FinishedInteractingHandler(Interactor interactor);

	public abstract class Interactable : MonoBehaviour {
		protected abstract void StartInteractingInner(Interactor interactor);
		protected abstract void StopInteractingInner(Interactor interactor);

		public event FinishedInteractingHandler FinishedInteracting;

		public int MaxInteractionTimes = -1;

		private Shader _cachedShader;
		private Renderer _renderer;
		private Interactor _currentInteractor;
		private int _timesInteracted;
		private float _lastInteractionStop;

		void Awake() {
			_renderer = GetComponentInChildren<Renderer>();
		}

		/// <summary>
		/// Whether or not the interactor should allow an interaction.
		/// NOTE: This method does not have to account for MaxIterationTimes.
		/// </summary>
		/// <returns></returns>
		public virtual bool ShouldAllowInteraction(Interactor interactor) {
			return true;
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

		/// <summary>
		/// Whether or not the interactor can interact with the given interactable.
		/// </summary>
		/// <param name="interactor">The interactor which wishes to interact.</param>
		/// <returns></returns>
		public bool CanInteract(Interactor interactor) {
			if (ForbidInteraction) return false;
			if (interactor.Locked) return false;
			if (MaxInteractionTimes >= 0 && _timesInteracted >= MaxInteractionTimes) return false;
			// Prevent retriggering an interactable if the interact button is used to stop interacting.
			if (Time.unscaledTime == _lastInteractionStop) return false;
			return ShouldAllowInteraction(interactor);
		}

		/// <summary>
		/// Begins interacting with this interactable. Check if the object can interact
		/// with CanInteract before calling this method.
		/// </summary>
		/// <param name="interactor"></param>
		public void StartInteracting(Interactor interactor) {
			if (!CanInteract(interactor)) {
				Debug.LogError("Call to StartInteracting without checking CanInteract first.");
				return;
			}

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
			_timesInteracted++;
			StartInteractingInner(interactor);
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
			_lastInteractionStop = Time.unscaledTime;
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