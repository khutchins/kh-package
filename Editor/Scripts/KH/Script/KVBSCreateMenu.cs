#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace KH.Script {
    public static class KVBSCreateMenu {
        [MenuItem("Assets/Create/KH/Script/KVBS Script", priority = 0)]
        public static void CreateKVBSFile() {
            var path = AssetDatabase.GenerateUniqueAssetPath(
                Path.Combine(GetSelectedPathOrFallback(), "script.kvbs"));
            File.WriteAllText(path, "");
            AssetDatabase.ImportAsset(path);
            Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(path);
            EditorGUIUtility.PingObject(Selection.activeObject);
        }

        private static string GetSelectedPathOrFallback() {
            var path = "Assets";
            foreach (var obj in Selection.GetFiltered<Object>(SelectionMode.Assets)) {
                var p = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(p)) {
                    if (File.Exists(p)) return Path.GetDirectoryName(p);
                    if (Directory.Exists(p)) return p;
                }
            }
            return path;
        }
    }
}

#endif