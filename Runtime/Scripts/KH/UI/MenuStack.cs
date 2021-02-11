using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace KH.UI {
    public struct MenuAttributes {
        public CursorLockMode cursorLockMode;
        public bool cursorVisible;
        /// <summary>
        /// Sets the time scale while in the menu. If the value is negative, will not modify the existing time scale.
        /// </summary>
        public float timeScale;
        public bool pauseGame;

        public static MenuAttributes StandardPauseMenu() {
            MenuAttributes attributes = new MenuAttributes();
            attributes.cursorLockMode = CursorLockMode.Confined;
            attributes.cursorVisible = true;
            attributes.timeScale = 0f;
            attributes.pauseGame = true;
            return attributes;
		}

        public static MenuAttributes StandardInGame() {
            MenuAttributes attributes = new MenuAttributes();
            attributes.cursorLockMode = CursorLockMode.Locked;
            attributes.cursorVisible = false;
            attributes.timeScale = -1f;
            attributes.pauseGame = false;
            return attributes;
        }

        public static MenuAttributes StandardNonPauseMenu() {
            MenuAttributes attributes = new MenuAttributes();
            attributes.cursorLockMode = CursorLockMode.Confined;
            attributes.cursorVisible = true;
            attributes.timeScale = -1f;
            attributes.pauseGame = false;
            return attributes;
        }
    }

    public interface IMenu {
        MenuAttributes GetMenuAttributes();
        void SetMenuUp(bool newUp);
	}

    public class MenuStack : MonoBehaviour {
        public static MenuStack Shared;

        private Stack<IMenu> _menuStack = new Stack<IMenu>();
        private Stack<MenuAttributes> _cachedMenuAttributes = new Stack<MenuAttributes>();

        public BoolVariable PauseReference;

		void Awake() {
            Shared = this;
		}

        public void ToggleMenu(IMenu menu) {
            if (!_menuStack.Contains(menu)) {
                PushAndShowMenu(menu);
			} else if (_menuStack.Count > 0 && _menuStack.Peek() == menu) {
                PopAndCloseMenu(menu);
			}
		}

        public void PushAndShowMenu(IMenu menu) {
            if (menu == null) {
                Debug.LogWarning("Attempting to push a null menu!");
            }

            CacheCurrentMenuAttributes();
            _menuStack.Push(menu);
            ApplyMenuAttributes(menu.GetMenuAttributes());
            menu.SetMenuUp(true);
        }

        public void PopAndPushNewMenu(IMenu current, IMenu newMenu) {
            PopAndCloseMenu(current);
            PushAndShowMenu(newMenu);
		}

        public void PopAndCloseMenu(IMenu menu) {
            if (_menuStack.Count == 0) {
                Debug.LogWarning("Attempting to pop menu but stack is empty!");
                return;
            } else if (_menuStack.Peek() != menu) {
                Debug.LogWarning("Attempting to pop menu not on top of stack!");
                return;
            }
            PopAndApplyMenuAttributes();
            if (_menuStack.Count > 0) {
                _menuStack.Pop().SetMenuUp(false);
			}
		}

        void CacheCurrentMenuAttributes() {
            MenuAttributes attributes = new MenuAttributes();
            attributes.cursorLockMode = Cursor.lockState;
            attributes.cursorVisible = Cursor.visible;
            attributes.timeScale = Time.timeScale;
            attributes.pauseGame = PauseReference != null ? PauseReference.Value : false;
            _cachedMenuAttributes.Push(attributes);
        }

        void PopAndApplyMenuAttributes() {
            if (_cachedMenuAttributes.Count == 0) {
                Debug.LogWarning("Attempting to pop menu attributes but stack is empty!");
                return;
            }
            MenuAttributes attributes = _cachedMenuAttributes.Pop();
            ApplyMenuAttributes(attributes);
        }

        void ApplyMenuAttributes(MenuAttributes attributes) {
            Cursor.lockState = attributes.cursorLockMode;
            Cursor.visible = attributes.cursorVisible;
            if (attributes.timeScale >= 0) {
                Time.timeScale = attributes.timeScale;
            }
            PauseReference?.SetValue(attributes.pauseGame);
        }
    }
}