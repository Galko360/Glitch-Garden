using UnityEngine;
using TMPro;

/// <summary>
/// Shows current wave number and a countdown during the break between waves.
/// Attach anywhere in the Canvas. Wire up the two TMP text fields.
/// </summary>
public class WaveUI : MonoBehaviour
{
    [SerializeField] private WaveController waveController;
    [SerializeField] private TextMeshProUGUI waveLabel;       // e.g. "Wave 4"
    [SerializeField] private TextMeshProUGUI breakLabel;      // e.g. "Next wave in 5s..." (hidden during wave)

    private void Awake()
    {
        if (waveController == null)
            waveController = FindFirstObjectByType<WaveController>();
    }

    private void Update()
    {
        if (waveController == null) return;

        if (waveLabel != null)
            waveLabel.text = $"Wave {waveController.CurrentWave}";

        if (breakLabel != null)
        {
            if (waveController.IsBreak)
                breakLabel.text = $"Next wave in {Mathf.CeilToInt(waveController.BreakTimeRemaining)}s...";
            else
                breakLabel.text = string.Empty;
        }
    }
}
