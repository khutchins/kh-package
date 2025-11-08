using System.Collections.Generic;
using UnityEngine;

namespace KH.SceneStuff {
    [DefaultExecutionOrder(-500)]
    public class GenericScenePrefabLoader : ScenePrefabLoader {
        public enum MenuType {
            InGameMenu,
            MainMenu,
            None
        }

        [SerializeField]
        private MenuList[] MenuPrefabs;
        [Tooltip("Prefabs that should only be loaded in this scene.")]
        [SerializeField]
        private GameObject[] BonusPrefabs;


        [SerializeField]
        private MenuType _menuType;

        private class GenericList<T> where T : System.Enum {
            public T Type;
            public GameObject[] Prefabs;

            public static GameObject[] PrefabsForType(GenericList<T>[] items, T type) {
                foreach (var item in items) {
                    if (EqualityComparer<T>.Default.Equals(item.Type, type)) return item.Prefabs;
                }
                Debug.LogWarning("No items found for enum " + type);
                return new GameObject[0];
            }
        }

        [System.Serializable]
        private class MenuList : GenericList<MenuType> { }

        protected override void OnAfterLoadSceneScoped() {
            LoadAllPrefabs(MenuList.PrefabsForType(MenuPrefabs, _menuType));
            if (BonusPrefabs != null) {
                LoadAllPrefabs(BonusPrefabs);
            }
        }
    }
}