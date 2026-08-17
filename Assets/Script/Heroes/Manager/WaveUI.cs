using UnityEngine;
using TMPro;

public class WaveUI : MonoBehaviour
{
    [Header("Top Left")]
    [SerializeField] private TextMeshProUGUI waveLabel;        // "Wave : 3"

    [Header("Top Middle")]
    [SerializeField] private TextMeshProUGUI countdownLabel;  // "Next Wave in : 5s"

    [Header("Containers / GameObjects")]
    [SerializeField] private GameObject prepPhaseContainer;
    [SerializeField] private GameObject breakContainer;

    private WaveController waveController;

    // -------------------------------------------------

    private void Awake()
    {
        waveController = FindFirstObjectByType<WaveController>();

        // Ensure containers start disabled
        if (prepPhaseContainer != null)
            prepPhaseContainer.SetActive(false);

        if (breakContainer != null)
            breakContainer.SetActive(false);

        if (countdownLabel != null)
            countdownLabel.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (waveController == null) return;

        // Top left — always visible
        if (waveLabel != null)

            // Used to be
            //waveLabel.text = $"{waveController.CurrentWave}";
            waveLabel.text = $"Wave: {waveController.CurrentWave}";

        // Top middle — visible during prep phase or between-wave break
        if (countdownLabel != null)
        {
            bool isPrep = waveController.IsPrepPhase;
            bool isBreak = waveController.IsBreak;

            countdownLabel.gameObject.SetActive(isBreak || isPrep);

            // Toggle specific GameObjects / Containers
            if (prepPhaseContainer != null)
                prepPhaseContainer.SetActive(isPrep);

            if (breakContainer != null)
                breakContainer.SetActive(isBreak && !isPrep);

            if (isPrep)

                // used to be
                // countdownLabel.text = $"Buy and Place Heroes to defend the castle. " + $"Prepare, Wave in {Mathf.CeilToInt(waveController.BreakTimeRemaining)}s";
                countdownLabel.text = $"Buy, Place and Merge Heroes to defend the castle. " + $"Prepare your Defenders, Incoming Wave in  {Mathf.CeilToInt(waveController.BreakTimeRemaining)}s";
            else if (isBreak)
                countdownLabel.text = $"Next Wave in : {Mathf.CeilToInt(waveController.BreakTimeRemaining)}s";
        }
    }
}