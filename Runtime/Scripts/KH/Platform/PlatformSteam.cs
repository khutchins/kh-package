using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if BUILD_FLAVOR_STEAM
using Steamworks;
#endif
using System;
using Ratferences;
using KH.Achievements;
using System.Linq;

namespace KH.Platform {
	public class PlatformSteam : SingletonInstance<PlatformSteam> {
		[SerializeField] uint AppId = 884480;
		[SerializeField] StringSignal AchievementUnlockedSignal;
		[SerializeField] BoolReference ShowLocalAchievements;

#if BUILD_FLAVOR_STEAM

		bool _initializeFailed = false;

		void Start() {
			try {
				SteamClient.Init(AppId);
			} catch (Exception e) {
				Debug.LogError("Initialize failed with exception: " + e);
                FailedToInit();
                return;
			}

			if (!SteamClient.IsValid) {
				Debug.LogError("SteamClient not valid");
				FailedToInit();
				return;
			}

			if (ShowLocalAchievements != null) ShowLocalAchievements.Value = false;

			Debug.Log($"Initialized for user: {SteamClient.Name}");

			if (AchievementManager.INSTANCE != null) {
				var cheevos = AchievementManager.INSTANCE.AllUnlockedAchievements();
				foreach (var cheevo in cheevos) {
					UnlockAchievement(cheevo);
				}
			}
		}

		void FailedToInit() {
			_initializeFailed = true; 
			this.gameObject.SetActive(false);
		}

		public void UnlockAchievement(string achievementId) {
			if (!SteamClient.IsValid) return;

			Steamworks.Data.Achievement achievement = SteamUserStats.Achievements.Where(x => x.Identifier == achievementId).FirstOrDefault();
            if (achievement.Identifier != achievementId) {
                Debug.Log($"Could not find achievement with id {achievementId}");
                return;
            }
			if (achievement.State) return;
            Debug.Log($"Unlocked {achievementId}");
            achievement.Trigger();
        }

        private void Update() {
			SteamClient.RunCallbacks();
        }

        private void OnEnable() {
			AchievementUnlockedSignal.OnSignalRaised += UnlockAchievement;
        }

        private void OnDisable() {
            AchievementUnlockedSignal.OnSignalRaised -= UnlockAchievement;
            SteamClient.Shutdown();
        }

		public bool IsValidPlatform() {
			return !_initializeFailed && SteamClient.IsValid;
		}
#endif
	}
}