using UnityEngine;

public class TextAssetPreviewAttribute : PropertyAttribute {
    public readonly float height;

    public TextAssetPreviewAttribute(float height = 180f) {
        this.height = height;
    }
}