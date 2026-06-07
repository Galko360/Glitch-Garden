using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataInspector : Editor
{
    // ── EditorPrefs keys (shared across all EnemyData assets) ────────────────
    private const string KEY_MAX_HP     = "GG_EnemyData_MaxHP";
    private const string KEY_MAX_DAMAGE = "GG_EnemyData_MaxDamage";

    // ── Foldout state ─────────────────────────────────────────────────────────
    private bool rewardFoldout   = false;
    private bool settingsFoldout = false;

    // ── Cached background texture ─────────────────────────────────────────────
    private Texture2D headerBg;

    // ─────────────────────────────────────────────────────────────────────────

    public override void OnInspectorGUI()
    {
        EnemyData data = (EnemyData)target;
        serializedObject.Update();

        // Load display maxes from EditorPrefs
        int maxHp     = EditorPrefs.GetInt(KEY_MAX_HP,     50);
        int maxDamage = EditorPrefs.GetInt(KEY_MAX_DAMAGE, 20);

        // ── Header ────────────────────────────────────────────────────────────
        DrawHeader(data);
        EditorGUILayout.Space(6);

        // ── Identity fields ───────────────────────────────────────────────────
        EditorGUILayout.PropertyField(serializedObject.FindProperty("id"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("prefab"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("icon"));
        EditorGUILayout.Space(8);

        // ── Stat bars ─────────────────────────────────────────────────────────
        DrawStatBar("HP",     data.hp,           maxHp,     new Color(0.25f, 0.85f, 0.25f));
        DrawStatBar("Damage", data.attackDamage,  maxDamage, new Color(1.00f, 0.30f, 0.20f));

        EditorGUILayout.PropertyField(serializedObject.FindProperty("attackCooldown"));
        EditorGUILayout.Space(6);

        // ── Info boxes ────────────────────────────────────────────────────────
        if (data.attackCooldown > 0f)
        {
            float dps = data.attackDamage / data.attackCooldown;
            EditorGUILayout.HelpBox($"DPS: {dps:F1} damage / sec", MessageType.Info);
        }

        EditorGUILayout.HelpBox(
            "These values override the Enemy prefab's inline stats at runtime.",
            MessageType.Info);

        EditorGUILayout.Space(6);

        // ── Reward foldout ────────────────────────────────────────────────────
        rewardFoldout = EditorGUILayout.Foldout(rewardFoldout, "Reward", true, EditorStyles.foldoutHeader);
        if (rewardFoldout)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(serializedObject.FindProperty("goldReward"));
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(4);

        // ── Display Settings foldout ──────────────────────────────────────────
        settingsFoldout = EditorGUILayout.Foldout(settingsFoldout, "Display Settings", true, EditorStyles.foldoutHeader);
        if (settingsFoldout)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.LabelField("Stat bar maximums (display only — does not affect gameplay)",
                EditorStyles.miniLabel);
            EditorGUILayout.Space(2);

            int newMaxHp     = EditorGUILayout.IntField("Max HP",     maxHp);
            int newMaxDamage = EditorGUILayout.IntField("Max Damage", maxDamage);

            // Persist changes immediately
            if (newMaxHp != maxHp)
                EditorPrefs.SetInt(KEY_MAX_HP, Mathf.Max(1, newMaxHp));
            if (newMaxDamage != maxDamage)
                EditorPrefs.SetInt(KEY_MAX_DAMAGE, Mathf.Max(1, newMaxDamage));

            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(6);

        // ── Validation ────────────────────────────────────────────────────────
        if (data.prefab == null)
            EditorGUILayout.HelpBox("Prefab is not assigned — this enemy cannot spawn.", MessageType.Error);

        if (data.hp <= 0)
            EditorGUILayout.HelpBox("HP is 0 — enemy will die instantly when spawned.", MessageType.Warning);

        if (string.IsNullOrWhiteSpace(data.id))
            EditorGUILayout.HelpBox("ID is empty — merge lookups and debug logs rely on this.", MessageType.Warning);

        serializedObject.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private void DrawHeader(EnemyData data)
    {
        if (headerBg == null)
            headerBg = MakeTex(2, 2, new Color(0.13f, 0.13f, 0.13f, 1f));

        GUIStyle bgStyle = new GUIStyle(GUI.skin.box)
        {
            padding  = new RectOffset(6, 6, 6, 6),
            margin   = new RectOffset(0, 0, 0, 0)
        };
        bgStyle.normal.background = headerBg;

        EditorGUILayout.BeginHorizontal(bgStyle, GUILayout.Height(68));

        // Portrait
        if (data.icon != null)
        {
            Texture2D tex = AssetPreview.GetAssetPreview(data.icon);
            if (tex != null)
                GUILayout.Label(tex, GUILayout.Width(60), GUILayout.Height(60));
            else
            {
                // Preview not ready yet — ask for a repaint
                GUILayout.Box("...", GUILayout.Width(60), GUILayout.Height(60));
                Repaint();
            }
        }
        else
        {
            GUIStyle emptyStyle = new GUIStyle(GUI.skin.box);
            GUILayout.Box("No Icon", emptyStyle, GUILayout.Width(60), GUILayout.Height(60));
        }

        GUILayout.Space(8);

        // Name
        GUIStyle nameStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 16,
            alignment = TextAnchor.MiddleLeft
        };
        nameStyle.normal.textColor = Color.white;

        EditorGUILayout.LabelField(
            string.IsNullOrWhiteSpace(data.id) ? "Unnamed Enemy" : data.id,
            nameStyle,
            GUILayout.Height(60));

        EditorGUILayout.EndHorizontal();
    }

    /// <summary>Draws a labeled colored fill bar with a value / max readout.</summary>
    private void DrawStatBar(string label, float value, float max, Color barColor)
    {
        EditorGUILayout.BeginHorizontal();

        // Label column
        EditorGUILayout.LabelField(label, GUILayout.Width(58));

        // Bar rect (takes up the remaining space minus the value label)
        Rect barRect = GUILayoutUtility.GetRect(0f, 16f, GUILayout.ExpandWidth(true));
        barRect = new Rect(barRect.x, barRect.y + 1f, barRect.width, 14f);

        // Background
        EditorGUI.DrawRect(barRect, new Color(0.18f, 0.18f, 0.18f));

        // Fill
        float fill     = max > 0f ? Mathf.Clamp01(value / max) : 0f;
        Rect  fillRect = new Rect(barRect.x, barRect.y, barRect.width * fill, barRect.height);
        EditorGUI.DrawRect(fillRect, barColor);

        // Thin border
        DrawRectOutline(barRect, new Color(0.4f, 0.4f, 0.4f));

        // Value text
        EditorGUILayout.LabelField($"{value} / {max}", GUILayout.Width(72));

        EditorGUILayout.EndHorizontal();
    }

    private static void DrawRectOutline(Rect r, Color color)
    {
        EditorGUI.DrawRect(new Rect(r.x,             r.y,              r.width, 1),           color);
        EditorGUI.DrawRect(new Rect(r.x,             r.y + r.height - 1, r.width, 1),         color);
        EditorGUI.DrawRect(new Rect(r.x,             r.y,              1, r.height),           color);
        EditorGUI.DrawRect(new Rect(r.x + r.width - 1, r.y,           1, r.height),           color);
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[]   pix   = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
