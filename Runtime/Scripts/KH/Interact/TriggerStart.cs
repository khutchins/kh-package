using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
    public class TriggerStart : MonoBehaviour {
        public Action Action;

        void Start() {
            Action.Begin();
        }
    }
}