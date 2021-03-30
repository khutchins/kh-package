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

	public readonly Color NormalColor;

	public MenuConfig(bool closeable, bool menuPausesGame, string mainPanelKey, PaletteConfig paletteConfig, PanelConfig[] panelConfigs) {
		Closeable = closeable;
		MenuPausesGame = menuPausesGame;
		MainPanelKey = mainPanelKey;
		PaletteConfig = paletteConfig;
		PanelConfigs = panelConfigs;
	}

	public class Builder {
		private bool _closeable;
		private bool _menuPausesGame;
		private string _mainPanelKey = null;
		private PaletteConfig _paletteConfig;
		private List<PanelConfig> _panelConfigs = new List<PanelConfig>();

		public Builder(bool closeable, bool menuPausesGame, PaletteConfig paletteConfig) {
			_closeable = closeable;
			_menuPausesGame = menuPausesGame;
			_paletteConfig = paletteConfig;
		}

		public Builder AddPanelConfig(PanelConfig config, bool mainPanel = false) {
			_panelConfigs.Add(config);
			if (mainPanel) {
				_mainPanelKey = config.Key;
			}
			return this;
		}

		public Builder AddPanelConfig(PanelConfig.Builder configBuilder, bool mainPanel = false) {
			PanelConfig config = configBuilder.Build();
			_panelConfigs.Add(config);
			if (mainPanel) {
				_mainPanelKey = config.Key;
			}
			return this;
		}

		public Builder InsertPanelConfig(PanelConfig config, int index, bool mainPanel = false) {
			_panelConfigs.Insert(index, config);
			if (mainPanel) {
				_mainPanelKey = config.Key;
			}
			return this;
		}

		public Builder InsertPanelConfig(PanelConfig.Builder configBuilder, int index, bool mainPanel = false) {
			PanelConfig config = configBuilder.Build();
			_panelConfigs.Insert(index, config);
			if (mainPanel) {
				_mainPanelKey = config.Key;
			}
			return this;
		}

		public MenuConfig Build() {
			if (_mainPanelKey == null) {
				_mainPanelKey = _panelConfigs[0].Key;
			}
			return new MenuConfig(_closeable, _menuPausesGame, _mainPanelKey, _paletteConfig, _panelConfigs.ToArray());
		}
	}
}
