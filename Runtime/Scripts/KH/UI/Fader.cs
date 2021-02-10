using UnityEngine;
using UnityEngine.UI;
using KH;
using UnityAtoms.BaseAtoms;

namespace KH.UI {
	/// <summary>
	/// Modifies the alpha of an image based on a float event. Can be used for fade to blacks, etc.
	/// </summary>
	[RequireComponent(typeof(Image))]
	public class Fader : MonoBehaviour, UnityAtoms.IAtomListener<float> {

		public FloatEvent FaderRef;
		public Color Color;
		private Image image;

		private void Awake() {
			image = GetComponent<Image>();
		}

		void OnEnable() {
			FaderRef.RegisterListener(this);
		}

		void OnDisable() {
			FaderRef.UnregisterListener(this);
		}

		public void OnEventRaised(float item) {
			if (image != null) {
				image.color = new Color(Color.r, Color.g, Color.b, item);
			}
		}
	}
}