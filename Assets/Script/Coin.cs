using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    [SerializeField] private string moneyBagTag = "MoneyBag";
    [SerializeField] private Transform target;

    [Header("Auto-Checkpoint Settings")]
    [SerializeField] private bool useAutoCheckpoints = true;
    [SerializeField] private string checkpointPrefix = "Coin_CP_"; // Looks for Coin_CP_1, Coin_CP_2, etc.

    [Header("Movement Tweaks")]
    [SerializeField] private float totalTravelDuration = 1.2f;
    [SerializeField] private Vector2 arcControl = new Vector2(0f, 2f);

    private int value;
    private List<Transform> discoveredCheckpoints = new List<Transform>();


    private void Awake()
    {
        // 1. Auto-find MoneyBag target if not assigned
        if (target == null && !string.IsNullOrEmpty(moneyBagTag))
        {
            GameObject bagObj = GameObject.FindGameObjectWithTag(moneyBagTag);
            if (bagObj != null) target = bagObj.transform;
        }

        // 2. Auto-find and sequence checkpoints by name pattern if enabled
        if (useAutoCheckpoints)
        {
            FindAndSortCheckpoints();
        }
    }

    private void FindAndSortCheckpoints()
    {
        discoveredCheckpoints.Clear();

        List<Transform> foundList = new List<Transform>();

        // Modern, performance-friendly API call matching current Unity standards
        GameObject[] allObjects = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (var obj in allObjects)
        {
            if (obj != null && obj.name.StartsWith(checkpointPrefix))
            {
                foundList.Add(obj.transform);
            }
        }

        // Sort them numerically (e.g., Coin_CP_1, Coin_CP_2, Coin_CP_3)
        foundList.Sort((a, b) => {
            int numA = ExtractNumber(a.name);
            int numB = ExtractNumber(b.name);
            return numA.CompareTo(numB);
        });

        discoveredCheckpoints = foundList;
    }

    private int ExtractNumber(string name)
    {
        string numberOnly = System.Text.RegularExpressions.Regex.Match(name, @"\d+").Value;
        int.TryParse(numberOnly, out int result);
        return result;
    }

    /// <summary>
    /// Call this from Enemy to launch the coin.
    /// </summary>
    public void Launch(int goldAmount, Transform customTarget = null)
    {
        value = goldAmount;

        if (customTarget != null)
        {
            target = customTarget;
        }

        StartCoroutine(FlyThroughCheckpointsRoutine());
    }

    private IEnumerator FlyThroughCheckpointsRoutine()
    {
        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(transform.position);

        // Add all discovered checkpoints in order
        foreach (var cp in discoveredCheckpoints)
        {
            if (cp != null) pathPoints.Add(cp.position);
        }

        // Ensure the final target is at the end of the path
        Vector3 finalTargetPos = target != null ? target.position : pathPoints[pathPoints.Count - 1];
        pathPoints.Add(finalTargetPos);

        int totalSegments = pathPoints.Count - 1;
        if (totalSegments <= 0) totalSegments = 1;

        float segmentDuration = totalTravelDuration / totalSegments;
        int currentSegment = 0;

        while (currentSegment < totalSegments)
        {
            Vector3 startPos = pathPoints[currentSegment];

            bool isLastSegment = (currentSegment == totalSegments - 1);
            Vector3 endPos = isLastSegment && target != null ? target.position : pathPoints[currentSegment + 1];

            Vector3 midPoint = Vector3.Lerp(startPos, endPos, 0.5f) + (Vector3)arcControl;

            float elapsedTime = 0f;

            while (elapsedTime < segmentDuration)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / segmentDuration);

                if (isLastSegment && target != null)
                {
                    endPos = target.position;
                    midPoint = Vector3.Lerp(startPos, endPos, 0.5f) + (Vector3)arcControl;
                }

                Vector3 positionOnCurve = Mathf.Pow(1f - t, 2) * startPos +
                                         2f * (1f - t) * t * midPoint +
                                         Mathf.Pow(t, 2) * endPos;

                transform.position = positionOnCurve;

                yield return null;
            }

            currentSegment++;
        }

        GoldManager.Instance?.AddGold(value);
        Destroy(gameObject);
    }
}