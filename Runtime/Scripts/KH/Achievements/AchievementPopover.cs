using KH.Achievements;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopover : MonoBehaviour {
    [SerializeField] TMP_Text Name;
    [SerializeField] TMP_Text Description;
    [SerializeField] Image Image;

    public void SetAchievement(AchievementList.Achievement achievement) {
        Name.text = achievement.Name;
        Description.text = achievement.Description;
        Image.sprite = achievement.Image;
    }
}
