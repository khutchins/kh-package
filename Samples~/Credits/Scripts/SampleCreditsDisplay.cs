using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Menutee;

public class SampleCreditsDisplay : MonoBehaviour {
    [SerializeField] private MenuHook _creditsMenu;
    [SerializeField] private MenuInputMediator _inputMediator;

    private void Update() {
        // Displays the credits menu if it's not already up.
        // Credits animator manages its own close.
        if (_inputMediator.MenuToggleDown() && !MenuStack.Shared.IsMenuUp(_creditsMenu)) {
            _creditsMenu.PushMenu();
        }
    }
}
