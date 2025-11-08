using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace KH.SceneStuff {

    [CustomEditor(typeof(GenericScenePrefabLoader))]
    public class GenericScenePrefabLoaderEditor : UnityEditor.Editor {

        SerializedProperty persistentPrefabsProp;
        SerializedProperty sceneScopedPrefabsProp;
        SerializedProperty menuPrefabsProp;
        SerializedProperty bonusPrefabsProp;
        SerializedProperty menuTypeProp;

        void OnEnable() {
            persistentPrefabsProp = serializedObject.FindProperty("PersistentPrefabs");
            sceneScopedPrefabsProp = serializedObject.FindProperty("SceneScopedPrefabs");
            menuPrefabsProp = serializedObject.FindProperty("MenuPrefabs");
            bonusPrefabsProp = serializedObject.FindProperty("BonusPrefabs");
            menuTypeProp = serializedObject.FindProperty("_menuType");
        }

        public override void OnInspectorGUI() {
            serializedObject.ApplyModifiedProperties();

            // Editable if it's the asset itself or it's in prefab mode.
            bool isEditable = PrefabUtility.IsPartOfPrefabAsset(target) || PrefabStageUtility.GetCurrentPrefabStage() != null;

            // Makes it readonly until the value changes.
            GUI.enabled = isEditable;

            if (!isEditable) {
                EditorGUILayout.HelpBox("These lists can only be edited on the prefab asset itself.", MessageType.Warning);
            }

            EditorGUILayout.PropertyField(persistentPrefabsProp, true);
            EditorGUILayout.PropertyField(sceneScopedPrefabsProp, true);

            DrawMenuPrefabsUI();

            GUI.enabled = true;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scene-Specific Settings", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(bonusPrefabsProp, true);
            EditorGUILayout.PropertyField(menuTypeProp);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawMenuPrefabsUI() {
            EditorGUILayout.LabelField("Menu Type Prefabs", EditorStyles.boldLabel);

            // Get all possible values of the MenuType enum
            var menuTypes = System.Enum.GetValues(typeof(GenericScenePrefabLoader.MenuType));

            EditorGUI.indentLevel++;
            foreach (GenericScenePrefabLoader.MenuType menuType in menuTypes) {
                SerializedProperty prefabsForType = GetOrCreatePrefabsListForType(menuType);
                string label = ObjectNames.NicifyVariableName(menuType.ToString()) + " Prefabs";
                EditorGUILayout.PropertyField(prefabsForType, new GUIContent(label), true);
            }
            EditorGUI.indentLevel--;
        }

        private SerializedProperty GetOrCreatePrefabsListForType(GenericScenePrefabLoader.MenuType menuType) {
            for (int i = 0; i < menuPrefabsProp.arraySize; i++) {
                SerializedProperty element = menuPrefabsProp.GetArrayElementAtIndex(i);
                SerializedProperty typeProp = element.FindPropertyRelative("Type");

                if (typeProp.enumValueIndex == (int)menuType) {
                    return element.FindPropertyRelative("Prefabs");
                }
            }

            // Menu type does not exist yet - create it.
            menuPrefabsProp.arraySize++;
            SerializedProperty newElement = menuPrefabsProp.GetArrayElementAtIndex(menuPrefabsProp.arraySize - 1);
            newElement.FindPropertyRelative("Type").enumValueIndex = (int)menuType;
            SerializedProperty newPrefabsList = newElement.FindPropertyRelative("Prefabs");
            newPrefabsList.ClearArray();
            return newPrefabsList;
        }
    }
}