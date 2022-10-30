using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace KH.References {
    public static class DictionarySerializer {

        private const char KEY_VALUE_DELIMITER = '=';
        private const char ENTRY_DELIMITER = '\n';

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void SyncWebGLFileSystem();
#endif

        public static string Serialize(Dictionary<string, string> dictionary) {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in dictionary) {
                if (kvp.Key.Contains(KEY_VALUE_DELIMITER)) {
                    Debug.LogWarning($"Key {kvp.Key} contains '{KEY_VALUE_DELIMITER}'. Key will be skipped for serialization.");
                } else if (kvp.Value.Contains(ENTRY_DELIMITER)) {
                    Debug.LogWarning($"Key {kvp.Key} has value that contains a newline. Key will be skipped for serialization.");
                } else {
                    sb.Append($"{kvp.Key}={kvp.Value}{ENTRY_DELIMITER}");
                }
            }
            // Remove terminal newline.
            if (sb.Length > 0) sb.Remove(sb.Length - 1, 1);
            return sb.ToString();
        }

        private static void SyncFS() {
#if UNITY_WEBGL && !UNITY_EDITOR
            if (Application.platform == RuntimePlatform.WebGLPlayer) { 
                SyncWebGLFileSystem();
            }
#endif
        }

        /// <summary>
        /// Returns a persistent path with the provided filename. Differs slightly from
        /// using Application.persistentDataPath directly in that it will return a static
        /// path (using Application.productName) in WebGL.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static string PersistentPath(string filename) {
            string basePath = Application.persistentDataPath;
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                basePath = $"/idbfs/{Regex.Replace(Application.productName, "[^a-zA-Z0-9]", String.Empty)}/";
            }
            return Path.Combine(basePath, filename);
        }

        public static Dictionary<string, string> Load(string serialized) {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            if (serialized == null) return dictionary;
            foreach (string str in serialized.Split(ENTRY_DELIMITER)) {
                if (!str.Contains(KEY_VALUE_DELIMITER)) {
                    Debug.LogWarning($"Expected serialized line to contain '{KEY_VALUE_DELIMITER}'. Skipping line: {str}");
                } else {
                    string[] split = str.Split(KEY_VALUE_DELIMITER);
                    dictionary.Add(split[0], String.Join(KEY_VALUE_DELIMITER.ToString(), split.Skip(1).ToArray()));
                }
            }
            return dictionary;
        }

        public static string PrefixedKey(string str, string prefix) {
            return str + prefix;
        }

        public static Dictionary<string, string> AddDict(Dictionary<string, string> original, Dictionary<string, string> addl) {
            foreach (var pair in addl) {
                if (original.ContainsKey(pair.Key)) {
                    Debug.LogWarning($"Save dictionary already contains key \"{pair.Key}\"");
                }
                original[pair.Key] = pair.Value;
            }
            return original;
        }

        public static int ReadInt(Dictionary<string, string> dictionary, string key, int defaultValue) {
            if (!dictionary.ContainsKey(key)) return defaultValue;
            if (!int.TryParse(dictionary[key], out int value)) {
                Debug.LogWarning($"Tried to read key {key} but it couldn't be parsed. Value: {dictionary[key]}");
                return defaultValue;
            }
            return value;
        }

        public static bool ReadBool(Dictionary<string, string> dictionary, string key, bool defaultValue) {
            if (!dictionary.ContainsKey(key)) return defaultValue;
            return dictionary[key] == "0" ? false : true;
        }

        public static void SaveToDisk(string filepath, Dictionary<string, string> contents) {
            string dir = Path.GetDirectoryName(filepath);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllText(filepath, Serialize(contents));
            SyncFS();
        }

        public static Dictionary<string, string> LoadFromDisk(string filepath) {
            string text = null;
            if (File.Exists(filepath)) {
                text = File.ReadAllText(filepath);
            }
            return Load(text);
        }

        public static string ToCopyString(Dictionary<string, string> contents, string saveKey) {
            var saveDict = new Dictionary<string, string>(contents);
            saveDict["_type"] = saveKey;
            string str = Serialize(saveDict);
            return $"!!!{Convert.ToBase64String(Encoding.UTF8.GetBytes(str))}!!!";
        }
    }
}
