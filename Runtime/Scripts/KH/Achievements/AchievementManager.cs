using Ratferences;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace KH.Achievements {
    public interface IAchievementPersister {
        public void Save(IEnumerable<string> unlockedAchievements);
        public IEnumerable<string> Load();
    }

    public class AchievementManager : SingletonInstance<AchievementManager> {
        [SerializeField] StringSignal UnlockSignal;
        [Tooltip("If true, will load the achievements from disk and union them with the in-memory copy to help protect against writes from multiple processes.")]
        [SerializeField] bool RefreshBeforeSave;

        public bool ManualSavesOnly = false;
        private HashSet<string> _achievementStatus = new HashSet<string>();
        private List<string> _deferredAchievements = new List<string>();
        private IAchievementPersister _persistance;

        protected override void Awake() {
            base.Awake();
            _persistance = GetComponent<IAchievementPersister>();
            if (_persistance == null) {
                _persistance = new KVBDSLAchievementPersister();
            }
            LoadFromDisk();
        }

        /// <summary>
        /// Tells the manager to unlock the achievement. This will automatically save the new
        /// achievement list to disk, unless <see cref="ManualSavesOnly"/> is enabled. This
        /// achievement will be notified immediately.
        /// </summary>
        /// <param name="id">Achievement id to unlock.</param>
        public void Unlock(string id) {
            Unlock(id, false);
        }

        /// <summary>
        /// Tells the manager to unlock the achievement. This will automatically save the new
        /// achievement list to disk, unless <see cref="ManualSavesOnly"/> is enabled.
        /// </summary>
        /// <param name="id">Achievement id to unlock.</param>
        /// <param name="deferNotifications">If true, will not notify about the unlocked 
        /// achievement until <see cref="FlushDeferredNotifications"/> is called. Useful if an 
        /// achievement would ruin a moment. Does not interfere with saving.</param>
        public void Unlock(string id, bool deferNotifications) {
            if (_achievementStatus.Contains(id)) return;

            _achievementStatus.Add(id);
            if (!deferNotifications) {
                Notify(id);
            } else if (deferNotifications) {
                _deferredAchievements.Add(id);
            }

            MaybeSave();
        }

        /// <summary>
        /// Notifies listeners about all deferred notifications.
        /// </summary>
        public void FlushDeferredNotifications() {
            foreach (string id in _deferredAchievements) {
                Notify(id);
            }
            _deferredAchievements.Clear();
        }

        private void Notify(string id) {
            if (UnlockSignal != null) {
                UnlockSignal.Raise(id);
            }
        }

        private void MaybeSave() {
            if (ManualSavesOnly) return;
            SaveToDisk();
        }

        public void Lock(string id) {
            if (!_achievementStatus.Contains(id)) return;
            _achievementStatus.Remove(id);
            MaybeSave();
        }

        public void LockAll() {
            _achievementStatus.Clear();
            MaybeSave();
        }

        /// <summary>
        /// Triggers a manual save to disk. Calling this is only necessary if automatic
        /// saving is disabled via <see cref="ManualSavesOnly"/>.
        /// </summary>
        public void SaveToDisk() {
            if (RefreshBeforeSave) {
                _achievementStatus.Union(_persistance.Load());
            }
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