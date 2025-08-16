using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.SceneStuff {
    /// <summary>
    /// Class that manages prefabs that should be in every scene
    /// without you having to put them in every scene manually.
    /// If you have multiple types of scenes (e.g. menu, in-game
    /// I recommend having an enum or something that you load
    /// assets based off of in LoadAdditional).
    /// 
    /// Load order:
    /// OnBeforeLoadPersistent
    /// PersistentObjects
    /// OnAfterLoadPersistent
    /// OnBeforeLoadSceneScoped
    /// SceneScopedObjects
    /// OnAfterLoadSceneScoped
    /// OnAfterAwake
    /// </summary>
    [DefaultExecutionOrder(-5)]
    public class ScenePrefabLoader : MonoBehaviour {
        [Tooltip("Persistent prefabs to load. Will load in the provided order.")]
        [SerializeField]
        private GameObject[] PersistentPrefabs;
        [Tooltip("Prefabs to load for the current scene. Will load in the provided order.")]
        [SerializeField]
        private GameObject[] SceneScopedPrefabs;

        private PersistentContainer ddolContainer;

        // Start is called before the first frame update
        void Awake() {
            if (PersistentContainer.INSTANCE != null) {
                ddolContainer = PersistentContainer.INSTANCE;
            } else {
                GameObject obj = new GameObject();
                obj.name = "Persistent Container";
                ddolContainer = obj.AddComponent(typeof(PersistentContainer)) as PersistentContainer;
            }

            OnBeforeLoadPersistent();
            LoadAllPersistentPrefabs(PersistentPrefabs);
            OnAfterLoadPersistent();

            OnBeforeLoadSceneScoped();
            LoadAllPrefabs(SceneScopedPrefabs);
            OnAfterLoadSceneScoped();

            OnAfterAwake();
        }

        protected void LoadAllPrefabs(GameObject[] objects) {
            if (objects == null) return;
            foreach (GameObject go in objects) {
                LoadPrefab(go);
            }
        }

        protected void LoadPrefab(GameObject go) {
            if (go == null) {
                Debug.LogWarning("Attempting to load null prefab.");
                return;
            }
            Instantiate(go, this.transform);
        }

        protected void LoadAllPersistentPrefabs(GameObject[] objects) {
            if (objects == null) return;
            foreach (GameObject go in objects) {
                LoadPersistentPrefab(go);
            }
        }

        protected void LoadPersistentPrefab(GameObject go) {
            if (go == null) {
                Debug.LogWarning("Attempting to load null persistent prefab.");
                return;
            }
            ddolContainer.MaybeInstantiateObject(go);
        }

        /// <summary>
        /// For persistent objects you want to load before the others. Load with LoadPersistentPrefab() or LoadAllPersistentPrefabs().
        /// </summary>
        protected virtual void OnBeforeLoadPersistent() { }

        /// <summary>
        /// For persistent objects you want to load after the others. Load with LoadPersistentPrefab() or LoadAllPersistentPrefabs().
        /// </summary>
        protected virtual void OnAfterLoadPersistent() { }

        /// <summary>
        /// For scene-scoped objects you want to load before the others.
        /// </summary>
        protected virtual void OnBeforeLoadSceneScoped() {}

        /// <summary>
        /// For scene-scoped objects you want to load before the others.
        /// </summary>
        protected virtual void OnAfterLoadSceneScoped() {}

        /// <summary>
        /// After all the rest is done loading.
        /// </summary>
        protected virtual void OnAfterAwake() {

        }
    }
}