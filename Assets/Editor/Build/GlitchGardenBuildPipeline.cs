using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Custom build pipeline for Glitch Garden.
///
/// PRE-BUILD  (IPreprocessBuildWithReport)
///   1. Scans the scene + project assets for known issues.
///   2. If auto-fixable issues are found → Dialog: "Fix Automatically / Skip".
///   3. If critical errors still remain     → Dialog: "Cancel Build / Build Anyway".
///
/// POST-BUILD (IPostprocessBuildWithReport)
///   • Logs "I NOT am an agent, Thank you very much" (assignment requirement 13).
///   • Logs platform, output path, size, and build duration.
///
/// Covers assignment requirements 11 (pre/post-build), 12 (custom pipeline + dialogs),
/// and 13 (post-build debug log).
/// </summary>
public class GlitchGardenBuildPipeline : IPreprocessBuildWithReport, IPostprocessBuildWithReport
{
    // Lower = runs before other processors that have a higher callbackOrder.
    public int callbackOrder => 0;

    // =========================================================================
    //  PRE-BUILD
    // =========================================================================

    public void OnPreprocessBuild(BuildReport report)
    {
        Debug.Log("[GlitchGarden Build] ── Pre-build validation starting ──");

        List<BuildIssue> issues = CollectIssues();

        if (issues.Count == 0)
        {
            Debug.Log("[GlitchGarden Build] ✓ All checks passed — no issues found.");
            return;
        }

        // Print every issue to the Console so the log is permanent.
        foreach (BuildIssue issue in issues)
        {
            if (issue.Severity == IssueSeverity.Error)
                Debug.LogError("[Build] " + issue.Message);
            else
                Debug.LogWarning("[Build] " + issue.Message);
        }

        int errorCount   = CountBySeverity(issues, IssueSeverity.Error);
        int warningCount = CountBySeverity(issues, IssueSeverity.Warning);
        bool hasAutoFixes = issues.Exists(i => i.AutoFixable);

        // ── Dialog 1 : offer to auto-fix ─────────────────────────────────────
        if (hasAutoFixes)
        {
            string body = BuildDialogText(issues, errorCount, warningCount)
                        + "\n\nSome issues can be fixed automatically. Fix them now?";

            bool fix = EditorUtility.DisplayDialog(
                "Build Validation — Issues Found",
                body,
                "Fix Automatically",   // true
                "Skip");               // false

            if (fix)
            {
                ApplyAutoFixes(issues);
                Debug.Log("[GlitchGarden Build] Auto-fixes applied. Re-scanning...");

                // Re-collect so the second dialog reflects the new state.
                issues      = CollectIssues();
                errorCount  = CountBySeverity(issues, IssueSeverity.Error);
            }
        }
        else
        {
            // No fixes available — just show the summary and let the user dismiss it.
            EditorUtility.DisplayDialog(
                "Build Validation — Issues Found",
                BuildDialogText(issues, errorCount, warningCount),
                "OK");
        }

        // ── Dialog 2 : cancel if critical errors remain ───────────────────────
        if (errorCount > 0)
        {
            bool cancel = EditorUtility.DisplayDialog(
                "Build Validation — Critical Errors Remain",
                $"{errorCount} critical error(s) could not be fixed automatically.\n" +
                $"Building with these errors may cause runtime issues.\n\n" +
                $"Cancel the build, or proceed anyway?",
                "Cancel Build",    // true  → throw to abort
                "Build Anyway");   // false → continue

            if (cancel)
                throw new BuildFailedException(
                    "[GlitchGarden] Build cancelled by user after validation errors.");
        }

        Debug.Log("[GlitchGarden Build] Pre-build validation finished. Proceeding with build...");
    }

    // =========================================================================
    //  POST-BUILD
    // =========================================================================

    public void OnPostprocessBuild(BuildReport report)
    {
        // ── Assignment-required log (requirement 13) ──────────────────────────
        Debug.Log("I NOT am an agent, Thank you very much");

        // ── Human-readable build summary ──────────────────────────────────────
        BuildSummary s = report.summary;

        long   bytes    = (long)s.totalSize;
        string sizeStr  = bytes > 1_000_000
                        ? $"{bytes / 1_000_000f:F1} MB"
                        : $"{bytes / 1_000f:F1} KB";

        double seconds  = (s.buildEndedAt - s.buildStartedAt).TotalSeconds;

        Debug.Log(
            $"[GlitchGarden Build] ✓ Build complete!\n" +
            $"  Platform  : {s.platform}\n"             +
            $"  Output    : {s.outputPath}\n"           +
            $"  Size      : {sizeStr}\n"                +
            $"  Duration  : {seconds:F0}s\n"            +
            $"  Result    : {s.result}"
        );
    }

