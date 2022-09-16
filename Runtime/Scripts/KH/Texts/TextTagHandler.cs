using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Texts {
    /// <summary>
    /// Allows the handling of custom tags.
    /// </summary>
    public abstract class TextTagHandler : MonoBehaviour {
        public virtual void TextStarted(string textWithMarkup, string textWithoutMarkup) { }
        public virtual void TextProgressed(TextUpdate textUpdate) { }
        public virtual void TextCompleted(string textWithMarkup, string textWithoutMarkup) { }
    }
}