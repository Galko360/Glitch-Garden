using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Project editor shortcuts.
///   Ctrl+Alt+G  —  Validate Scene
///   Ctrl+Alt+T  —  Clear All Tiles
///   Ctrl+Alt+Q  —  Select All WaveConfigs
/// </summary>
public static class ProjectEditorMenu
{
    #region << Validate Scene >>
    // ═════════════════════════════════════════════════════════════════════════
    // 1. Validate Scene   Ctrl+Alt+G
    // ═════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/Glitch Garden/Validate Scene %&g")]
    public static void ValidateScene()
    {
        List<string> issues = new List<string>();

        // ── WaveController ────────────────────────────────────────────────────
        WaveController waveController = Object.FindFirstObjectByType<WaveController>();
        if (waveController == null)
        {
            issues.Add("[WaveController] Not found in scene.");
        }
        else
        {
            SerializedObject wcSO = new SerializedObject(waveController);

            SerializedProperty wavesList = wcSO.FindProperty("waves");
            if (wavesList != null && wavesList.arraySize == 0)
                issues.Add("[WaveController] No WaveConfigs assigned in the Waves list.");

            SerializedProperty spawnersList = wcSO.FindProperty("spawners");
            if (spawnersList != null && spawnersList.arraySize == 0)
                issues.Add("[WaveController] Spawners list is empty — enemies won't spawn.");
        }

        // ── MergeManager ──────────────────────────────────────────────────────
        MergeManager mergeManager = Object.FindFirstObjectByType<MergeManager>();
        if (mergeManager == null)
        {
            issues.Add("[MergeManager] Not found in scene.");
        }
        else
        {
            SerializedObject mmSO = new SerializedObject(mergeManager);

            SerializedProperty mergeDb = mmSO.FindProperty("mergeDatabase");
            if (mergeDb != null && mergeDb.objectReferenceValue == null)
                issues.Add("[MergeManager] No MergeDatabase assigned — merging will not work.");

            SerializedProperty tileLayer = mmSO.FindProperty("tileLayer");
            if (tileLayer != null && tileLayer.intValue == 0)
                issues.Add("[MergeManager] Tile Layer mask is empty — placement clicks won't hit tiles.");
        }

        // ── EnemySpawners ─────────────────────────────────────────────────────
        EnemySpawner[] spawners = Object.FindObjectsByType<EnemySpawner>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (spawners.Length == 0)
        {
            issues.Add("[EnemySpawner] No EnemySpawners found in scene.");
        }
        else
        {
            foreach (EnemySpawner spawner in spawners)
            {
                SerializedObject spSO = new SerializedObject(spawner);
                SerializedProperty defaultPrefab = spSO.FindProperty("defaultPrefab");
                if (defaultPrefab != null && defaultPrefab.objectReferenceValue == null)
                    issues.Add($"[EnemySpawner] '{spawner.name}' has no default prefab assigned.");
            }
        }

        // ── DragManager ───────────────────────────────────────────────────────
        DragManager dragManager = Object.FindFirstObjectByType<DragManager>();
        if (dragManager == null)
        {
            issues.Add("[DragManager] Not found in scene.");
        }
        else
        {
            SerializedObject dmSO = new SerializedObject(dragManager);

            SerializedProperty canvas    = dmSO.FindProperty("canvas");
            SerializedProperty inventory = dmSO.FindProperty("inventory");
            SerializedProperty merge     = dmSO.FindProperty("merge");

            if (canvas    != null && canvas.objectReferenceValue    == null)
                issues.Add("[DragManager] Canvas not assigned.");
            if (inventory != null && inventory.objectReferenceValue == null)
                issues.Add("[DragManager] InventoryManager not assigned.");
            if (merge     != null && merge.objectReferenceValue     == null)
                issues.Add("[DragManager] MergeManager not assigned.");
        }

        // ── GoldManager ───────────────────────────────────────────────────────
        GoldManager goldManager = Object.FindFirstObjectByType<GoldManager>();
        if (goldManager == null)
            issues.Add("[GoldManager] Not found in scene — gold rewards won't work.");

        // ── TileCells — stale occupied state ──────────────────────────────────
        TileCell[] tiles = Object.FindObjectsByType<TileCell>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (tiles.Length == 0)
        {
            issues.Add("[TileCell] No TileCells found in scene — board is missing.");
        }

        // ── WaveConfig assets — project-wide scan ─────────────────────────────
        string[] waveGuids = AssetDatabase.FindAssets("t:WaveConfig");
        foreach (string guid in waveGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            WaveConfig config = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
            if (config == null) continue;

            if (config.spawnInterval <= 0f)
                issues.Add($"[WaveConfig] '{config.name}' has spawnInterval ≤ 0 — will freeze Unity!");
            if (config.enemyCount <= 0)
                issues.Add($"[WaveConfig] '{config.name}' has enemyCount = 0 — wave spawns nothing.");
        }

        // ── EnemyData assets — project-wide scan ──────────────────────────────
        string[] enemyGuids = AssetDatabase.FindAssets("t:EnemyData");
        foreach (string guid in enemyGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(path);
            if (enemyData == null) continue;

            if (enemyData.prefab == null)
                issues.Add($"[EnemyData] '{enemyData.name}' has no prefab assigned.");
            if (string.IsNullOrWhiteSpace(enemyData.id))
                issues.Add($"[EnemyData] '{enemyData.name}' has an empty ID.");
            if (enemyData.hp <= 0)
                issues.Add($"[EnemyData] '{enemyData.name}' has HP ≤ 0 — will die instantly.");
        }

        // ── Result dialog ─────────────────────────────────────────────────────
        if (issues.Count == 0)
        {
            EditorUtility.DisplayDialog(
                "Validate Scene",
                "✓ Everything looks good! No issues found.",
                "Great!");
        }
        else
        {
            string report = $"{issues.Count} issue(s) found:\n\n" + string.Join("\n", issues);

            bool logToConsole = EditorUtility.DisplayDialog(
                "Validate Scene — Issues Found",
                report,
                "Log to Console",
                "Close");

            if (logToConsole)
            {
                Debug.LogWarning("[Glitch Garden] Scene validation found issues:");
                foreach (string issue in issues)
                    Debug.LogWarning(issue);
            }
        }
    }

