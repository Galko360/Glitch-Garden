using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(StatBarAttribute))]
public class StatBarDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Same height as a normal single-line field
        return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Guard: only works on numeric types
        if (property.propertyType != SerializedPropertyType.Integer &&
            property.propertyType != SerializedPropertyType.Float)
        {
            EditorGUI.HelpBox(position, $"[StatBar] only works on int or float — '{property.name}' is {property.propertyType}", MessageType.Warning);
            return;
        }

        StatBarAttribute attr = (StatBarAttribute)attribute;

        float value = property.propertyType == SerializedPropertyType.Integer
            ? property.intValue
            : property.floatValue;

        // ── Layout ────────────────────────────────────────────────────────────
        //  [ Label ] [ ████████░░░░░░░ ] [ value field ]

        float fieldWidth = 52f;
        float spacing    = 4f;

        Rect labelRect = new Rect(
            position.x,
            position.y,
            EditorGUIUtility.labelWidth,
            position.height);

        Rect barRect = new Rect(
            position.x + EditorGUIUtility.labelWidth + spacing,
            position.y + 2f,
            position.width - EditorGUIUtility.labelWidth - fieldWidth - spacing * 2f,
            position.height - 4f);

        Rect fieldRect = new Rect(
            position.xMax - fieldWidth,
            position.y,
            fieldWidth,
            position.height);

        // ── Label ─────────────────────────────────────────────────────────────
        EditorGUI.LabelField(labelRect, label);

        // ── Bar background ────────────────────────────────────────────────────
        EditorGUI.DrawRect(barRect, new Color(0.18f, 0.18f, 0.18f));

        // ── Bar fill — color shifts red → green based on how full it is ───────
        float fill     = attr.max > 0f ? Mathf.Clamp01(value / attr.max) : 0f;
        Color barColor = Color.Lerp(
            new Color(0.85f, 0.20f, 0.20f),   // red   (empty)
            new Color(0.20f, 0.85f, 0.20f),   // green (full)
            fill);

        if (fill > 0f)
        {
            Rect fillRect = new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height);
            EditorGUI.DrawRect(fillRect, barColor);
        }

        // ── Border ────────────────────────────────────────────────────────────
        DrawRectOutline(barRect, new Color(0.35f, 0.35f, 0.35f));

        // ── Editable value field ──────────────────────────────────────────────
        EditorGUI.BeginChangeCheck();

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            int newVal = EditorGUI.IntField(fieldRect, property.intValue);
            if (EditorGUI.EndChangeCheck())
                property.intValue = newVal;
        }
        else
        {
            float newVal = EditorGUI.FloatField(fieldRect, property.floatValue);
            if (EditorGUI.EndChangeCheck())
                property.floatValue = newVal;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static void DrawRectOutline(Rect r, Color color)
    {
        EditorGUI.DrawRect(new Rect(r.x,          r.y,           r.width, 1f),       color);
        EditorGUI.DrawRect(new Rect(r.x,          r.yMax - 1f,   r.width, 1f),       color);
        EditorGUI.DrawRect(new Rect(r.x,          r.y,           1f, r.height),      color);
        EditorGUI.DrawRect(new Rect(r.xMax - 1f,  r.y,           1f, r.height),      color);
    }
}
