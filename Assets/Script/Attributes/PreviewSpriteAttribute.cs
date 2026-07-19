using UnityEngine;

/// <summary>
/// Displays a Sprite field with a small thumbnail preview directly below it.
/// Works on any Sprite field in any MonoBehaviour or ScriptableObject.
///
/// Usage:
///   [PreviewSprite]        public Sprite icon;          // default 64px preview
///   [PreviewSprite(96f)]   public Sprite portrait;      // custom size
/// </summary>
public class PreviewSpriteAttribute : PropertyAttribute
{
    public readonly float previewSize;

    public PreviewSpriteAttribute(float previewSize = 64f)
    {
        this.previewSize = previewSize;
    }
}
