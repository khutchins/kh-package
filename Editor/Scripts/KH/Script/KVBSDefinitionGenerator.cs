using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace KH.Script {
    public class KVBSDefinitionGenerator {
        private static Dictionary<Type, string> _aliasMap;

        private const string OutputDirectory = "Assets/KVBScripts";

        [MenuItem("Tools/KH.Tools/Generate Definition Files (from Attributes)")]
        public static void GenerateDefinitionFiles() {
            _aliasMap = new Dictionary<Type, string>();
            Directory.CreateDirectory(OutputDirectory);

            GenerateAliases(OutputDirectory);
            GenerateCommands(OutputDirectory);

            AssetDatabase.Refresh();
            Debug.Log($"KVBS definition generation complete.");
        }

        static string GetTypeString(object typeObject) {
            // Prioritize aliases.
            if (typeObject is Type type && _aliasMap.ContainsKey(type)) {
                return _aliasMap[type];
            }

            // If type, enumerate options.
            if (typeObject is Type assetType && typeof(UnityEngine.Object).IsAssignableFrom(assetType)) {
                return GetTypeStringFor(assetType);
            }

            if (typeObject is string typeName) {
                return typeName;
            }

            Debug.LogWarning($"Could not resolve type '{typeObject}'. Defaulting to 'never'.");
            return "never";
        }

        private static string GetTypeStringFor(Type type) {
            string[] guids = AssetDatabase.FindAssets($"t:{type.Name}");
            if (guids.Length == 0) {
                // If no assets are found, the type is unusable.
                return "never";
            }

            // Get the asset names and escape them for placing in "quotes".
            var assetNames = guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(Path.GetFileNameWithoutExtension)
                .OrderBy(x => x)
                .Select(Quotize);

            // Join the names into a union type: ("asset1" | "asset2" | "asset3")
            return $"({string.Join(" | ", assetNames)})";
        }

        private static string Quotize(string str) {
            return $"\"{str.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }

        /// <summary>
        /// Finds all Enums and ScriptableObjects with the [KVBSAlias] attribute,
        /// generates the corresponding 'alias' definitions, and populates the internal alias map.
        /// </summary>
        private static void GenerateAliases(string targetDirectory) {
            var allTypesWithAlias = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type => type.IsDefined(typeof(KVBSAliasAttribute), false));

            var aliasesByFile = new Dictionary<string, StringBuilder>();

            foreach (var type in allTypesWithAlias) {
                var aliasAttr = (KVBSAliasAttribute)type.GetCustomAttribute(typeof(KVBSAliasAttribute));
                string unionString;

                if (type.IsClass) {
                    // For classes, validate that they inherit from ScriptableObject.
                    if (!typeof(ScriptableObject).IsAssignableFrom(type)) {
                        Debug.LogError($"Attribute [KVBSAlias] on class '{type.Name}' is invalid. This attribute can only be used on classes that inherit from 'ScriptableObject'. Skipping alias generation for this type.");
                        continue;
                    }

                    unionString = GetTypeStringFor(type);
                } else {
                    Debug.LogError($"Attribute [KVBSAlias] on '{type.Name}' is invalid. Skipping alias generation for this type.");
                    continue;
                }

                // If the type is valid, add it to our map for the next phase.
                _aliasMap[type] = type.Name;

                // Group aliases into files based on their class name for better organization.
                string className = type.DeclaringType?.Name ?? type.Name;
                if (!aliasesByFile.ContainsKey(className)) {
                    aliasesByFile[className] = new StringBuilder();
                    aliasesByFile[className].AppendLine($"# This file is auto-generated for aliases defined in '{className}'.");
                    aliasesByFile[className].AppendLine("# Do not edit it manually, as your changes will be overwritten.\n");
                }
                aliasesByFile[className].AppendLine($"alias {type.Name} {unionString}\n");
            }

            // Write all the discovered alias definitions to their respective files.
            foreach (var pair in aliasesByFile) {
                string filePath = Path.Combine(targetDirectory, $"{pair.Key}.def.kvbs");
                File.WriteAllText(filePath, pair.Value.ToString());
            }
        }

        /// <summary>
        /// Finds all methods with the [KVBSCommand] attribute and generates 'cmd' definitions.
        /// </summary>
        private static void GenerateCommands(string targetDirectory) {
            var allMethodsWithCommands = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public))
                .Where(method => method.IsDefined(typeof(KVBSCommandAttribute), false));

            var commandsByClass = allMethodsWithCommands.GroupBy(m => m.DeclaringType);

            foreach (var classGroup in commandsByClass) {
                var className = classGroup.Key.Name;
                var fileContent = new StringBuilder();
                fileContent.AppendLine($"# This file is auto-generated for commands defined in '{className}'.");
                fileContent.AppendLine("# Do not edit it manually, as your changes will be overwritten.\n");

                foreach (var method in classGroup) {
                    var cmdAttr = (KVBSCommandAttribute)method.GetCustomAttribute(typeof(KVBSCommandAttribute));
                    var argAttrs = (KVBSArgAttribute[])method.GetCustomAttributes(typeof(KVBSArgAttribute));

                    // Format documentation
                    fileContent.AppendLine($"# {cmdAttr.Documentation}");
                    foreach (var arg in argAttrs) {
                        fileContent.AppendLine($"# @param {arg.Name} {arg.Documentation}");
                    }

                    // Format command signature
                    var signatureParts = new List<string> { "cmd", cmdAttr.Name };
                    foreach (var arg in argAttrs) {
                        string typeString = GetTypeString(arg.Type);
                        string variadicPrefix = arg.IsVariadic ? "..." : "";
                        signatureParts.Add($"{arg.Name}:{variadicPrefix}{typeString}");
                    }

                    fileContent.AppendLine(string.Join(" ", signatureParts));
                    fileContent.AppendLine();
                }

                // Append the command definitions to the file.
                // This safely handles cases where a class might already have an alias file.
                string filePath = Path.Combine(targetDirectory, $"{className}.def.kvbs");
                File.AppendAllText(filePath, fileContent.ToString());
            }
        }
    }
}