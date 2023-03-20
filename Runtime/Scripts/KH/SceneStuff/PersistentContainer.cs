using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.SceneStuff {
    /// <summary>
    /// Class that exists to serve as a repository for persistent objects.
    /// </summary>
    public class PersistentContainer : MonoBehaviour {

        public static PersistentContainer INSTANCE;

        private HashSet<GameObject> _objectsInited = new HashSet<GameObject>();

        void Awake() {
            INSTANCE = this;
            DontDestroyOnLoad(this);
        }

        /// <summary>
        /// Instantiates an object if it doesn't already exist.
        /// </summary>
        /// <param name="prefab"></param>
        public void MaybeInstantiateObject(GameObject prefab) {
            if (_objectsInited.Contains(prefab)) return;
            Instantiate(prefab, this.transform);
        }
    }
}