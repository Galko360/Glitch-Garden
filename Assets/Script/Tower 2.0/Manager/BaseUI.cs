using UnityEngine;
using TMPro;

public class BaseUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI hpLabel;

    private void Start()
    {
        if (BaseManager.Instance == null) return;

        Refresh(BaseManager.Instance.Hp, BaseManager.Instance.MaxHp);
        BaseManager.Instance.OnHpChanged += Refresh;
    }

    private void OnDestroy()
    {
        if (BaseManager.Instance != null)
            BaseManager.Instance.OnHpChanged -= Refresh;
    }

    private void Refresh(int current, int max)
    {
        if (hpLabel != null)
            hpLabel.text = $"Base HP: {current} / {max}";
    }
}
