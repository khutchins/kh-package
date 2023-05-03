using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace KH {
    public static class IOHelper {
        public static void EnsurePathAndWriteAllBytes(string path, byte[] bytes) {
            string dir = Path.GetDirectoryName(path);
            if (!Directory.Exists(dir)) {
                Directory.CreateDirectory(dir);
            }
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// The equivalent of Application.persistentDataPath, but modified on WebGL targets
        /// to not change with each build.
        /// </summary>
        public static string PersistentPath(string filename) {
            string basePath = Application.persistentDataPath;
            if (Application.platform == RuntimePlatform.WebGLPlayer) {
                basePath = $"/idbfs/{Regex.Replace(Application.productName, "[^a-zA-Z0-9]", String.Empty)}/";
            }
            return Path.Combine(basePath, filename);
        }
    }
}