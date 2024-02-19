using System.Collections;
using System.Collections.Generic;
using KH.Texts;
using UnityEngine;

public class TextTagHandlerDiagnostic : TextTagHandler {
    public override void TextStarted(string textWithMarkup, string textWithoutMarkup) {
        Debug.Log("Starting");
        Debug.Log($"Text w/ markup: {textWithMarkup}");
        Debug.Log($"Text w/o markup: {textWithoutMarkup}");
    }

    public override void TextProgressed(TextUpdate textUpdate) {
        foreach (TextToken token in textUpdate.UnrecognizedTags) {
            Debug.Log($"Token: {token}");
        }
    }

    public override void TextSkipped(TextUpdate[] remainingUpdates) {
        Debug.Log($"Skipped {remainingUpdates.Length} updates");
        foreach (TextUpdate textUpdate in remainingUpdates) {
            foreach (TextToken token in textUpdate.UnrecognizedTags) {
                Debug.Log($"Token: {token}");
            }
        }
        base.TextSkipped(remainingUpdates);
    }

    public override void TextCompleted(string textWithMarkup, string textWithoutMarkup) {
        Debug.Log("Completed");
        Debug.Log($"Text w/ markup: {textWithMarkup}");
        Debug.Log($"Text w/o markup: {textWithoutMarkup}");
    }

    public override void TextDismissed() {
        Debug.Log("Dismissed");
    }
}
