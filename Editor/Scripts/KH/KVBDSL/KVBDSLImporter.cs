#if UNITY_EDITOR
using System.IO;
using System.Text;
using UnityEditor.AssetImporters;
using UnityEngine;

namespace KH.KVBDSL {
    [ScriptedImporter(version: 1, ext: "kvbdsl", AllowCaching = true)]
    public class KVBDSLImporter : ScriptedImporter {
        public override void OnImportAsset(AssetImportContext ctx) {
            var bytes = File.ReadAllBytes(ctx.assetPath);
            var text = Encoding.UTF8.GetString(bytes);

            var asset = ScriptableObject.CreateInstance<KVBDSLAsset>();
            asset.name = Path.GetFileNameWithoutExtension(ctx.assetPath);
            asset.SetText(text);

            ctx.AddObjectToAsset("KHScriptAsset", asset);
            ctx.SetMainObject(asset);
        }
    }
}
#endif
