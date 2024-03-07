using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH {
    public class SingletonInstance<T> : MonoBehaviour where T : MonoBehaviour {
        public static T INSTANCE {
            get; private set;
        }

        protected virtual void Awake() {
            INSTANCE = this as T;
        }
    }
}