    #endregion

    #region << Clear All Tiles >>
    // ═════════════════════════════════════════════════════════════════════════
    // 2. Clear All Tiles   Ctrl+Alt+C
    // ═════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/Glitch Garden/Clear All Tiles %&c")]
    public static void ClearAllTiles()
    {
        // Count occupied tiles first so the dialog is informative
        TileCell[] allTiles = Object.FindObjectsByType<TileCell>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int occupiedCount = 0;
        foreach (TileCell t in allTiles)
            if (t.IsOccupied) occupiedCount++;

        if (occupiedCount == 0)
        {
            EditorUtility.DisplayDialog(
                "Clear All Tiles",
                "No units are placed on the board — nothing to clear.",
                "OK");
            return;
        }

        // Dialog with a CHOICE — confirm or cancel
        bool confirmed = EditorUtility.DisplayDialog(
            "Clear All Tiles",
            $"This will remove all {occupiedCount} placed unit(s) from the board.\n\nThis cannot be undone.",
            "Yes, Clear",   // returns true
            "Cancel");      // returns false

        if (!confirmed) return;

        // Destroy all UnitCombat GameObjects on the board
        UnitCombat[] units = Object.FindObjectsByType<UnitCombat>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (UnitCombat unit in units)
            Object.DestroyImmediate(unit.gameObject);

        // Reset every tile regardless
        foreach (TileCell tile in allTiles)
            tile.Clear();

        Debug.Log($"[Glitch Garden] Cleared {occupiedCount} tile(s).");
    }
    #endregion

    #region << Select All WaveConfigs >>
    // ═════════════════════════════════════════════════════════════════════════
    // 3. Select All WaveConfigs   Ctrl+Alt+W
    // ═════════════════════════════════════════════════════════════════════════

    [MenuItem("Tools/Glitch Garden/Select All WaveConfigs %&w")]
    public static void SelectAllWaveConfigs()
    {
        // Asset search — finds every WaveConfig asset in the whole project
        string[] guids = AssetDatabase.FindAssets("t:WaveConfig");

        if (guids.Length == 0)
        {
            EditorUtility.DisplayDialog(
                "Select All WaveConfigs",
                "No WaveConfig assets found in the project.",
                "OK");
            return;
        }

        // Load all found assets
        Object[] configs = new Object[guids.Length];
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            configs[i] = AssetDatabase.LoadAssetAtPath<WaveConfig>(path);
        }

        // Select them all in the Project window
        Selection.objects = configs;

        // Also ping the first one so the Project window scrolls to it
        EditorGUIUtility.PingObject(configs[0]);

        Debug.Log($"[Glitch Garden] Selected {configs.Length} WaveConfig asset(s).");
    }
    #endregion
}