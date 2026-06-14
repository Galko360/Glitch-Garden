using System;
using System.Collections;
using UnityEngine;

public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    [Header("Starting Gold")]
    [SerializeField] private int startingGold = 100;

    [Header("Passive Income")]
    [SerializeField] private int incomeAmount = 10;
    [SerializeField] private float incomeInterval = 5f;

    public int Gold { get; private set; }

    public event Action<int> OnGoldChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        Gold = startingGold;
        OnGoldChanged?.Invoke(Gold);
        StartCoroutine(PassiveIncome());
    }

    private IEnumerator PassiveIncome()
    {
        while (true)
        {
            yield return new WaitForSeconds(incomeInterval);
            AddGold(incomeAmount);
        }
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool TrySpend(int cost)
    {
        if (Gold < cost) return false;
        Gold -= cost;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
