using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using KH;
using UnityEngine.EventSystems;
using System.Linq;
using UnityAtoms.BaseAtoms;

namespace KH.UI {
	public class MenuHelper : MonoBehaviour {

		private bool Active;
		public BoolVariable Paused;

		public GameObject BG;
		public PanelManager[] Panels;
		public MenuConfig MenuConfig;

		public Stack<string> MenuStack = new Stack<string>();

		public EventSystem EventSystem;
		private PanelManager _activeMenu;
		private UIElementManager _activeDefaultInput;
		private float _cachedTime = 1;
		private CursorLockMode _cachedLockMode;
		private bool _cachedVisible;

		public KH.Input.InputMediator InputMediator;

		private void Start() {
			// Read Closeable in Start so that other scripts
			// can set it in Awake.
			SetMenuUp(!MenuConfig.Closeable);
		}

		void SetMenuUp(bool up) {
			Active = up;
			BG.SetActive(Active);
			Paused?.SetValue(up);
			if (Active) {
				_cachedLockMode = Cursor.lockState;
				Cursor.lockState = CursorLockMode.None;
				_cachedVisible = Cursor.visible;
				Cursor.visible = true;
				_cachedTime = Time.timeScale;
				Time.timeScale = MenuConfig.MenuPausesGame ? 0 : _cachedTime;
				ActivateMenu(MenuConfig.MainPanelKey);
			} else { // Out of Menu
				Cursor.lockState = _cachedLockMode;
				Cursor.visible = _cachedVisible;
				Time.timeScale = _cachedTime;
				ActivateMenu(null);
			}
		}

		void ToggleMenu() {
			if (!MenuConfig.Closeable) {
				return;
			}
			SetMenuUp(!Active);
		}

		private void ActivateMenu(string key) {
			ActivateMenu(key, MenuConfig.PanelConfigs.Where(p => p.Key == key).FirstOrDefault());
		}

		private void ActivateMenu(string key, PanelConfig config) {
			PanelManager active = null;
			EventSystem.SetSelectedGameObject(null);
			foreach(PanelManager manager in Panels) {
				manager.SetPanelActive(key == manager.Key);
				if (key == manager.Key) {
					active = manager;
				}
			}
			_activeMenu = active;
			if (active != null) {
				_activeDefaultInput = active.DefaultInput;
				if (_activeDefaultInput != null && _activeDefaultInput.SelectableObject != null) {
					EventSystem.SetSelectedGameObject(_activeDefaultInput.SelectableObject);
				}

				if (MenuConfig.MenuDecoration != null && config != null) {
					foreach (GameObject obj in MenuConfig.MenuDecoration) {
						obj.SetActive(!config.HideMenuDecoration);
					}
				}
			} else {
				_activeDefaultInput = null;
			}
		}

		private void Update() {
			if (InputMediator.PauseDown()) {
				ToggleMenu();
			} else if (InputMediator.UICancelDown()) {
				if (!IsAtRoot()) {
					PopMenu();
				}
			}
			if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) {
				ScreenCapture.CaptureScreenshot("Screenshot" + System.DateTime.Now.ToString("__yyyy-MM-dd-HH-mm-ss") + ".png", 2);
			}
			if (_activeDefaultInput != null && EventSystem.currentSelectedGameObject == null && (Mathf.Abs(InputMediator.UIX()) > 0.1 || Mathf.Abs(InputMediator.UIY()) > 0.1)) {
				EventSystem.SetSelectedGameObject(_activeDefaultInput.SelectableObject);
			}
		}

		/// <summary>
		/// Goes to a menu, bypassing the stack. Used by push and pop after
		/// modifying the stack. Only use if you know what you're doing.
		/// </summary>
		/// <param name="key">Key of the menu to go to.</param>
		protected void GoToMenu(string key) {
			ActivateMenu(key);
		}

		public void PushMenu(string key) {
			foreach (PanelManager panel in Panels) {
				if (panel.Key == key) {
					MenuStack.Push(key);
					GoToMenu(key);
					return;
				}
			}
			Debug.LogError("Cannot push menu " + key + "! Not in Panels array.");
		}

		private bool IsAtRoot() {
			return MenuStack.Count == 0;
		}

		public void PopMenu() {
			if (MenuStack.Count > 0) {
				MenuStack.Pop();
			}
			if (MenuStack.Count == 0) {
				GoToMenu(MenuConfig.MainPanelKey);
			} else {
				GoToMenu(MenuStack.Last());
			}
		}

		public void ExitMenu() {
			if (!Active)
				return;
			ToggleMenu();
		}

		public void NewMap() {
			SceneManager.LoadScene("GameScene");
		}

		public void ExitGame() {
			Application.Quit();
		}
	}
}