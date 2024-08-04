using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.LowRez {
    [RequireComponent(typeof(Canvas))]
    public class LowRezScaleManager : MonoBehaviour {
        private class GlobalScaleSettings {
            public int Scale;
            public bool AlwaysUseMax;
        }

        [SerializeField] private int _scale;
        [SerializeField] private bool _alwaysUseMax;
        [SerializeField] Vector2 BaseResolution = new Vector2(64, 64);
        [SerializeField] KeyCode[] ScaleUpKeys = new KeyCode[] { KeyCode.Plus, KeyCode.Equals, KeyCode.KeypadPlus };
        [SerializeField] KeyCode[] ScaleDownKeys = new KeyCode[] { KeyCode.Minus, KeyCode.KeypadMinus };
        [SerializeField] ScaleSetting[] Settings;

        private static GlobalScaleSettings _settings;
        private Canvas _canvas;
        private Rect _lastPixelRect = Rect.zero;

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
            get {
                EnsureGlobalSettings();
                return _settings.Scale;
            }
            set {
                EnsureGlobalSettings();
                _settings.Scale = value;
                UpdateScale();
            }
        }

        public bool AlwaysUseMax {
            get {
                EnsureGlobalSettings();
                return _settings.AlwaysUseMax;
            }
            set {
                EnsureGlobalSettings();
                _settings.AlwaysUseMax = value;
                UpdateScale();
            }
        }

        private void EnsureGlobalSettings() {
            if (_settings == null) {
                _settings = new GlobalScaleSettings();
                _settings.Scale = _scale;
                _settings.AlwaysUseMax = _alwaysUseMax;
            }
        }

        private void Awake() {
            EnsureCanvas();
            EnsureGlobalSettings();
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

        private void UpdateScale(int scale) {
            if (_settings != null) {
                _settings.Scale = scale;
            } else {
                _scale = scale;
            }
        }

        void UpdateScale() {
            int maxScale = MaxScale();
            if (AlwaysUseMax) Scale = maxScale;
            UpdateScale(Mathf.Clamp(Scale, 1, maxScale));
            Vector2 size = BaseResolution * Scale;
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

            if (_canvas.pixelRect != _lastPixelRect) {
                UpdateScale();
                _lastPixelRect = _canvas.pixelRect;
            }
        }

        private void OnValidate() {
            //UpdateScale();
        }
    }
}