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
		[Tooltip("Reference of color to fade to/from. Takes precedence over Color.")]
		public ColorReference ColorRef;
		[Tooltip("Colorolor to fade to/from. Only used if ColorRef is null.")]
		public Color Color;
		private Image image;

		private void Awake() {
			// Set FaderRef to 0 on awake to avoid accidentally transfering fade across scenes.
			// As such, setting the initial value of FaderRef should be done in Start().
			FaderRef.Value = 0;
			image = GetComponent<Image>();
		}

		void OnEnable() {
			FaderRef.ValueChanged += UpdateFade;
		}

		void OnDisable() {
			FaderRef.ValueChanged -= UpdateFade;
		}

		public void UpdateFade(float newValue) {
			Color color = ColorRef != null ? ColorRef.Value : Color;
			if (image != null) {
				image.color = new Color(color.r, color.g, color.b, newValue);
			}
		}
	}
}