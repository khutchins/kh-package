using Ratferences;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KH.Achievements {
    public interface IAchievementPersister {
        public void Save(IEnumerable<string> unlockedAchievements);
        public IEnumerable<string> Load();
    }

    public class AchievementManager : SingletonInstance<AchievementManager> {
        [SerializeField] StringSignal UnlockSignal;

        private HashSet<string> _achievementStatus = new HashSet<string>();
        private IAchievementPersister _persistance;

        protected override void Awake() {
            base.Awake();
            _persistance = GetComponent<IAchievementPersister>();
            if (_persistance == null) {
                _persistance = new PlayerPrefsAchievementPersister();
            }
            LoadFromDisk();
        }

        public void Unlock(string id, bool silent) {
            if (!silent && UnlockSignal != null) {
                UnlockSignal.Raise(id);
            }
        }

        public void Lock(string id) {
            _achievementStatus.Remove(id);
        }

        public void LockAll() {
            _achievementStatus.Clear();
        }

        public void SaveToDisk() {
            _persistance.Save(_achievementStatus);
        }

        private void LoadFromDisk() {
            _achievementStatus.Clear();
            _achievementStatus.Union(_persistance.Load());
        }

        public IEnumerable<string> AllUnlockedAchievements() {
            return _achievementStatus;
        }

        class PlayerPrefsAchievementPersister : IAchievementPersister {
            public string Key;

            public PlayerPrefsAchievementPersister(string key = "achievements") {
                Key = key;
            }

            public IEnumerable<string> Load() {
                return PlayerPrefs.GetString(Key, "").Split(";").Where(x => x.Length > 0);
            }

            public void Save(IEnumerable<string> unlockedAchievements) {
                PlayerPrefs.SetString(Key, string.Join(";", unlockedAchievements));
            }
        }
    }
}