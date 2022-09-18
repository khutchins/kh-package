using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace KH.Texts {
    /// <summary>
    /// Allows the handling of custom tags.
    /// </summary>
    public abstract class TextTagHandler : MonoBehaviour {
        /// <summary>
        /// Called when the text begins.
        /// </summary>
        /// <param name="textWithMarkup">Text with the standard markup (italics, bold, etc.)</param>
        /// <param name="textWithoutMarkup">Text with the markup stripped.</param>
        public virtual void TextStarted(string textWithMarkup, string textWithoutMarkup) { }

        /// <summary>
        /// Called on every text animation update.
        /// </summary>
        /// <param name="textUpdate">The raw information about the text update. Most saliently, it has the unrecognized tags in UnrecognizedTags.</param>
        public virtual void TextProgressed(TextUpdate textUpdate) { }

        /// <summary>
        /// Called when the text is skipped and contains all future updates. Useful if you
        /// are setting state in the text box. If you're doing something relevant to the text,
        /// like shaking or SFX, you probably want to ignore this.
        /// 
        /// NOTE: This will only be called if text is skipped. If you want to execute some
        /// action when the text completes, use TextCompleted. If you want to execute some
        /// action when the text is dismissed, use TextDismissed.
        /// </summary>
        /// <param name="textUpdate">The raw information about the text update. Most saliently, it has the unrecognized tags in UnrecognizedTags.</param>
        public virtual void TextSkipped(TextUpdate[] remainingUpdates) { }

        /// <summary>
        /// Called when the text animation finishes/is skipped.
        /// 
        /// NOTE: This will be called when the text is still up. If you want to do something
        /// when the text box goes away/a new line is started, use TextDismissed.
        /// </summary>
        /// <param name="textWithMarkup">Text with the standard markup (italics, bold, etc.)</param>
        /// <param name="textWithoutMarkup">Text with the markup stripped.</param>
        public virtual void TextCompleted(string textWithMarkup, string textWithoutMarkup) { }

        /// <summary>
        /// Called when the current text is dismissed, either for a new line of dialogue or 
        /// because dialogue has ended.
        /// </summary>
        /// <param name="textWithMarkup">Text with the standard markup (italics, bold, etc.)</param>
        /// <param name="textWithoutMarkup">Text with the markup stripped.</param>
        public virtual void TextDismissed() { }
    }
}