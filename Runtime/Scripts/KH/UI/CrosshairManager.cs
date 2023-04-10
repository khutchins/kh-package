using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ratferences;

namespace KH.UI {
	public class CrosshairManager : MonoBehaviour {

		public Image CrosshairNormal;
		public Image CrosshairInteractable;

		public BoolReference CanInteract;
		public BoolReference ShowCrosshair;

		private bool _currentInteractable = false;
		private float _animProgress = 0;

		private static readonly float ANIM_TIME = 0.1f;

		private void Awake() {
			CrosshairInteractable.enabled = false;
			CrosshairNormal.enabled = true;
			ShowCrosshair.Value = true;
		}

		public void SetInteractableCrosshair(bool interactable) {
			if (interactable == _currentInteractable) {
				return;
			}
			StopAllCoroutines();
			_currentInteractable = interactable;
			StartCoroutine(UpdateCrosshair());
		}

		IEnumerator UpdateCrosshair() {
			float target = _currentInteractable ? 1f : 0f;
			float changePerSecond = (_currentInteractable ? 1f : -1f) / Mathf.Max(0.0001f, ANIM_TIME);
			while (_animProgress != target) {
				_animProgress = Mathf.Clamp(_animProgress + changePerSecond * Time.deltaTime, 0, 1);
				CrosshairNormal.enabled = _animProgress < 0.1f;
				CrosshairInteractable.enabled = _animProgress >= 0.1f;
				CrosshairInteractable.transform.localScale = new Vector3(_animProgress, _animProgress, _animProgress);
				yield return null;
			}
		}

		private void OnEnable() {
			CanInteract.ValueChanged += InteractUpdated;
			ShowCrosshair.ValueChanged += ShowHideCrosshair;
		}

		private void OnDisable() {
			CanInteract.ValueChanged -= InteractUpdated;
			ShowCrosshair.ValueChanged -= ShowHideCrosshair;
		}

		private void ShowHideCrosshair(bool newValue) {
			GetComponent<Canvas>().enabled = newValue;
		}

		private void InteractUpdated(bool newInteract) {
			SetInteractableCrosshair(newInteract);
		}
	}
}