using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace KH.Editor {
    public class ShoeBuildSettingsProvider : SettingsProvider {

        private ShoeBuildSettings _settings;
        private SerializedObject _serializedSettings;

        class Styles {
            public static GUIContent number = new GUIContent("My Number");
            public static GUIContent someString = new GUIContent("Some string");
        }

        public ShoeBuildSettingsProvider(string path, SettingsScope scope = SettingsScope.User)
            : base(path, scope) { }

        public override void OnActivate(string searchContext, VisualElement rootElement) {
            // This function is called when the user clicks on the MyCustom element in the Settings window.
            _serializedSettings = ShoeBuildSettings.GetSerializedSettings();
        }

        public override void OnGUI(string searchContext) {
            if (_settings == null) {
                _settings = ShoeBuildSettings.GetOrCreateSettings();
                _serializedSettings = ShoeBuildSettings.GetSerializedSettings();
            }
            ShoeBuild.SharedGUI(false, _settings, _serializedSettings);

            _serializedSettings.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.SaveAssetIfDirty(_settings);
        }

        [SettingsProvider]
        public static SettingsProvider CreateMyCustomSettingsProvider() {

            var provider = new ShoeBuildSettingsProvider("Project/ShoeBuild", SettingsScope.Project);

			// Automatically extract all keywords from the Styles.
			provider.keywords = GetSearchKeywordsFromGUIContentProperties<Styles>();
			return provider;
        }
    }
}