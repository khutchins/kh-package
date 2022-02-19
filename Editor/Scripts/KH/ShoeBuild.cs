using System;
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

        private BuildGame.Platform _selectedPlatform;
        private string _selectedFlavor;

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

            GUILayout.Label("Build", EditorStyles.boldLabel);

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.LabelField("Builds will pull external files from all relevant directories in the Assets/Output/ folder. To only copy files for certain platforms or flavors, put them in subfolders with the name of the flavor or platform (e.g. lin, win, or mac for platforms and itchio or steam for flavors). These subfolders can be nested. Only files and subfolders of a folder named 'all' will be copied, so if you want a file to show up in all builds, put it in Assets/Output/all/file.txt. If you want it to only show up on Windows Steam builds, put it in Assets/Output/win/steam/file.txt or Assets/Output/steam/win/file.txt");
			}

            SO.ApplyModifiedProperties();

            List<BuildGame.Platform> plats = new List<BuildGame.Platform>();
            if (Windows) plats.Add(BuildGame.PLATFORM_WIN);
            if (Mac) plats.Add(BuildGame.PLATFORM_MAC);
            if (Linux) plats.Add(BuildGame.PLATFORM_LINUX);
            if (WebGL) plats.Add(BuildGame.PLATFORM_WEBGL);

            GUILayout.Label("Build Everything");

            if (GUILayout.Button("Build All Enabled Permutations")) {
                BuildGame.BuildForPlatforms(plats.ToArray(), Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray());
			}

            GUILayout.Label("Build Subset");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label("Specific Platform", GUILayout.Width(100));
                    int idx = plats.IndexOf(_selectedPlatform);
                    if (idx < 0) idx = 0;
                    idx = EditorGUILayout.Popup(idx, plats.Select(x => x.Name).ToArray());
                    _selectedPlatform = idx >= 0 && idx < plats.Count ? plats[idx] : null;
                    if (GUILayout.Button("Build All Flavors for Platform")) {
                        if (_selectedPlatform == null) {
                            Debug.LogError("No build platform selected");
                        } else {
                            BuildGame.BuildForPlatforms(new BuildGame.Platform[] { _selectedPlatform }, Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray());
                        }
                    }
                }

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label("Specific Flavor", GUILayout.Width(100));
                    string[] enabledFlavors = Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray();
                    int idx = Array.IndexOf(enabledFlavors, _selectedFlavor);
                    if (idx < 0) idx = 0;
                    idx = EditorGUILayout.Popup(idx, enabledFlavors);
                    _selectedFlavor = (idx >= 0 && idx < enabledFlavors.Length) ? enabledFlavors[idx] : null;
                    if (GUILayout.Button("Build All Platforms for Flavor")) {
                        if (_selectedFlavor == null) {
                            Debug.LogError("No build flavor selected.");
                        } else {
                            BuildGame.BuildForPlatforms(plats.ToArray(), new string[] { _selectedFlavor });
                        }
                    }
                }

                if (GUILayout.Button("Build for Selected Platform and Flavor")) {
                    if (_selectedPlatform == null) {
                        Debug.LogError("No build platform selected");
                    } else if (_selectedFlavor == null) {
                        Debug.LogError("No build flavor selected.");
                    } else {
                        BuildGame.BuildForPlatforms(new BuildGame.Platform[] { _selectedPlatform }, new string[] { _selectedFlavor });
                    }
                }
            }
        }
    }
}