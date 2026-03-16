using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class BuyButton : MonoBehaviour
{
    [System.Serializable]
    public class BuyableUnit
    {
        public GameObject prefab;
        public Sprite icon;
    }

    [Header("References")]
    [SerializeField] private InventoryManager inventory;

    [Header("Cost")]
    [SerializeField] private int buyCost = 20;

    [Header("Buyable Units")]
    [SerializeField] private List<BuyableUnit> buyableUnits = new List<BuyableUnit>();

    [Header("Repeat Decreaser")]
    [SerializeField] private bool repeatDecreaserActive = false;
    [SerializeField] private int rollsBeforeDecrease = 2;
    [SerializeField, Range(0f, 1f)] private float chanceReduction = 0.5f;

    // Internal state (lightweight)
    private BuyableUnit lastRolled;
    private int repeatCount = 0;

    public void Buy()
    {
        if (buyableUnits.Count == 0)
            return;

        if (GoldManager.Instance != null && !GoldManager.Instance.TrySpend(buyCost))
        {
            Debug.Log("[BuyButton] Not enough gold.");
            return;
        }

        BuyableUnit selected = RollUnit();

        bool added = inventory.TryAddToFirstEmpty(
            selected.prefab,
            selected.icon
        );

        if (!added)
        {
            // Inventory full � no side effects
            return;
        }

        UpdateRepeatState(selected);
    }

    private BuyableUnit RollUnit()
    {
        if (!repeatDecreaserActive || lastRolled == null || repeatCount < rollsBeforeDecrease)
        {
            return buyableUnits[Random.Range(0, buyableUnits.Count)];
        }

        // Reduced chance roll
        if (Random.value < chanceReduction)
        {
            // Re-roll once, excluding last unit
            List<BuyableUnit> pool = new List<BuyableUnit>(buyableUnits);
            pool.Remove(lastRolled);

            if (pool.Count > 0)
                return pool[Random.Range(0, pool.Count)];
        }

        return buyableUnits[Random.Range(0, buyableUnits.Count)];
    }

    private void UpdateRepeatState(BuyableUnit selected)
    {
        if (selected == lastRolled)
        {
            repeatCount++;
        }
        else
        {
            lastRolled = selected;
            repeatCount = 1;
        }
    }
}
