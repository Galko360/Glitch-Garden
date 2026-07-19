using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TMP_Text goldText;

    private void Start()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldChanged += UpdateDisplay;
            UpdateDisplay(GoldManager.Instance.Gold);
        }
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldChanged -= UpdateDisplay;
    }

    private void UpdateDisplay(int gold)
    {
        goldText.text = $"Gold: {gold}";
    }
}
