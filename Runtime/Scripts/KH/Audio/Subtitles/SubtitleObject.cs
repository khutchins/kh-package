using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Audio {
    public abstract class SubtitleObject : ScriptableObject {
        public abstract Subtitle AsSubtitle();
    }
}