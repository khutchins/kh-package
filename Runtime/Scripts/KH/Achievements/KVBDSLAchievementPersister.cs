using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif


namespace KH.Achievements {
    public class KVBDSLAchievementPersister : MonoBehaviour, IAchievementPersister {
        [SerializeField] bool UseMenagerieFolder = true;
        [SerializeField] string FileName = "achievements.khini";

        public string SavePath() {
            if (UseMenagerieFolder) {
                return Menagerie.Consts.MenageriePath(FileName);
            } else {
                return IOHelper.PersistentPath(FileName);
            }
        }

        private const string ACHIEVEMENTS_STR = "achievements";

        public IEnumerable<string> Load() {
            List<string> list = new List<string>();
            if (!File.Exists(SavePath())) return list;
            var str = File.ReadAllText(SavePath());
            var contents = new KVBDSL.Deserializer().Parse(str);
            if (contents == null || !contents.ContainsKey(ACHIEVEMENTS_STR)) {
                return list;
            }
            var dict = contents[ACHIEVEMENTS_STR];
            if (dict is not IDictionary) return list;

            foreach (DictionaryEntry entry in dict as IDictionary) {
                if (entry.Value == null) continue;
                if (entry.Value is bool b && b) list.Add(entry.Key as string);
            }

            return list;
        }

        public void Save(IEnumerable<string> unlockedAchievements) {
            var dict = new Dictionary<string, object>();
            dict[ACHIEVEMENTS_STR] = unlockedAchievements.ToDictionary(x => x, x => true);
            var data = new KVBDSL.Serializer().Serialize(dict);
            IOHelper.EnsurePathAndWriteText(SavePath(), data);
        }

#if UNITY_EDITOR
        [ContextMenu("Show File In Explorer")]
        private void OpenEditorToFile() {
            EditorUtility.RevealInFinder(SavePath());
        }
#endif
    }
}