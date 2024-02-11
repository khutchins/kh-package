using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.LowRez {
    [RequireComponent(typeof(Canvas))]
    public class LowRezScaleManager : MonoBehaviour {
        [SerializeField] private int _scale;
        [SerializeField] private bool _alwaysUseMax;
        [SerializeField] Vector2 BaseResolution = new Vector2(64, 64);
        [SerializeField] KeyCode[] ScaleUpKeys = new KeyCode[] { KeyCode.Plus, KeyCode.Equals, KeyCode.KeypadPlus };
        [SerializeField] KeyCode[] ScaleDownKeys = new KeyCode[] { KeyCode.Minus, KeyCode.KeypadMinus };
        [SerializeField] ScaleSetting[] Settings;

        private Canvas _canvas;

        public enum ScaleMode {
            Add,
            Multiply
        };

        [System.Serializable]
        public class ScaleSetting {
            public RectTransform Element;
            public ScaleMode Mode;
            public Vector2 SizeDifference;
        }

        public int Scale { 
            get => _scale; 
            set {
                _scale = value;
                UpdateScale();
            }
        }

        public bool AlwaysUseMax {
            get => _alwaysUseMax;
            set {
                _alwaysUseMax = value;
                UpdateScale();
            }
        }
        
        private void Awake() {
            UpdateScale();
        }

        private Canvas EnsureCanvas() {
            if (_canvas == null) {
                _canvas = GetComponent<Canvas>();
            }
            return _canvas;
        }

        public int MaxScale() {
            Canvas canvas = EnsureCanvas();
            Rect rect = canvas.pixelRect;
            int maxX = Mathf.FloorToInt(rect.width / Mathf.Max(1, BaseResolution.x));
            int maxY = Mathf.FloorToInt(rect.height / Mathf.Max(1, BaseResolution.y));
            return Mathf.Min(maxX, maxY);
        }

        void UpdateScale() {
            int maxScale = MaxScale();
            if (_alwaysUseMax) _scale = maxScale;
            _scale = Mathf.Clamp(_scale, 1, maxScale);
            Vector2 size = BaseResolution * _scale;
            foreach (ScaleSetting setting in Settings) {
                var element = setting.Element;
                Vector2 sizeDelta = element.sizeDelta;
                Vector2 modifier = new Vector2(
                    setting.Mode == ScaleMode.Multiply ? size.x * setting.SizeDifference.x : size.x + setting.SizeDifference.x,
                    setting.Mode == ScaleMode.Multiply ? size.y * setting.SizeDifference.y : size.y + setting.SizeDifference.y
                );
                if (element.anchorMin.x != 0 || element.anchorMax.x != 1) {
                    sizeDelta.x = modifier.x;
                }
                if (element.anchorMin.y != 0 || element.anchorMax.y != 1) {
                    sizeDelta.y = modifier.y;
                }
                element.sizeDelta = sizeDelta;
            }
        }

        private void Update() {

            bool keyHit(KeyCode[] keys) {
                foreach (var code in keys) {
                    if (UnityEngine.Input.GetKeyDown(code)) {
                        return true;
                    }
                }
                return false;
            }

            if (keyHit(this.ScaleUpKeys)) this.Scale++;
            if (keyHit(this.ScaleDownKeys)) this.Scale--;
        }

        private void OnValidate() {
            UpdateScale();
        }
    }
}