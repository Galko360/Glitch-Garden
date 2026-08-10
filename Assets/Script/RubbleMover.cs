using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RubbleMover : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    [SerializeField] private string targetTag = "TargetDestination";
    [SerializeField] private Transform target;

    [Header("Checkpoints")]
    [Tooltip("Assign the specific checkpoint transforms for this rubble's path in order.")]
    [SerializeField] private List<Transform> checkpoints = new List<Transform>();

    [Header("Movement Tweaks")]
    [SerializeField] private float totalTravelDuration = 1.2f;
    [SerializeField] private Vector2 arcControl = new Vector2(0f, 2f);

    private System.Action onArrivalCallback;

    private void Awake()
    {
        // Auto-find final target destination tag if not assigned
        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject targetObj = GameObject.FindGameObjectWithTag(targetTag);
            if (targetObj != null) target = targetObj.transform;
        }
    }

    /// <summary>
    /// Call this when spawning the rubble to start its movement along the assigned checkpoints.
    /// </summary>
    /// <param name="onComplete">Optional callback when movement finishes</param>
    /// <param name="customTarget">Optional override for the final destination</param>
    /// <param name="customCheckpoints">Optional runtime override for checkpoints</param>
    public void Launch(System.Action onComplete = null, Transform customTarget = null, List<Transform> customCheckpoints = null)
    {
        onArrivalCallback = onComplete;

        if (customTarget != null)
        {
            target = customTarget;
        }

        if (customCheckpoints != null && customCheckpoints.Count > 0)
        {
            checkpoints = customCheckpoints;
        }

        StartCoroutine(FlyThroughCheckpointsRoutine());
    }

    private IEnumerator FlyThroughCheckpointsRoutine()
    {
        List<Vector3> pathPoints = new List<Vector3>();
        pathPoints.Add(transform.position);

        // Add the manually assigned inspector checkpoints sequentially
        if (checkpoints != null)
        {
            foreach (var cp in checkpoints)
            {
                if (cp != null) pathPoints.Add(cp.position);
            }
        }

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

        // Triggers the arrival callback without destroying the object or modifying currency
        onArrivalCallback?.Invoke();
    }
}