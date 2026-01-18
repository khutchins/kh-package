using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using KH.UI;
using Menutee;

[RequireComponent(typeof(MenuHook))]
public class CreditsAnimator : MonoBehaviour {

    [Tooltip("Credits file.")]
    public TextAsset CreditsAsset;
	[Tooltip("Scroll speed in pixels/second.")]
	public float ScrollSpeed = 30f;

    public TextMeshProUGUI CreditsText;
	public MenuInputMediator InputMediator;

	private RectTransform _creditsTextTransform;
	private float _startY;
	private MenuHook _menuHook;
	private float _displayTime = 0;

	private void Awake() {
		_creditsTextTransform = CreditsText.GetComponent<RectTransform>();
		_startY = _creditsTextTransform.anchoredPosition.y;
		_menuHook = GetComponent<MenuHook>();
		CreditsText.text = CreditsAsset.text;
	}

	private void LateUpdate() {
		if (InputMediator != null && Time.unscaledTime != _displayTime && (InputMediator.MenuToggleDown() || InputMediator.UICancelDown() || InputMediator.UISubmitDown())) {
			CloseMenu();
		}
	}

	public void StartAnimating() {
		_displayTime = Time.unscaledTime;
		StartCoroutine(AnimateCredits());
	}

	IEnumerator AnimateCredits() {
		float from = _startY;
		float to = from + _creditsTextTransform.rect.height + CreditsText.preferredHeight;

		for (float y = from; y < to; y += ScrollSpeed * Time.unscaledDeltaTime) {
			_creditsTextTransform.anchoredPosition = new Vector3(_creditsTextTransform.anchoredPosition.x, y);
			yield return null;
		}
		CloseMenu();
	}

	private void CloseMenu() {
		StopAnimating();
		if (MenuStack.Shared.IsMenuUp(_menuHook)) {
			_menuHook.PopMenu();
		}
	}

	public void StopAnimating() {
		StopAllCoroutines();
		_creditsTextTransform.position = new Vector3(_creditsTextTransform.position.x, _startY);
	}
}
