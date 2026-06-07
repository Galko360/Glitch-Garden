using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(PreviewSpriteAttribute))]
public class PreviewSpriteDrawer : PropertyDrawer
{
    private const float SPACING = 3f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.ObjectReference)
            return EditorGUIUtility.singleLineHeight;

        // Only expand height when a sprite is actually assigned
        if (property.objectReferenceValue != null)
        {
            float previewSize = ((PreviewSpriteAttribute)attribute).previewSize;
            return EditorGUIUtility.singleLineHeight + SPACING + previewSize;
        }

        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Guard: only works on Sprite (ObjectReference) fields
        if (property.propertyType != SerializedPropertyType.ObjectReference)
        {
            EditorGUI.HelpBox(position, $"[PreviewSprite] only works on Sprite fields — '{property.name}' is {property.propertyType}", MessageType.Warning);
            return;
        }

        // ── Normal object picker on the first line ────────────────────────────
        Rect fieldRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(fieldRect, property, label);

        // ── Preview below — only when a sprite is assigned ────────────────────
        if (property.objectReferenceValue == null) return;

        Sprite sprite = property.objectReferenceValue as Sprite;
        if (sprite == null) return;

        PreviewSpriteAttribute attr = (PreviewSpriteAttribute)attribute;
        float size = attr.previewSize;

        Rect previewRect = new Rect(
            position.x + EditorGUIUtility.labelWidth + SPACING,
            position.y + EditorGUIUtility.singleLineHeight + SPACING,
            size,
            size);

        // Dark background panel
        EditorGUI.DrawRect(previewRect, new Color(0.13f, 0.13f, 0.13f));
        DrawRectOutline(previewRect, new Color(0.35f, 0.35f, 0.35f));

        // Sprite preview texture
        Texture2D tex = AssetPreview.GetAssetPreview(sprite);
        if (tex != null)
        {
            // Slight inset so the image doesn't overlap the border
            Rect imageRect = new Rect(previewRect.x + 2f, previewRect.y + 2f,
                                      previewRect.width - 4f, previewRect.height - 4f);
            GUI.DrawTexture(imageRect, tex, ScaleMode.ScaleToFit, true);
        }
        else
        {
            // Texture not loaded yet — request a repaint on the next frame
            EditorGUI.LabelField(previewRect, "Loading...", EditorStyles.centeredGreyMiniLabel);
            // Trigger repaint via the editor window
            HandleUtility.Repaint();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static void DrawRectOutline(Rect r, Color color)
    {
        EditorGUI.DrawRect(new Rect(r.x,         r.y,         r.width, 1f),  color);
        EditorGUI.DrawRect(new Rect(r.x,         r.yMax - 1f, r.width, 1f),  color);
        EditorGUI.DrawRect(new Rect(r.x,         r.y,         1f, r.height), color);
        EditorGUI.DrawRect(new Rect(r.xMax - 1f, r.y,         1f, r.height), color);
    }
}
