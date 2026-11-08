using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>
/// Gold pickup that arcs from a dead enemy to the money bag, then banks its value.
/// Finds its own checkpoints by name (Coin_CP_1, Coin_CP_2, ...).
/// </summary>
public class Coin : CheckpointFlyer
{
    [Header("Auto-Checkpoint Settings")]
    [SerializeField] private bool useAutoCheckpoints = true;
    [Tooltip("Objects whose name starts with this are collected and sorted numerically.")]
    [SerializeField] private string checkpointPrefix = "Coin_CP_";

    private int value;

    // -------------------------------------------------

    private void Reset() => targetTag = "MoneyBag";

    protected override void Awake()
    {
        base.Awake();

        if (useAutoCheckpoints)
            DiscoverCheckpoints();
    }

    /// <summary>Call this from Enemy to launch the coin.</summary>
    public void Launch(int goldAmount, Transform customTarget = null)
    {
        value = goldAmount;

        if (customTarget != null)
            target = customTarget;

        BeginFlight();
    }

    protected override void OnArrived()
    {
        GoldManager.Instance?.AddGold(value);
        Destroy(gameObject);
    }

    // -------------------------------------------------

    private void DiscoverCheckpoints()
    {
        checkpoints.Clear();

        foreach (GameObject obj in FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (obj != null && obj.name.StartsWith(checkpointPrefix))
                checkpoints.Add(obj.transform);
        }

        // Coin_CP_1, Coin_CP_2, ... Coin_CP_10 in numeric (not alphabetical) order
        checkpoints.Sort((a, b) => NumberIn(a.name).CompareTo(NumberIn(b.name)));
    }

    private static int NumberIn(string name)
    {
        int.TryParse(Regex.Match(name, @"\d+").Value, out int result);
        return result;
    }
}