    // =========================================================================
    //  VALIDATION  —  collect all issues
    // =========================================================================

    private static List<BuildIssue> CollectIssues()
    {
        var issues = new List<BuildIssue>();

        // ── WaveController ────────────────────────────────────────────────────
        WaveController waveCtrl = UnityEngine.Object.FindFirstObjectByType<WaveController>();
        if (waveCtrl == null)
        {
            issues.Add(BuildIssue.Error("[WaveController] Not found in scene — waves will not run."));
        }
        else
        {
            SerializedObject so = new SerializedObject(waveCtrl);

            if (so.FindProperty("waves")?.arraySize == 0)
                issues.Add(BuildIssue.Warning("[WaveController] No WaveConfigs assigned — only procedural waves will run."));

            if (so.FindProperty("spawners")?.arraySize == 0)
                issues.Add(BuildIssue.Error("[WaveController] Spawners list is empty — enemies won't spawn."));
        }

        // ── MergeManager ──────────────────────────────────────────────────────
        MergeManager mergeMgr = UnityEngine.Object.FindFirstObjectByType<MergeManager>();
        if (mergeMgr == null)
        {
            issues.Add(BuildIssue.Error("[MergeManager] Not found in scene."));
        }
        else
        {
            SerializedObject so = new SerializedObject(mergeMgr);

            if (so.FindProperty("mergeDatabase")?.objectReferenceValue == null)
                issues.Add(BuildIssue.Error("[MergeManager] No MergeDatabase assigned — merging will not work."));

            if (so.FindProperty("tileLayer")?.intValue == 0)
                issues.Add(BuildIssue.Error("[MergeManager] Tile layer mask is not set — placement clicks won't register."));
        }

        // ── EnemySpawners ─────────────────────────────────────────────────────
        EnemySpawner[] spawners = UnityEngine.Object.FindObjectsByType<EnemySpawner>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (spawners.Length == 0)
        {
            issues.Add(BuildIssue.Error("[EnemySpawner] No EnemySpawners in scene."));
        }
        else
        {
            foreach (EnemySpawner sp in spawners)
            {
                SerializedObject so = new SerializedObject(sp);
                if (so.FindProperty("defaultPrefab")?.objectReferenceValue == null)
                    issues.Add(BuildIssue.Error($"[EnemySpawner] '{sp.name}' has no default prefab."));
            }
        }

        // ── DragManager ───────────────────────────────────────────────────────
        DragManager dragMgr = UnityEngine.Object.FindFirstObjectByType<DragManager>();
        if (dragMgr == null)
        {
            issues.Add(BuildIssue.Error("[DragManager] Not found in scene."));
        }
        else
        {
            SerializedObject so = new SerializedObject(dragMgr);

            if (so.FindProperty("canvas")?.objectReferenceValue == null)
                issues.Add(BuildIssue.Error("[DragManager] Canvas reference is missing."));
            if (so.FindProperty("inventory")?.objectReferenceValue == null)
                issues.Add(BuildIssue.Error("[DragManager] InventoryManager reference is missing."));
            if (so.FindProperty("merge")?.objectReferenceValue == null)
                issues.Add(BuildIssue.Error("[DragManager] MergeManager reference is missing."));
        }

        // ── GoldManager ───────────────────────────────────────────────────────
        if (UnityEngine.Object.FindFirstObjectByType<GoldManager>() == null)
            issues.Add(BuildIssue.Error("[GoldManager] Not found in scene — gold rewards will not work."));

        // ── TileCells ─────────────────────────────────────────────────────────
        TileCell[] tiles = UnityEngine.Object.FindObjectsByType<TileCell>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        if (tiles.Length == 0)
        {
            issues.Add(BuildIssue.Error("[TileCell] No TileCells in scene — the board is missing."));
        }
        else
        {
            // Count tiles that are still in the Occupied state from edit-mode testing.
            // These aren't dangerous but are almost certainly leftover test state.
            TileCell[] occupiedTiles = Array.FindAll(tiles, t => t.IsOccupied);
            if (occupiedTiles.Length > 0)
            {
                TileCell[] captured = occupiedTiles; // lambda capture
                issues.Add(BuildIssue.Fixable(
                    $"[TileCell] {captured.Length} tile(s) are still marked Occupied from edit-mode — clearing them is recommended.",
                    () =>
                    {
                        foreach (TileCell t in captured)
                            t.Clear();
                        Debug.Log($"[GlitchGarden Build] Auto-fixed: cleared {captured.Length} stale tile(s).");
                    }));
            }
        }

        // ── WaveConfig assets (project-wide) ──────────────────────────────────
        foreach (string guid in AssetDatabase.FindAssets("t:WaveConfig"))
        {
            WaveConfig config = AssetDatabase.LoadAssetAtPath<WaveConfig>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (config == null) continue;

            if (config.spawnInterval <= 0f)
            {
                WaveConfig captured = config; // lambda capture
                issues.Add(BuildIssue.Fixable(
                    $"[WaveConfig] '{config.name}' has spawnInterval ≤ 0 — this will freeze the game!",
                    () =>
                    {
                        captured.spawnInterval = 0.5f;
                        EditorUtility.SetDirty(captured);
                        Debug.Log($"[GlitchGarden Build] Auto-fixed: set '{captured.name}' spawnInterval → 0.5.");
                    },
                    IssueSeverity.Error));
            }

            if (config.enemyCount <= 0)
                issues.Add(BuildIssue.Warning($"[WaveConfig] '{config.name}' has enemyCount = 0 — wave spawns nothing."));
        }

        // ── EnemyData assets (project-wide) ───────────────────────────────────
        foreach (string guid in AssetDatabase.FindAssets("t:EnemyData"))
        {
            EnemyData enemyData = AssetDatabase.LoadAssetAtPath<EnemyData>(
                AssetDatabase.GUIDToAssetPath(guid));
            if (enemyData == null) continue;

            if (enemyData.prefab == null)
                issues.Add(BuildIssue.Error($"[EnemyData] '{enemyData.name}' has no prefab assigned."));
            if (enemyData.hp <= 0)
                issues.Add(BuildIssue.Error($"[EnemyData] '{enemyData.name}' has HP ≤ 0 — will die instantly."));
            if (string.IsNullOrWhiteSpace(enemyData.id))
                issues.Add(BuildIssue.Warning($"[EnemyData] '{enemyData.name}' has an empty ID."));
        }

        return issues;
    }

