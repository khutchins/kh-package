using UnityEngine;
using System.Collections;

namespace KH.UI {
	/// <summary>
	/// Applies native resolution when entering fullscreen and
	/// restores windowed resolution when leaving it. Must be
	/// on an object in the game scene. Probably won't work well
	/// across scene changes.
	/// </summary>
	public class FullscreenHandler : MonoBehaviour {

		private Resolution _windowedResolution;
		private Resolution _nativeResolution;
		private bool _fullScreenApplied;

		void Awake() {
			Resolution[] resolutions = Screen.resolutions;
			Resolution max = resolutions[0];

			foreach(Resolution res in resolutions) {
				if(res.width * res.height > max.width * max.height) {
					max = res;
				}
			}
			_nativeResolution = max;
		}

		// Update is called once per frame
		void Update() {
			if(Screen.fullScreen && !_fullScreenApplied) {
				_windowedResolution = Screen.currentResolution;
				_fullScreenApplied = true;
				Screen.SetResolution(_nativeResolution.width, _nativeResolution.height, true);
            }
			if(!Screen.fullScreen && _fullScreenApplied) {
				if(_windowedResolution.width == _nativeResolution.width && _windowedResolution.height == _nativeResolution.height) {
					_windowedResolution.width = 1024;
					_windowedResolution.height = 768;
				}
				Screen.SetResolution(_windowedResolution.width, _windowedResolution.height, false);
				_fullScreenApplied = false;
			}
		}
	}

}