#if UNITY_EDITOR
using KH.Script;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TextAssetPreviewAttribute))]
public class TextAssetPreviewDrawer : PropertyDrawer {
    const float k_VerticalSpacing = 4f;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var attr = (TextAssetPreviewAttribute)attribute;

        var objectFieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(objectFieldRect, property, label);

        TextAsset ta = property.objectReferenceValue as TextAsset;
        string text = null;
        if (ta != null) text = ta.text;
        if (text == null) {
            var kvbs = property.objectReferenceValue as KVBSAsset;
            if (kvbs != null) text = kvbs.Text;
        }
        if (text != null) {
            var previewRect = new Rect(
                position.x,
                objectFieldRect.yMax + k_VerticalSpacing,
                position.width,
                attr.height
            );

            using (new EditorGUI.DisabledScope(true)) {
                // Boxed label
                EditorGUI.LabelField(
                    new Rect(previewRect.x, previewRect.y, previewRect.width, EditorGUIUtility.singleLineHeight),
                    "Preview",
                    EditorStyles.boldLabel
                );

                var textAreaRect = new Rect(
                    previewRect.x,
                    previewRect.y + EditorGUIUtility.singleLineHeight + 2f,
                    previewRect.width,
                    previewRect.height - (EditorGUIUtility.singleLineHeight + 2f)
                );

                // Use TextArea-like style that wraps and scrolls naturally
                var style = new GUIStyle(EditorStyles.textArea) { wordWrap = true };
                EditorGUI.TextArea(textAreaRect, text, style);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        var attr = (TextAssetPreviewAttribute)attribute;
        var baseHeight = EditorGUIUtility.singleLineHeight;

        var hasAsset = property.objectReferenceValue != null;
        return hasAsset ? baseHeight + k_VerticalSpacing + attr.height : baseHeight;
    }
}
#endif
