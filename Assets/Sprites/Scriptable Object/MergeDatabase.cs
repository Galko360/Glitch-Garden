using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "TD Merge/Merge Database", fileName = "MergeDatabase_")]
public class MergeDatabase : ScriptableObject
{
    [SerializeField] private List<MergeRecipe> recipes = new List<MergeRecipe>();

    private Dictionary<ulong, UnitData> mergeLookup;
    private Dictionary<GameObject, UnitData> prefabLookup;

    private void OnEnable()
    {
        BuildLookups();
    }

    private void BuildLookups()
    {
        mergeLookup = new Dictionary<ulong, UnitData>();
        prefabLookup = new Dictionary<GameObject, UnitData>();

        foreach (var r in recipes)
        {
            if (r == null || r.inputA == null || r.inputB == null || r.output == null)
                continue;

            // merge lookup
            ulong key = MakeKey(r.inputA, r.inputB);
            mergeLookup[key] = r.output;

            // prefab lookup (for resolving prefab -> UnitData)
            RegisterPrefab(r.inputA);
            RegisterPrefab(r.inputB);
            RegisterPrefab(r.output);
        }
    }

    private void RegisterPrefab(UnitData data)
    {
        if (data == null || data.prefab == null) return;
        if (!prefabLookup.ContainsKey(data.prefab))
            prefabLookup.Add(data.prefab, data);
    }

    private static ulong MakeKey(UnitData a, UnitData b)
    {
        int idA = a.GetInstanceID();
        int idB = b.GetInstanceID();

        uint low = (uint)Mathf.Min(idA, idB);
        uint high = (uint)Mathf.Max(idA, idB);

        return ((ulong)high << 32) | low;
    }

    public bool TryGetMergeResult(UnitData a, UnitData b, out UnitData output)
    {
        output = null;
        if (a == null || b == null) return false;

        if (mergeLookup == null) BuildLookups();

        return mergeLookup.TryGetValue(MakeKey(a, b), out output) && output != null;
    }

    public bool TryResolveByPrefab(GameObject prefab, out UnitData data)
    {
        data = null;
        if (prefab == null) return false;

        if (prefabLookup == null) BuildLookups();

        return prefabLookup.TryGetValue(prefab, out data) && data != null;
    }
}
