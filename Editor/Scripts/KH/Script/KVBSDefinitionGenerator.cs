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

        private const string OutputDirectory = "Assets/KVBScripts";

        [MenuItem("Tools/KH.Tools/Generate Definition Files (from Attributes)")]
        public static void GenerateDefinitionFiles() {
            Directory.CreateDirectory(OutputDirectory);

            var decoratedMethods = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                .Where(method => method.IsDefined(typeof(KVBSCommandAttribute), false));

            var methodsByClass = decoratedMethods.GroupBy(method => method.DeclaringType);

            if (!methodsByClass.Any()) {
                Debug.Log($"No methods with [KVBSCommand] attribute found. No definition files generated.");
                return;
            }

            var generatedFiles = new List<string>();

            foreach (var group in methodsByClass) {
                Type declaringType = group.Key;
                string className = declaringType.Name;
                string outputPath = Path.Combine(OutputDirectory, $"{className}.def.kvbs");

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"# This file is auto-generated for the class '{className}'.");
                stringBuilder.AppendLine("# Do not edit this manually, as your changes will be overwritten.");
                stringBuilder.AppendLine();

                var allAttributes = group.SelectMany(method => method.GetCustomAttributes<KVBSCommandAttribute>());

                foreach (var attr in allAttributes.OrderBy(a => a.CommandName)) {
                    string line = GenerateLineForCommand(attr);
                    if (!string.IsNullOrEmpty(line)) {
                        stringBuilder.AppendLine(line);
                    }
                }

                File.WriteAllText(outputPath, stringBuilder.ToString());
                generatedFiles.Add(outputPath);
            }

            AssetDatabase.Refresh();
            Debug.Log($"KVBS definition generation complete. {generatedFiles.Count} files generated in '{OutputDirectory}'.");
        }

        private static string GenerateLineForCommand(KVBSCommandAttribute attr) {
            var parts = new List<string> { attr.CommandName };

            foreach (object arg in attr.Arguments) {
                switch (arg) {
                    case string str:
                        parts.Add(str);
                        break;
                    case Type type when typeof(UnityEngine.Object).IsAssignableFrom(type):
                        string[] assetNames = GetAssetNamesOfType(type);
                        if (assetNames.Length == 0) {
                            Debug.LogWarning($"For command '{attr.CommandName}', no assets of type '{type.Name}' were found. Using 'never' type.");
                            parts.Add("never");
                        } else {
                            string union = $"({string.Join(" | ", assetNames.Select(name => $"\"{name}\""))})";
                            parts.Add(union);
                        }
                        break;
                    default:
                        Debug.LogWarning($"Unsupported argument type '{arg?.GetType().Name}' in command '{attr.CommandName}'. Ignoring.");
                        break;
                }
            }
            return string.Join(" ", parts);
        }

        private static string[] GetAssetNamesOfType(Type type) {
            var guids = AssetDatabase.FindAssets($"t:{type.Name}");
            return guids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>)
                .Where(asset => asset != null)
                .Select(asset => asset.name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();
        }
    }
}