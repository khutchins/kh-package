#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace KH.Script {
    [CustomEditor(typeof(KVBSAsset))]
    public class KHScriptAssetEditor : UnityEditor.Editor {
        Vector2 scroll;
        public override void OnInspectorGUI() {
            var asset = (KVBSAsset)target;

            EditorGUILayout.LabelField("Script Preview", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            using (new EditorGUI.DisabledScope(true)) {
                var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(160));
                EditorGUILayout.TextArea(asset.Text, style, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }

            EditorGUILayout.HelpBox(
                "This asset is imported from a .kvbs file. Edit the file in your code editor and Unity will reimport automatically.",
                MessageType.Info
            );
        }
    }
}
#endif
