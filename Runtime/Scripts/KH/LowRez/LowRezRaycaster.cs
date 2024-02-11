using Ratferences;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KH.LowRez {
	public class LowRezRaycaster : MonoBehaviour {
		[SerializeField] Vector3Reference MousePositionRef;
		private GraphicRaycaster _raycaster;
		private IEnumerable<GameObject> lastHits = new List<GameObject>();

		private PointerEventData pointer;
		private void Awake() {
			// Ensure GraphicRaycaster exists to query raycasts, but disable it
			// so that it doesn't send incorrect raycasts.
			_raycaster = GetComponent<GraphicRaycaster>();
			if (_raycaster == null) _raycaster = this.gameObject.AddComponent<GraphicRaycaster>();
			_raycaster.enabled = false;

			pointer = new PointerEventData(EventSystem.current);
		}

		private void Update() {
			List<RaycastResult> results = new List<RaycastResult>();
			pointer.position = MousePositionRef.Value;
			_raycaster.Raycast(pointer, results);
			List<GameObject> goResults = results.Select(x => x.gameObject).ToList();

			foreach (GameObject res in lastHits.Except(goResults)) {
				if (res == null) continue;
				foreach (var comp in res.GetComponents<IPointerExitHandler>()) {
					comp.OnPointerExit(pointer);
				}
			}
			foreach (GameObject res in goResults.Except(lastHits)) {
				foreach (var comp in res.GetComponents<IPointerEnterHandler>()) {
					//Debug.Log($"Entered: {comp}");
					comp.OnPointerEnter(pointer);
				}
			}

			lastHits = goResults;

			if (UnityEngine.Input.GetMouseButtonDown(0)) {
				foreach (GameObject res in goResults) {
					foreach (var comp in res.GetComponents<ISubmitHandler>()) {
						comp.OnSubmit(pointer);
					}
				}
			}
		}
	}
}