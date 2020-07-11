using KH;
using KH.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Helper class with some common menu setups.
/// </summary>
public static class MenuConfigHelper {
	public readonly static string KEY_RESUME = "resume";
	public readonly static string KEY_OPTIONS = "options";
	public readonly static string KEY_EXIT = "exit";
	public readonly static string KEY_BACK = "back";
	public readonly static string KEY_SLIDER_TEXT = "slidertext";
	public readonly static string KEY_SLIDER_LOOK = "sliderlook";
	public readonly static string KEY_SLIDER_VOLUME = "slidervolume";
	public readonly static string KEY_RESOLUTION = "resolution";
	public readonly static string KEY_QUALITY = "quality";
	public readonly static string KEY_FULLSCREEN = "fullscreen";

	public readonly static string KEY_GASOLINE = "gas";
	public readonly static string KEY_TOWER = "tower";

	/// <summary>
	/// Standard resolution dropdown. Filters out Hz values.
	/// </summary>
	/// <returns>Resolution panel object config</returns>
	public static PanelObjectConfig ResolutionConfig() {

		Resolution[] filteredResolutions = Screen.resolutions.Where(res => Mathf.Abs(res.refreshRate - Screen.currentResolution.refreshRate) <= 1).ToArray();
		Resolution playerResolution = new Resolution();
		playerResolution.width = Screen.width;
		playerResolution.height = Screen.height;
		Debug.Log("Startup resolution " + playerResolution);
		int idx = filteredResolutions.Length - 1;
		for (int i = 0; i < filteredResolutions.Length; i++) {
			if (filteredResolutions[i].width == playerResolution.width && filteredResolutions[i].height == playerResolution.height) {
				idx = i;
				break;
			}
		}
		string[] resolutionStrings = filteredResolutions.Select(x => x.width + " x " + x.height).ToArray();

		return new DropdownConfig(KEY_RESOLUTION, "Resolution", resolutionStrings, idx, null, delegate (DropdownManager manager, int newIndex, string optionString) {
			Resolution res = filteredResolutions[newIndex];
			Screen.SetResolution(res.width, res.height, Screen.fullScreenMode);
			Debug.Log("Setting resolution to " + res);
		});
	}

	public static PanelConfig StandardOptionsPanel(string key, MenuHelper menuHelper) {
		List<PanelObjectConfig> list = new List<PanelObjectConfig>();
		list.Add(new ButtonConfig(KEY_BACK, "Back", null, delegate (ButtonManager manager) {
			menuHelper.PopMenu();
		}));
		list.Add(new SliderConfig(KEY_SLIDER_LOOK, "Look Speed", 0.1f, 3, 1 /* Get look sensitivity */, null, delegate (SliderManager manager, float newValue) {
			// Handle sensitivity
		}));
		list.Add(new SliderConfig(KEY_SLIDER_VOLUME, "Volume", 0F, 1F, 1 /* Get volume value */, null, delegate (SliderManager manager, float newValue) {
			// Handle volume
		}));
		list.Add(new DropdownConfig(KEY_QUALITY, "Quality", QualitySettings.names, QualitySettings.GetQualityLevel(), null, delegate (DropdownManager manager, int newIndex, string optionString) {
			QualitySettings.SetQualityLevel(newIndex);
		}));
		// No point in showing resolution config in WebGL - It does nothing.
		if (Application.platform != RuntimePlatform.WebGLPlayer) {
			list.Add(ResolutionConfig());
		}
		list.Add(new ToggleConfig(KEY_FULLSCREEN, "Fullscreen", Screen.fullScreen, null, delegate (ToggleManager manager, bool newValue) {
			Screen.fullScreen = newValue;
		}));
		return new PanelConfig(key, KEY_BACK, list.ToArray());
	}
}