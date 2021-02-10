using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
	public class URLOpener : MonoBehaviour {
		public string URL = "https://khutchins.itch.io";

		public void OpenURL() {
			// WebGL will replace the game window with the loaded page,
			// which is less than ideal.
			if (Application.platform != RuntimePlatform.WebGLPlayer) {
				Application.OpenURL(URL);
			}
		}
	}
}