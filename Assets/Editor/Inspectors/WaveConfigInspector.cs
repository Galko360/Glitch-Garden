using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WaveConfig))]
public class WaveConfigInspector : Editor
{
    // ── Difficulty thresholds ─────────────────────────────────────────────────
    private const int EASY_MAX   = 5;
    private const int MEDIUM_MAX = 15;
    private const int MAX_DOTS   = 15;   // cap dots drawn before showing "+N"

    public override void OnInspectorGUI()
    {
        WaveConfig config = (WaveConfig)target;
        serializedObject.Update();

        // ── Difficulty header ─────────────────────────────────────────────────
        DrawDifficultyHeader(config);
        EditorGUILayout.Space(8);

        // ── Core fields ───────────────────────────────────────────────────────
        EditorGUILayout.PropertyField(serializedObject.FindProperty("enemyCount"));
        EditorGUILayout.PropertyField(serializedObject.FindProperty("spawnInterval"));

        // Round Robin — hidden when only 1 spawner exists in the open scene
        int spawnerCount = CountSpawnersInScene();
        if (spawnerCount > 1)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("roundRobin"));
        }
        else
        {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Toggle("Round Robin", false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.HelpBox(
                "Round Robin is only relevant with 2+ spawners. Add more EnemySpawners to enable it.",
                MessageType.None);
        }

        EditorGUILayout.Space(10);

        // ── Enemy type preview ────────────────────────────────────────────────
        EditorGUILayout.LabelField("Possible Enemies", EditorStyles.boldLabel);
        DrawEnemyPreviews(config);

        EditorGUILayout.PropertyField(serializedObject.FindProperty("possibleEnemies"), true);
        EditorGUILayout.Space(6);

        // ── Validation ────────────────────────────────────────────────────────
        if (config.spawnInterval <= 0f)
            EditorGUILayout.HelpBox(
                "Spawn Interval is 0 or negative — this will freeze Unity in an infinite loop!",
                MessageType.Error);

        if (config.possibleEnemies == null || config.possibleEnemies.Length == 0)
            EditorGUILayout.HelpBox(
                "No enemy types assigned — spawner will fall back to its own default prefab.",
                MessageType.Warning);

        if (config.enemyCount <= 0)
            EditorGUILayout.HelpBox(
                "Enemy count is 0 — this wave will spawn nothing.",
                MessageType.Warning);

        serializedObject.ApplyModifiedProperties();
    }

    // ─────────────────────────────────────────────────────────────────────────

    private void DrawDifficultyHeader(WaveConfig config)
    {
        Color  diffColor = GetDifficultyColor(config.enemyCount);
        string diffLabel = GetDifficultyLabel(config.enemyCount);

        // Tinted background
        Texture2D bg = MakeTex(2, 2, diffColor * new Color(1, 1, 1, 0.15f));

        GUIStyle bgStyle = new GUIStyle(GUI.skin.box);
        bgStyle.normal.background = bg;
        bgStyle.padding = new RectOffset(6, 8, 6, 6);

        EditorGUILayout.BeginHorizontal(bgStyle);

        // Colored dots (capped at MAX_DOTS)
        GUIStyle dotStyle = new GUIStyle(EditorStyles.label)
        {
            fontSize  = 13,
            alignment = TextAnchor.MiddleCenter
        };
        dotStyle.normal.textColor = diffColor;

        int dotsToShow = Mathf.Min(config.enemyCount, MAX_DOTS);
        for (int i = 0; i < dotsToShow; i++)
            GUILayout.Label("●", dotStyle, GUILayout.Width(14), GUILayout.Height(22));

        if (config.enemyCount > MAX_DOTS)
        {
            GUIStyle overflowStyle = new GUIStyle(EditorStyles.miniLabel);
            overflowStyle.normal.textColor = diffColor;
            GUILayout.Label($"+{config.enemyCount - MAX_DOTS}", overflowStyle, GUILayout.Height(22));
        }

        GUILayout.FlexibleSpace();

        // Difficulty label on the right
        GUIStyle labelStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize  = 12,
            alignment = TextAnchor.MiddleRight
        };
        labelStyle.normal.textColor = diffColor;
        GUILayout.Label(diffLabel, labelStyle, GUILayout.Height(22));

        EditorGUILayout.EndHorizontal();
    }

    private void DrawEnemyPreviews(WaveConfig config)
    {
        if (config.possibleEnemies == null || config.possibleEnemies.Length == 0)
        {
            EditorGUILayout.HelpBox("No enemies in list yet.", MessageType.None);
            return;
        }

        // Scroll horizontally if there are many enemies
        EditorGUILayout.BeginHorizontal();

        foreach (EnemyData enemy in config.possibleEnemies)
        {
            if (enemy == null)
            {
                GUILayout.Box("null", GUILayout.Width(54), GUILayout.Height(54));
                continue;
            }

            EditorGUILayout.BeginVertical(GUILayout.Width(56));

            // Portrait
            if (enemy.icon != null)
            {
                Texture2D tex = AssetPreview.GetAssetPreview(enemy.icon);
                if (tex != null)
                {
                    // Clickable — select the asset on click
                    if (GUILayout.Button(tex, GUIStyle.none, GUILayout.Width(50), GUILayout.Height(50)))
                        EditorGUIUtility.PingObject(enemy);
                }
                else
                {
                    GUILayout.Box("...", GUILayout.Width(50), GUILayout.Height(50));
                    Repaint();
                }
            }
            else
            {
                GUILayout.Box("?", GUILayout.Width(50), GUILayout.Height(50));
            }

            // Name below portrait
            GUIStyle nameStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            GUILayout.Label(
                string.IsNullOrWhiteSpace(enemy.id) ? "?" : enemy.id,
                nameStyle,
                GUILayout.Width(50));

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(4);
    }

    // ─────────────────────────────────────────────────────────────────────────

    private static Color GetDifficultyColor(int count)
    {
        if (count <= EASY_MAX)   return new Color(0.20f, 0.90f, 0.20f);  // green
        if (count <= MEDIUM_MAX) return new Color(1.00f, 0.80f, 0.10f);  // yellow
        return                          new Color(1.00f, 0.30f, 0.20f);  // red
    }

    private static string GetDifficultyLabel(int count)
    {
        if (count <= EASY_MAX)   return "● EASY";
        if (count <= MEDIUM_MAX) return "●● MEDIUM";
        return                          "●●● HARD";
    }

    private static int CountSpawnersInScene()
    {
        // FindObjectsByType works in edit mode too
        return Object.FindObjectsByType<EnemySpawner>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None).Length;
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[]   pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++) pix[i] = col;
        Texture2D tex = new Texture2D(width, height);
        tex.SetPixels(pix);
        tex.Apply();
        return tex;
    }
}
