using UnityEngine;
using UnityEngine.UI;
using KH.References;

namespace KH.UI {
	/// <summary>
	/// Modifies the alpha of an image based on a float event. Can be used for fade to blacks, etc.
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class Fader : MonoBehaviour {

		[SerializeField] FloatReference FaderRef;
		[Tooltip("Reference of color to fade to/from. Takes precedence over Color.")]
		[SerializeField] ColorReference ColorRef;
		[Tooltip("Disables image if the fader ref value is zero.")]
		[SerializeField] bool DisableImageIfInvisible = true;
		[Tooltip("Color to fade to/from. Only used if ColorRef is null.")]
		public Color Color;
		private Image _image;

		private void Awake() {
			_image = GetComponent<Image>();
			// Set FaderRef to 0 on awake to avoid accidentally transfering fade across scenes.
			// As such, setting the initial value of FaderRef should be done in Start().
			FaderRef.Value = 0;
			UpdateFade(0);
		}

		void OnEnable() {
			FaderRef.ValueChanged += UpdateFade;
		}

		void OnDisable() {
			FaderRef.ValueChanged -= UpdateFade;
		}

		public void UpdateFade(float newValue) {
			_image.enabled = newValue > 0 || !DisableImageIfInvisible;
			Color color = ColorRef != null ? ColorRef.Value : Color;
			if (_image != null) {
				_image.color = new Color(color.r, color.g, color.b, newValue);
			}
		}
	}
}