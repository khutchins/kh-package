using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.UI {
	[RequireComponent(typeof(Canvas))]
    public class MenuHook : MonoBehaviour, IMenu {
		[Header("General")]
		public bool ShowOnStart = false;
		[Header("Menu Attributes")]
		public CursorLockMode CursorLockMode = CursorLockMode.None;
		public bool CursorVisible = true;
		public bool PausesGame = true;
		[Tooltip("Time scale to use when in menu. If negative, it will use the existing time scale.")]
		public float TimeScale = 0;

		private Canvas _canvas;

		void Awake() {
			_canvas = GetComponent<Canvas>();
			_canvas.enabled = false;
		}

		void Start() {
			if (ShowOnStart) {
				PushMenu();
			}
		}

		public MenuAttributes GetMenuAttributes() {
			MenuAttributes attributes = new MenuAttributes {
				cursorLockMode = CursorLockMode,
				cursorVisible = CursorVisible,
				pauseGame = PausesGame,
				timeScale = TimeScale
			};
			return attributes;
		}

		public void SetMenuUp(bool newUp) {
			_canvas.enabled = newUp;
			Debug.Log("Up: " + newUp);
		}

		public void SetMenuOnTop(bool newOnTop) {
			Debug.Log("Top: " + newOnTop);
		}

		public void PushMenu() {
			MenuStack.Shared.PushAndShowMenu(this);
		}

		public void PopMenu() {
			MenuStack.Shared.PopAndCloseMenu(this);
		}

		void Update() {
			if (UnityEngine.Input.GetKeyDown(KeyCode.Escape)) {
				MenuStack.Shared.ToggleMenu(this);
			}
		}

	}
}