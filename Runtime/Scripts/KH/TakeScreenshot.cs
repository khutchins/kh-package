using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public class TakeScreenshot : MonoBehaviour {
        void Update() {
            if (UnityEngine.Input.GetKeyDown(KeyCode.F5)) {
                string loc = string.Format("{0}_{1}.png", Application.productName, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
                ScreenCapture.CaptureScreenshot(loc);
                Debug.LogFormat("Captured screenshot {0}.", loc);
            }
        }
    }
}