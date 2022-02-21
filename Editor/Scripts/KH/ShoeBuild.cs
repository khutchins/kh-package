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

        private ShoeBuildSettings _settings;
        private SerializedObject _serializedSettings;

        private void OnEnable() {
            _settings = ShoeBuildSettings.GetOrCreateSettings();
            _serializedSettings = ShoeBuildSettings.GetSerializedSettings();
		}

        public static void SharedGUI(bool showBuildButtons, ShoeBuildSettings settings, SerializedObject so) {
            SerializedProperty[] platformProps = new SerializedProperty[] {
                so.FindProperty("Windows"),
                so.FindProperty("Linux"),
                so.FindProperty("Mac"),
                so.FindProperty("WebGL"),
            };
            SerializedProperty flavorsProp = so.FindProperty("Flavors");

            GUILayout.Label("Platforms", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                foreach (SerializedProperty prop in platformProps) {
                    EditorGUILayout.PropertyField(prop);
                }
                using (new GUILayout.HorizontalScope()) {
                    if (GUILayout.Button("Select All")) {
                        foreach (SerializedProperty prop in platformProps) {
                            prop.boolValue = true;
                        }
                    }
                    if (GUILayout.Button("Deselect All")) {
                        foreach (SerializedProperty prop in platformProps) {
                            prop.boolValue = false;
                        }
                    }
                }
            }

            GUILayout.Space(10);

            GUILayout.Label("Build Flavors", EditorStyles.boldLabel);
            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                EditorGUILayout.PropertyField(flavorsProp);
            }

            GUILayout.Space(10);

            GUILayout.Label("Build", EditorStyles.boldLabel);


            List<BuildGame.Platform> plats = new List<BuildGame.Platform>();
            if (settings.Windows) plats.Add(BuildGame.PLATFORM_WIN);
            if (settings.Mac) plats.Add(BuildGame.PLATFORM_MAC);
            if (settings.Linux) plats.Add(BuildGame.PLATFORM_LINUX);
            if (settings.WebGL) plats.Add(BuildGame.PLATFORM_WEBGL);


            if (showBuildButtons) {
                using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                    EditorGUILayout.LabelField("Builds will pull external files from all relevant directories in the Assets/Output/ folder. To only copy files for certain platforms or flavors, put them in subfolders with the name of the flavor or platform (e.g. lin, win, or mac for platforms and itchio or steam for flavors). These subfolders can be nested. Only files and subfolders of a folder named 'all' will be copied, so if you want a file to show up in all builds, put it in Assets/Output/all/file.txt. If you want it to only show up on Windows Steam builds, put it in Assets/Output/win/steam/file.txt or Assets/Output/steam/win/file.txt");
                }

                GUILayout.Label("Build Everything");

                if (GUILayout.Button("Build All Enabled Permutations")) {
                    BuildGame.BuildForPlatforms(plats.ToArray(), settings.Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray());
                }
            }

            GUILayout.Label("Build Subset");

            using (new GUILayout.VerticalScope(EditorStyles.helpBox)) {
                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label("Specific Platform", GUILayout.Width(100));
                    int idx = plats.Select(x => x.Name).ToList().IndexOf(settings.SelectedPlatform);
                    if (idx < 0) idx = 0;
                    idx = EditorGUILayout.Popup(idx, plats.Select(x => x.Name).ToArray());
                    settings.SelectedPlatform = idx >= 0 && idx < plats.Count ? plats[idx].Name : null;
                    if (showBuildButtons) {
                        if (GUILayout.Button("Build All Flavors for Platform")) {
                            if (settings.SelectedPlatform == null) {
                                Debug.LogError("No build platform selected");
                            } else {
                                BuildGame.BuildForPlatforms(new BuildGame.Platform[] { settings.PlatformForSelected(plats) }, settings.Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray());
                            }
                        }
                    }
                }

                using (new GUILayout.HorizontalScope()) {
                    GUILayout.Label("Specific Flavor", GUILayout.Width(100));
                    string[] enabledFlavors = settings.Flavors.Where(x => x.Enabled).Select(x => x.Name).ToArray();
                    int idx = Array.IndexOf(enabledFlavors, settings.SelectedFlavor);
                    if (idx < 0) idx = 0;
                    idx = EditorGUILayout.Popup(idx, enabledFlavors);
                    settings.SelectedFlavor = (idx >= 0 && idx < enabledFlavors.Length) ? enabledFlavors[idx] : null;
                    if (showBuildButtons) {
                        if (GUILayout.Button("Build All Platforms for Flavor")) {
                            if (settings.SelectedFlavor == null) {
                                Debug.LogError("No build flavor selected.");
                            } else {
                                BuildGame.BuildForPlatforms(plats.ToArray(), new string[] { settings.SelectedFlavor });
                            }
                        }
                    }
                }

                if (showBuildButtons) {
                    if (GUILayout.Button("Build for Selected Platform and Flavor")) {
                        BuildGame.Platform platform = settings.PlatformForSelected(plats);
                        if (platform == null) {
                            Debug.LogError("No build platform selected");
                        } else if (settings.SelectedFlavor == null) {
                            Debug.LogError("No build flavor selected.");
                        } else {
                            BuildGame.BuildForPlatforms(new BuildGame.Platform[] { platform }, new string[] { settings.SelectedFlavor });
                        }
                    }
                }
            }
        }

        private void OnGUI() {
            if (_settings == null) {
                _settings = ShoeBuildSettings.GetOrCreateSettings();
                _serializedSettings = ShoeBuildSettings.GetSerializedSettings();
            }

            SharedGUI(true, _settings, _serializedSettings);
            _serializedSettings.ApplyModifiedProperties();
            AssetDatabase.SaveAssetIfDirty(_settings);
        }
    }
}