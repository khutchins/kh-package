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
	public class MenuHelper : MonoBehaviour, IMenu {

		private bool _active;
		public BoolVariable Paused;

		public GameObject BG;
		public PanelManager[] Panels;
		public MenuConfig MenuConfig;

		protected Stack<string> _menuStack = new Stack<string>();

		public EventSystem EventSystem;
		private UIElementManager _activeDefaultInput;

		public KH.Input.InputMediator InputMediator;

		private void Awake() {
			SetMenuUp(false);
		}

		private void Start() {
			// Read Closeable in Start so that other scripts
			// can set it in Awake.
			if (!MenuConfig.Closeable) {
				MenuStack.Shared.PushAndShowMenu(this);
			}
		}

		public MenuAttributes GetMenuAttributes() {
			return MenuConfig.MenuPausesGame ? MenuAttributes.StandardPauseMenu() : MenuAttributes.StandardNonPauseMenu();
		}

		public void SetMenuUp(bool newUp) {
			_active = newUp;
			BG.SetActive(_active);
			ActivateMenu(_active ? MenuConfig.MainPanelKey : null);
		}

		void ToggleMenu() {
			if (!MenuConfig.Closeable) {
				return;
			}
			MenuStack.Shared.ToggleMenu(this);
		}

		private void ActivateMenu(string key) {
			ActivateMenu(key, MenuConfig.PanelConfigs.Where(p => p.Key == key).FirstOrDefault());
		}

		private void ActivateMenu(string key, PanelConfig config) {
			PanelManager active = null;
			EventSystem.SetSelectedGameObject(null);
			foreach (PanelManager manager in Panels) {
				manager.SetPanelActive(key == manager.Key);
				if (key == manager.Key) {
					active = manager;
				}
			}
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
					_menuStack.Push(key);
					GoToMenu(key);
					return;
				}
			}
			Debug.LogError("Cannot push menu " + key + "! Not in Panels array.");
		}

		private bool IsAtRoot() {
			return _menuStack.Count == 0;
		}

		public void PopMenu() {
			if (_menuStack.Count > 0) {
				_menuStack.Pop();
			}
			if (_menuStack.Count == 0) {
				GoToMenu(MenuConfig.MainPanelKey);
			} else {
				GoToMenu(_menuStack.Last());
			}
		}

		public void ExitMenu() {
			if (!_active)
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