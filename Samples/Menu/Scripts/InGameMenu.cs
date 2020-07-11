using KH;
using KH.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InGameMenu : MenuGenerator {

	public readonly static string MENU_KEY_MAIN = "Main";
	public readonly static string MENU_KEY_OPTIONS = "Options";
	public readonly static string MENU_KEY_CREDITS = "Credits";

	public readonly static string KEY_RESUME = "resume";
	public readonly static string KEY_OPTIONS = "options";
	public readonly static string KEY_CREDITS = "credits";
	public readonly static string KEY_EXIT = "exit";
	public readonly static string KEY_BACK = "back";
	public readonly static string KEY_SLIDER_TEXT = "slidertext";
	public readonly static string KEY_SLIDER_LOOK = "sliderlook";
	public readonly static string KEY_RESOLUTION = "resolution";
	public readonly static string KEY_QUALITY = "quality";
	public readonly static string KEY_FULLSCREEN = "fullscreen";

	public GameObject CreditsPrefab;

	private MenuHelper _manager;

	void Awake() {
		_manager = GetComponent<MenuHelper>();

		MenuConfig config = new MenuConfig(true, true, MENU_KEY_MAIN, PaletteConfig, new PanelConfig[] {
			new PanelConfig(MENU_KEY_MAIN, KEY_RESUME, new PanelObjectConfig[] {
				new ButtonConfig(KEY_RESUME, "Resume", null, delegate(ButtonManager manager) {
					_manager.ExitMenu();
				}),
				new ButtonConfig(KEY_OPTIONS, "Options", null, delegate(ButtonManager manager) {
					_manager.PushMenu(MENU_KEY_OPTIONS);
				}),
				new ButtonConfig("Restart", "Restart", null, delegate(ButtonManager manager) {
					_manager.NewMap();
				}),
				new ButtonConfig(KEY_EXIT, "Exit", null, delegate(ButtonManager manager) {
					_manager.ExitGame();
				})
			}),
			new PanelConfig(MENU_KEY_CREDITS, KEY_BACK, new PanelObjectConfig[] {
				new ButtonConfig(KEY_BACK, "Back", null, delegate (ButtonManager manager) {
					_manager.PopMenu();
				})
			}, new GameObject[] { CreditsPrefab }, true),
			MenuConfigHelper.StandardOptionsPanel(MENU_KEY_OPTIONS, _manager),
		}, MenuDecoration);

		CreateMenu(_manager, config);
	}
}
