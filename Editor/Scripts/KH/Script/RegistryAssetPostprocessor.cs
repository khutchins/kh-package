// File: Editor/AssetRegistryUpdater.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class RegistryAssetPostprocessor : AssetPostprocessor {

    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) {
        bool needsUpdate = importedAssets.Any(path => path.EndsWith(".asset")) ||
                           deletedAssets.Any(path => path.EndsWith(".asset"));

        if (!needsUpdate) return;

        // Use delayCall to ensure the AssetDatabase is fully updated before we query it.
        EditorApplication.delayCall += UpdateAllRegistries;
    }

    [MenuItem("Tools/KH.Tools/Update All Asset Registries")]
    public static void UpdateAllRegistries() {
        var registryTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => IsSubclassOfRawGeneric(typeof(RegistrySO<>), type) && !type.IsAbstract);
        bool registryWasUpdated = false;

        foreach (var registryType in registryTypes) {
            // Find the assets for this registry type.
            var guids = AssetDatabase.FindAssets($"t:{registryType.Name}");
            if (guids.Length == 0) {
                Debug.Log($"No registry asset found for type {registryType.Name}. Skipping.");
                continue;
            }

            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            var registryAsset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(path);
            if (registryAsset == null) continue;

            // Figure out what type of asset this registry holds.
            Type assetType = null;
            Type currentType = registryType;
            while (currentType != null && currentType != typeof(object)) {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(RegistrySO<>)) {
                    assetType = currentType.GetGenericArguments()[0];
                    break;
                }
                currentType = currentType.BaseType;
            }

            if (assetType == null) {
                Debug.LogWarning($"Could not determine the asset type for registry '{registryType.Name}'. Skipping.");
                continue;
            }

            // Type shenanigans to get it to let me assign to a specific list type of a generic.
            Type genericListType = typeof(List<>);
            Type specificListType = genericListType.MakeGenericType(assetType);
            var allAssets = (IList)Activator.CreateInstance(specificListType);

            // Find all assets of that type in the project
            var allAssetGuids = AssetDatabase.FindAssets($"t:{assetType.Name}");
            foreach (var guid in allAssetGuids) {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                var asset = AssetDatabase.LoadAssetAtPath(assetPath, assetType);
                if (asset != null) {
                    allAssets.Add(asset);
                }
            }

            // Use reflection to set the private list on the registry asset
            var editorListProperty = registryType.GetProperty("Items");
            if (editorListProperty != null) {
                editorListProperty.SetValue(registryAsset, allAssets);
                EditorUtility.SetDirty(registryAsset);
                registryWasUpdated = true;
                Debug.Log($"{registryType.Name} automatically updated. Found {allAssets.Count} assets of type {assetType.Name}.");
            }
        }

        // If any registry was updated, regenerate the KVBS definitions.
        if (registryWasUpdated) {
            KH.Script.KVBSDefinitionGenerator.GenerateDefinitionFiles();
        }
    }

    static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
        while (toCheck != null && toCheck != typeof(object)) {
            var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
            if (generic == cur) return true;
            toCheck = toCheck.BaseType;
        }
        return false;
    }
}