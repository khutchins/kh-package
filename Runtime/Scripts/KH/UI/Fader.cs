using UnityEngine;
using UnityEngine.UI;
using KH.References;

namespace KH.UI {
	/// <summary>
	/// Modifies the alpha of an image based on a float event. Can be used for fade to blacks, etc.
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class Fader : MonoBehaviour {

		public FloatReference FaderRef;
		public Color Color;
		private Image image;

		private void Awake() {
			image = GetComponent<Image>();
		}

		void OnEnable() {
			FaderRef.ValueChanged += UpdateFade;
		}

		void OnDisable() {
			FaderRef.ValueChanged -= UpdateFade;
		}

		public void UpdateFade(float item) {
			if (image != null) {
				image.color = new Color(Color.r, Color.g, Color.b, item);
			}
		}
	}
}