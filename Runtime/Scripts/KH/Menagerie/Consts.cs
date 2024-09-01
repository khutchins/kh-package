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
            return Path.Combine(parent.ToString(), FOLDER, dir, fileName);
#else
        return IOHelper.PersistentPath(fileName);
#endif
        }
    }
}