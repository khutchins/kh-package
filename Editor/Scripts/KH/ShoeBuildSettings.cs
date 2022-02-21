using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KH.Editor {
    public class ShoeBuildSettings : ScriptableObject {
        public const string k_ShoeBuildSettingsFolder = "Assets/Editor";
        public const string k_ShoeBuildSettingsPath = "Assets/Editor/ShoeBuildSettings.asset";

        public bool Windows;
        public bool Linux;
        public bool Mac;
        public bool WebGL;
        public ShoeBuild.BuildFlavor[] Flavors;

        [HideInInspector]
        public string SelectedPlatform;
        [HideInInspector]
        public string SelectedFlavor;

        internal static ShoeBuildSettings GetOrCreateSettings() {
            var settings = AssetDatabase.LoadAssetAtPath<ShoeBuildSettings>(k_ShoeBuildSettingsPath);
            if (settings == null) {
                settings = ScriptableObject.CreateInstance<ShoeBuildSettings>();
                settings.Windows = true;
                settings.SelectedPlatform = BuildGame.PLATFORM_WIN.Name;
                settings.SelectedFlavor = "itchio";
                settings.Flavors = new ShoeBuild.BuildFlavor[] {
                    new ShoeBuild.BuildFlavor() { Name = "itchio", Enabled = true },
                    new ShoeBuild.BuildFlavor() { Name = "steam", Enabled = false },
                };
                if (!AssetDatabase.IsValidFolder(k_ShoeBuildSettingsFolder)) {
                    AssetDatabase.CreateFolder("Assets", "Editor");
				}
                AssetDatabase.CreateAsset(settings, k_ShoeBuildSettingsPath);
                AssetDatabase.SaveAssets();
            }
            return settings;
        }

        internal static SerializedObject GetSerializedSettings() {
            return new SerializedObject(GetOrCreateSettings());
        }

        public BuildGame.Platform PlatformForSelected(IEnumerable<BuildGame.Platform> platforms) {
            return platforms.Where(x => x.Name == SelectedPlatform).FirstOrDefault();
		}
    }
}