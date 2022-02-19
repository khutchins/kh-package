using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace KH.Editor {
    public class ShoeBuild : EditorWindow {
        // Add menu item named "My Window" to the Window menu
        [MenuItem("Window/ShoeBuild")]
        public static void ShowWindow() {
            //Show existing window instance. If one doesn't exist, make one.
            EditorWindow.GetWindow(typeof(ShoeBuild));
        }

        [System.Serializable]
        public struct BuildFlavor {
            public string Name;
            public bool Enabled;
		}

        public bool Windows = true;
        public bool Linux;
        public bool Mac;
        public bool WebGL;
        public string[] FlavorStrings;
        //= new string[] {
        //    "itchio",
        //    "steam"
        //};
        public BuildFlavor[] Flavors;

        SerializedObject SO;
        SerializedProperty[] PlatformProps;
        SerializedProperty FlavorsProp;

        private void OnEnable() {
			Flavors = new BuildFlavor[] {
				new BuildFlavor() { Name = "itchio", Enabled = true },
				new BuildFlavor() { Name = "steam", Enabled = false},
			};
			SO = new SerializedObject(this);
            PlatformProps = new SerializedProperty[] {
                SO.FindProperty("Windows"),
                SO.FindProperty("Linux"),
                SO.FindProperty("Mac"),
                SO.FindProperty("WebGL"), 
            };
            FlavorsProp = SO.FindProperty("Flavors");
		}

        private void OnGUI() {
            GUILayout.Label("Platforms", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                foreach (SerializedProperty prop in PlatformProps) {
                    EditorGUILayout.PropertyField(prop);
                }
                using (new GUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Select All")) {
                        foreach (SerializedProperty prop in PlatformProps) {
                            prop.boolValue = true;
                        }
                    }
                    if (GUILayout.Button("Deselect All")) {
                        foreach (SerializedProperty prop in PlatformProps) {
                            prop.boolValue = false;
                        }
                    }
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("Build Flavors", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
				EditorGUILayout.PropertyField(FlavorsProp);
            }

            GUILayout.Space(10);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField("Builds will pull external files from all relevant directories in the Assets/Output/ folder. To only copy files for certain platforms or flavors, put them in subfolders with the name of the flavor or platform (e.g. lin, win, or mac for platforms and itchio or steam for flavors). These subfolders can be nested. Only files and subfolders of a folder named 'all' will be copied, so if you want a file to show up in all builds, put it in Assets/Output/all/file.txt. If you want it to only show up on Windows Steam builds, put it in Assets/Output/win/steam/file.txt or Assets/Output/steam/win/file.txt");
			}

            SO.ApplyModifiedProperties();

            if (GUILayout.Button("Build Game")) {
                List<BuildGame.Platform> plats = new List<BuildGame.Platform>();
                if (Windows) plats.Add(BuildGame.PLATFORM_WIN);
                if (Mac) plats.Add(BuildGame.PLATFORM_MAC);
                if (Linux) plats.Add(BuildGame.PLATFORM_LINUX);
                if (WebGL) plats.Add(BuildGame.PLATFORM_WEBGL);
                BuildGame.BuildForPlatforms(plats.ToArray(), Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray());
			}
        }
    }
}