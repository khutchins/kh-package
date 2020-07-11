using KH.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuConfig {
	public readonly bool Closeable;
	public readonly bool MenuPausesGame;
	public readonly string MainPanelKey;
	public readonly PaletteConfig PaletteConfig;
	public readonly PanelConfig[] PanelConfigs;
	public GameObject[] MenuDecoration;

	public readonly Color NormalColor;

	public MenuConfig(bool closeable, bool menuPausesGame, string mainPanelKey, PaletteConfig paletteConfig, PanelConfig[] panelConfigs, GameObject[] menuDecoration = null) {
		Closeable = closeable;
		MenuPausesGame = menuPausesGame;
		MainPanelKey = mainPanelKey;
		PaletteConfig = paletteConfig;
		PanelConfigs = panelConfigs;
		MenuDecoration = menuDecoration;
	}
}
