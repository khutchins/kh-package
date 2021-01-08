using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KH {
	[RequireComponent(typeof(Image))]
	public class FocusImage : MonoBehaviour {

		public static FocusImage Shared;

		private Image _image;

		void Awake() {
			Shared = this;
			_image = GetComponent<Image>();
		}

		public void FocusGained() {
			_image.enabled = true;
		}

		public void FocusLost() {
			_image.enabled = false;
		}
	}
}