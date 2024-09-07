using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Achievements {
    [CreateAssetMenu(menuName = "KH/Achievements/List")]
    public class AchievementList : ScriptableObject {
        public Achievement[] Achievements;

        [System.Serializable]
        public class Achievement {
            public string ID;
            public string Name;
            public string Description;
            public Sprite Image;
        }
    }
}