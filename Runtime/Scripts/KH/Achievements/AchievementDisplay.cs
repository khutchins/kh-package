using Ratferences;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

namespace KH.Achievements {
    [RequireComponent(typeof(Canvas))]
    public class AchievementDisplay : MonoBehaviour {
        [SerializeField] StringSignal AchievementSignal;
        [Tooltip("If this is set, it will use its value to determine if it shows achievements or not. Useful when you don't want to double-show with Steam.")]
        [SerializeField] BoolReference ShowLocalAchievements;
        [SerializeField] AchievementList AchievementList;
        [SerializeField] AchievementPopover AchievementPopover;
        [SerializeField] VerticalLayoutGroup AchievementLayoutGroup;
        [SerializeField] float AnimationTime = 0.25f;
        [SerializeField] float ShowTime = 3f;
        

        private int _achievementHeight;
        private int _achievementYOffset;
        private RectTransform _layoutGroupRect;
        private SingleCoroutineManager _achievementCoroutineManager;
        private List<AchievementList.Achievement>  _enqueuedAchievements = new List<AchievementList.Achievement>();
        private Canvas _canvas;

        private void Awake() {
            _canvas = GetComponent<Canvas>();
            _canvas.enabled = false;
            _achievementYOffset = AchievementLayoutGroup.padding.top;
            _achievementHeight = Mathf.CeilToInt(AchievementPopover.GetComponent<RectTransform>().sizeDelta.y);
            _layoutGroupRect = AchievementLayoutGroup.GetComponent<RectTransform>();
            Debug.Log($"{_achievementHeight} {_achievementYOffset}");

            SetLayoutPadding(ReceededPadding);

            _achievementCoroutineManager = new SingleCoroutineManager(this);
        }

        private void OnEnable() {
            AchievementSignal.OnSignalRaised += AchievementUnlocked;
        }

        private void OnDisable() {
            AchievementSignal.OnSignalRaised -= AchievementUnlocked;
        }

        void AchievementUnlocked(string achievementId) {
            if (ShowLocalAchievements != null && !ShowLocalAchievements.Value) return;
            var achievement = AchievementList.Achievements.Where(x => x.ID == achievementId).FirstOrDefault();
            if (achievement == null) {
                Debug.LogWarning($"Unknown achievement {achievementId}");
            }
            Debug.Log($"Unlocked achievement {achievement.Name}");
            _enqueuedAchievements.Add(achievement);
            _achievementCoroutineManager.MaybeStartCoroutine(ProcessAchievements());
        }
        
        void SetLayoutPadding(int amt) {
            RectOffset padding = AchievementLayoutGroup.padding;
            padding.top = amt;
            padding.bottom = amt;
            AchievementLayoutGroup.padding = padding;
        }

        int ReceededPadding {
            get => -(_achievementYOffset + _achievementHeight);
        }

        int DefaultPadding {
            get => _achievementYOffset;
        }

        IEnumerator ProcessAchievements() {
            _canvas.enabled = true;
            while (_enqueuedAchievements.Count > 0) {

                var achievement = _enqueuedAchievements[0];

                AchievementPopover.SetAchievement(achievement);

                int start = ReceededPadding;
                int end = DefaultPadding;
                yield return EZTween.DoPercentAction(p => {
                    SetLayoutPadding(Mathf.RoundToInt(Mathf.Lerp(start, end, p)));
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroupRect);
                }, AnimationTime, EZTween.Curve.SinEaseInOut, EZTween.TimeGetter.Unscaled);
                yield return new WaitForSecondsRealtime(ShowTime);
                yield return EZTween.DoPercentAction(p => {
                    SetLayoutPadding(Mathf.RoundToInt(Mathf.Lerp(end, start, p)));
                    LayoutRebuilder.ForceRebuildLayoutImmediate(_layoutGroupRect);
                }, AnimationTime, EZTween.Curve.SinEaseInOut, EZTween.TimeGetter.Unscaled);
                yield return new WaitForSecondsRealtime(0.5f);

                if (_enqueuedAchievements.Count > 0) {
                    _enqueuedAchievements.RemoveAt(0);
                }
            }
            _canvas.enabled = false;
        }
    }
}