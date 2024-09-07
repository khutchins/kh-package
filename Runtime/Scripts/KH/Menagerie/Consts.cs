using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KH.Menagerie {
    public static class Consts {
        public const string FOLDER = "Menagerie";

        public static string PersistentPath(string fileName) {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
            string pathStr = Application.persistentDataPath;
            string dir = Path.GetFileName(pathStr);
            DirectoryInfo parent = Directory.GetParent(pathStr);
            return Path.Combine(MenagerieFolder, dir, fileName);
#else
        return IOHelper.PersistentPath(fileName);
#endif
        }

        public static string MenageriePath(string fileName) {
            return Path.Combine(MenagerieFolder, fileName);
        }

        public static string MenagerieFolder {
            get {
#if (UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN)
                string pathStr = Application.persistentDataPath;
                string dir = Path.GetFileName(pathStr);
                DirectoryInfo parent = Directory.GetParent(pathStr);
                return Path.Combine(parent.ToString(), FOLDER);
#else
            return IOHelper.PersistentPath("");
#endif
            }
        }
    }
}