    // =========================================================================
    //  AUTO-FIX
    // =========================================================================

    private static void ApplyAutoFixes(List<BuildIssue> issues)
    {
        foreach (BuildIssue issue in issues)
        {
            if (!issue.AutoFixable) continue;
            issue.Fix?.Invoke();
        }
        AssetDatabase.SaveAssets();
    }

    // =========================================================================
    //  HELPERS
    // =========================================================================

    private static int CountBySeverity(List<BuildIssue> issues, IssueSeverity sev)
    {
        int count = 0;
        foreach (BuildIssue i in issues)
            if (i.Severity == sev) count++;
        return count;
    }

    private static string BuildDialogText(List<BuildIssue> issues, int errorCount, int warningCount)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Found {errorCount} error(s) and {warningCount} warning(s):\n");

        foreach (BuildIssue issue in issues)
        {
            string icon = issue.Severity == IssueSeverity.Error ? "[ERROR]" : "[WARN] ";
            string fix  = issue.AutoFixable ? " (auto-fixable)" : string.Empty;
            sb.AppendLine($"{icon} {issue.Message}{fix}");
        }

        return sb.ToString().TrimEnd();
    }

    // =========================================================================
    //  DATA TYPES
    // =========================================================================

    private enum IssueSeverity { Warning, Error }

    /// <summary>
    /// Represents a single validation finding.
    /// AutoFixable issues carry a <see cref="Fix"/> delegate that repairs the problem.
    /// </summary>
    private class BuildIssue
    {
        public IssueSeverity Severity  { get; private set; }
        public string        Message   { get; private set; }
        public bool          AutoFixable => Fix != null;
        public Action        Fix       { get; private set; }

        // ── factories ─────────────────────────────────────────────────────────

        public static BuildIssue Error(string message) =>
            new BuildIssue { Severity = IssueSeverity.Error, Message = message };

        public static BuildIssue Warning(string message) =>
            new BuildIssue { Severity = IssueSeverity.Warning, Message = message };

        /// <summary>Auto-fixable warning (default severity = Warning).</summary>
        public static BuildIssue Fixable(string message, Action fix,
            IssueSeverity severity = IssueSeverity.Warning) =>
            new BuildIssue { Severity = severity, Message = message, Fix = fix };
    }
}
