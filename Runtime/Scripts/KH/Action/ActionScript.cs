using KH.Actions;
using KH.Script;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Actions {
    public class ActionScript : Action {
        [TextAssetPreview(200)][SerializeField] KVBSAsset ScriptAsset;
        [TextArea][SerializeField] string Script;

        public override void Begin() {
            StartCoroutine(Run());
        }

        IEnumerator Run() {
            var script = ScriptAsset != null ? ScriptAsset.Text : Script;
            yield return ScriptingEngine.INSTANCE.RunAndAwait(script);
            Finished();
        }

#if UNITY_EDITOR
        [ContextMenu("Convert Inline Script to Asset")]
        private void ConvertScriptStringToAsset() {
            if (ScriptAsset != null) {
                Debug.LogWarning($"Script is already populated. Not converting.");
                return;
            }
            if (string.IsNullOrWhiteSpace(Script)) {
                Debug.LogWarning($"Script string is empty. Nothing to convert.");
                return;
            }

            const string rootFolder = "Assets";

            UnityEditor.AssetDatabase.Refresh();
            if (!UnityEditor.AssetDatabase.IsValidFolder(rootFolder)) {
                Debug.LogWarning($"{rootFolder} is not valid. I don't think this can happen?");
                return;
            }

            string defaultName = $"{gameObject.name}";

            string path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                "Save KVBSAsset",
                defaultName,
                "kvbs",
                "Choose where to save the KVB script.",
                rootFolder
            );

            // User decided not to save the asset after all.
            if (string.IsNullOrEmpty(path)) {
                return;
            }

            var asset = ScriptableObject.CreateInstance<KVBSAsset>();
            asset.SetText(Script);

            UnityEditor.AssetDatabase.CreateAsset(asset, path);
            UnityEditor.AssetDatabase.SaveAssets();

            ScriptAsset = asset;
            Script = string.Empty;

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.EditorUtility.SetDirty(asset);
            UnityEditor.AssetDatabase.Refresh();

            Debug.Log($"Converted inline Script to KVBSAsset at '{path}' and assigned to ScriptAsset.");
        }
#endif
    }
}