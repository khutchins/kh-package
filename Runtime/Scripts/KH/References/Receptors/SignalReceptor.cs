using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.References {
    /// <summary>
    /// Class that provides a mechanism for raising a signal.
    /// </summary>
    public class SignalReceptor : MonoBehaviour {

        [SerializeField] Signal Signal;

        public void Raise() {
            if (Signal != null) {
                Signal.Raise();
            }
        }
    }
}