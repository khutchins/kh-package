using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public class ChordDetection {
        /// <summary>
        /// Checks if the keys are all held down and the trigger is just down.
        /// </summary>
        public static bool Pressed(params KeyCode[] keys) {
            foreach (var key in keys) {
                if (!UnityEngine.Input.GetKey(key)) return false;
            }
            return true;
        }

        /// <summary>
        /// Checks if the keys are all held down and the trigger is just down.
        /// </summary>
        public static bool Pressed(KeyCode trigger, params KeyCode[] keys) {
            return UnityEngine.Input.GetKeyDown(trigger) && Pressed(keys);
        }
    }
}