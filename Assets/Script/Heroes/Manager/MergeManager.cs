using System.Collections.Generic;
using UnityEngine;

public class MergeManager : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private LayerMask tileLayer;

    [Header("Merge")]
    [SerializeField] private MergeDatabase mergeDatabase;

    // Track what is on each tile (so we can merge/replace)
    private readonly Dictionary<TileCell, Transform> tileToUnit = new();
    private readonly Dictionary<TileCell, UnitData> tileToData = new();

    private void Reset()
    {
        cam = Camera.main;
    }

    public bool TryPlace(GameObject unitPrefab, Vector2 screenPosition)
    {
        if (cam == null) cam = Camera.main;

        if (mergeDatabase == null)
        {
            Debug.LogError("MergeManager: mergeDatabase missing");
            return false;
        }

        if (!mergeDatabase.TryResolveByPrefab(unitPrefab, out UnitData incomingData))
        {
            Debug.LogError("MergeManager: Could not resolve UnitData for prefab (missing in MergeDatabase)");
            return false;
        }

        Vector3 world = cam.ScreenToWorldPoint(screenPosition);
        world.z = 0f;

        Collider2D hit = Physics2D.OverlapPoint(world, tileLayer);
        if (hit == null) return false;

        TileCell tile = hit.GetComponent<TileCell>();
        if (tile == null) return false;

        // If our dictionaries say it's occupied but the object was destroyed, clean it up
        if (tileToUnit.TryGetValue(tile, out Transform existingTf) && existingTf == null)
        {
            tileToUnit.Remove(tile);
            tileToData.Remove(tile);
            tile.Clear(); // safe even if your TileCell auto-frees
        }

        // -------------------------
        // EMPTY: normal placement
        // -------------------------
        if (!tile.IsOccupied)
        {
            GameObject unit = Instantiate(unitPrefab, tile.CenterWorld, Quaternion.identity);

            if (unit.TryGetComponent<UnitCombat>(out var combat))
            {
                // Lane row assignment (independent of death subscription)
                Lane lane = tile.GetComponentInParent<Lane>();
                if (lane != null) combat.SetRow(lane.RowIndex);

                // Always subscribe to death so the tile frees itself
                SubscribeToUnitDeath(combat, tile);
            }

            tile.TryOccupy(unit.transform);
            tileToUnit[tile] = unit.transform;
            tileToData[tile] = incomingData;
            return true;
        }

        // -------------------------
        // OCCUPIED: attempt merge
        // -------------------------
        if (!tileToData.TryGetValue(tile, out UnitData existingData) || existingData == null)
        {
            Debug.LogWarning("MergeManager: Tile is occupied but has no UnitData tracked (dictionary missing).");
            return false;
        }

        if (!mergeDatabase.TryGetMergeResult(existingData, incomingData, out UnitData outputData))
            return false; // no recipe

        // Merge success: destroy old, place new
        if (tileToUnit.TryGetValue(tile, out Transform oldTf) && oldTf != null)
            Destroy(oldTf.gameObject);

        tile.Clear();

        GameObject merged = Instantiate(outputData.prefab, tile.CenterWorld, Quaternion.identity);

        if (merged.TryGetComponent<UnitCombat>(out var mergedCombat))
        {
            Lane mergedLane = tile.GetComponentInParent<Lane>();
            if (mergedLane != null) mergedCombat.SetRow(mergedLane.RowIndex);
        }

        tile.TryOccupy(merged.transform);

        tileToUnit[tile] = merged.transform;
        tileToData[tile] = outputData;

        if (merged.TryGetComponent<UnitCombat>(out var mergedCombat2))
            SubscribeToUnitDeath(mergedCombat2, tile);

        return true;
    }

    // ─── Death cleanup ─────────────────────────────────────────────────────────

    /// <summary>
    /// When the unit on <paramref name="tile"/> fires OnDeath, clear the tile
    /// and remove it from both tracking dictionaries.
    /// </summary>
    private void SubscribeToUnitDeath(UnitCombat combat, TileCell tile)
    {
        combat.OnDeath += () =>
        {
            tile.Clear();               // marks tile free + resets icon to Unoccupied
            tileToUnit.Remove(tile);
            tileToData.Remove(tile);
        };
    }
}
