#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TDMergeWorkbenchWindow : EditorWindow
{
    private const string MergeDatabaseGuidKey = "TDMerge_Workbench_MergeDatabaseGUID";

    [SerializeField] private MergeDatabase mergeDatabase;

    private Vector2 scroll;
    private string searchText = string.Empty;

    [MenuItem("TD Merge/Merge Workbench %#m")]
    public static void Open()
    {
        GetWindow<TDMergeWorkbenchWindow>("Merge Workbench");
    }

    private void OnEnable()
    {
        LoadSavedMergeDatabase();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("TD Merge Workbench", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Central editor hub for merge recipes and validation.", MessageType.Info);

        EditorGUI.BeginChangeCheck();
        mergeDatabase = (MergeDatabase)EditorGUILayout.ObjectField(
            "Merge Database",
            mergeDatabase,
            typeof(MergeDatabase),
            false);

        if (EditorGUI.EndChangeCheck())
        {
            SaveMergeDatabase();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Validate Merge DB", GUILayout.Height(24)))
                ValidateMergeDatabase();

            if (GUILayout.Button("Clear Saved DB", GUILayout.Height(24)))
                ClearSavedMergeDatabase();
        }

        EditorGUILayout.Space(6);
        searchText = EditorGUILayout.TextField("Search", searchText);
        EditorGUILayout.Space(8);

        if (mergeDatabase == null)
        {
            EditorGUILayout.HelpBox("Assign a MergeDatabase to inspect recipes.", MessageType.Warning);
            return;
        }

        DrawRecipeSummary();
    }

    private void DrawRecipeSummary()
    {
        IReadOnlyList<MergeRecipe> recipes = mergeDatabase.Recipes;

        if (recipes == null || recipes.Count == 0)
        {
            EditorGUILayout.HelpBox("No merge recipes exist in this database.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("Recipes: " + recipes.Count, EditorStyles.miniBoldLabel);

        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < recipes.Count; i++)
        {
            MergeRecipe recipe = recipes[i];
            if (recipe == null)
                continue;

            string inputA = GetUnitLabel(recipe.inputA);
            string inputB = GetUnitLabel(recipe.inputB);
            string output = GetUnitLabel(recipe.output);

            string displayLine = inputA + " + " + inputB + " -> " + output;

            if (!string.IsNullOrWhiteSpace(searchText) &&
                !displayLine.ToLowerInvariant().Contains(searchText.ToLowerInvariant()))
            {
                continue;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField(displayLine, EditorStyles.boldLabel);
            EditorGUILayout.ObjectField("Recipe Asset", recipe, typeof(MergeRecipe), false);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ping Recipe"))
                    EditorGUIUtility.PingObject(recipe);

                if (recipe.output != null && GUILayout.Button("Ping Output"))
                    EditorGUIUtility.PingObject(recipe.output);
            }

            if (recipe.inputA == null || recipe.inputB == null || recipe.output == null)
                EditorGUILayout.HelpBox("This recipe has a missing input or output.", MessageType.Error);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ValidateMergeDatabase()
    {
        if (mergeDatabase == null)
        {
            EditorUtility.DisplayDialog("Validation", "Assign a MergeDatabase first.", "OK");
            return;
        }

        IReadOnlyList<MergeRecipe> recipes = mergeDatabase.Recipes;
        if (recipes == null || recipes.Count == 0)
        {
            EditorUtility.DisplayDialog("Validation", "The database has no recipes.", "OK");
            return;
        }

        int missingCount = 0;
        int duplicateCount = 0;
        HashSet<string> seenPairs = new HashSet<string>();

        for (int i = 0; i < recipes.Count; i++)
        {
            MergeRecipe recipe = recipes[i];

            if (recipe == null || recipe.inputA == null || recipe.inputB == null || recipe.output == null)
            {
                missingCount++;
                continue;
            }

            string key = MakePairKey(recipe.inputA, recipe.inputB);
            if (!seenPairs.Add(key))
                duplicateCount++;
        }

        string message =
            "Validation complete.\n\n" +
            "Missing / incomplete recipes: " + missingCount + "\n" +
            "Duplicate pairs: " + duplicateCount;

        EditorUtility.DisplayDialog("Validation Result", message, "OK");
    }

    private void SaveMergeDatabase()
    {
        if (mergeDatabase == null)
        {
            EditorPrefs.DeleteKey(MergeDatabaseGuidKey);
            return;
        }

        string path = AssetDatabase.GetAssetPath(mergeDatabase);
        if (string.IsNullOrEmpty(path))
            return;

        string guid = AssetDatabase.AssetPathToGUID(path);
        if (!string.IsNullOrEmpty(guid))
            EditorPrefs.SetString(MergeDatabaseGuidKey, guid);
    }

    private void LoadSavedMergeDatabase()
    {
        if (!EditorPrefs.HasKey(MergeDatabaseGuidKey))
            return;

        string guid = EditorPrefs.GetString(MergeDatabaseGuidKey, string.Empty);
        if (string.IsNullOrEmpty(guid))
            return;

        string path = AssetDatabase.GUIDToAssetPath(guid);
        if (string.IsNullOrEmpty(path))
            return;

        mergeDatabase = AssetDatabase.LoadAssetAtPath<MergeDatabase>(path);
    }

    private void ClearSavedMergeDatabase()
    {
        mergeDatabase = null;
        EditorPrefs.DeleteKey(MergeDatabaseGuidKey);
    }

    private static string GetUnitLabel(UnitData unit)
    {
        if (unit == null)
            return "<missing>";

        return string.IsNullOrWhiteSpace(unit.id) ? unit.name : unit.id;
    }

    private static string MakePairKey(UnitData a, UnitData b)
    {
        string left = a != null ? a.GetInstanceID().ToString() : "null";
        string right = b != null ? b.GetInstanceID().ToString() : "null";

        return string.CompareOrdinal(left, right) <= 0
            ? left + "|" + right
            : right + "|" + left;
    }
}
#endif