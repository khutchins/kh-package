using Ratferences;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace KH.LowRez {
	[DefaultExecutionOrder(-1)]
	[RequireComponent(typeof(RawImage))]
	public class UpdateMousePositionFromRawImage : MonoBehaviour {
		[Tooltip("Vector3 Reference to dump the mouse position in. Hook this up to the LowRezRaycaster.")]
		[SerializeField] Vector3Reference MousePositionRef;

		private RawImage _rawImage;
		private RectTransform _canvasRT;
		private RectTransform _rawImageRT;

		private void Awake() {
			_rawImage = GetComponent<RawImage>();
			_rawImageRT = _rawImage.GetComponent<RectTransform>();
			Canvas canvas = _rawImage.GetComponentInParent<Canvas>();
			_canvasRT = canvas.GetComponent<RectTransform>();
		}

		private void Update() {
			Rect canvasRect = _canvasRT.rect;
			Rect rtRect = _rawImageRT.rect;

			int textureWidth = _rawImage.texture.width;
			int textureHeight = _rawImage.texture.height;

			float _scale = textureWidth / rtRect.width;

			float canvasWidth = canvasRect.width * _scale;
			float canvasHeight = canvasRect.height * _scale;

			float rawImageBottomLeftX = (canvasWidth / 2.0f) - textureWidth - (rtRect.xMin * _scale);
			float rawImageBottomLeftY = (canvasHeight / 2.0f) - textureHeight - (rtRect.yMin * _scale);

			Vector3 pos = UnityEngine.Input.mousePosition;
			pos.x = (pos.x / Screen.width * canvasWidth) - rawImageBottomLeftX;
			pos.y = (pos.y / Screen.height * canvasHeight) - rawImageBottomLeftY;

			MousePositionRef.Value = pos;
		}
	}
}