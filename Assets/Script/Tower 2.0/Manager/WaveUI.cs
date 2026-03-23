using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("Top Left")]
    [SerializeField] private TextMeshProUGUI waveLabel;       // "Wave : 3"

    [Header("Top Middle")]
    [SerializeField] private TextMeshProUGUI countdownLabel;  // "Next Wave in : 5s"

    private WaveController waveController;

    // -------------------------------------------------

    private void Awake()
    {
        waveController = FindFirstObjectByType<WaveController>();

        if (countdownLabel != null)
            countdownLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (waveController == null) return;

        // Top left — always visible
        if (waveLabel != null)
            waveLabel.text = $"Wave : {waveController.CurrentWave}";

        // Top middle — only visible during break
        if (countdownLabel != null)
        {
            countdownLabel.gameObject.SetActive(waveController.IsBreak);

            if (waveController.IsBreak)
                countdownLabel.text = $"Next Wave in : {Mathf.CeilToInt(waveController.BreakTimeRemaining)}s";
        }
    }
}
