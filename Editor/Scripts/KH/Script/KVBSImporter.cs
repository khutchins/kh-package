#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KH.Script {
    [ScriptedImporter(version: 1, ext: "kvbs", AllowCaching = true)]
    public class KHScriptImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            // Read file as UTF8 (with BOM tolerance)
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var text = Encoding.UTF8.GetString(bytes);

            // Create the ScriptableObject and populate it
            var asset = ScriptableObject.CreateInstance<KVBSAsset>();
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            asset.SetText(text);

            // Add to asset database
            ctx.AddObjectToAsset("KHScriptAsset", asset);
            ctx.SetMainObject(asset);
        }
    }
}
#endif
