using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Flies along a list of checkpoints in a smooth arc, then reports arrival.
/// Subclasses supply the checkpoints and decide what happens at the end.
/// </summary>
public abstract class CheckpointFlyer : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Looked up by tag on Awake if Target is left empty.")]
    [FormerlySerializedAs("moneyBagTag")]
    [SerializeField] protected string targetTag = "";
    [SerializeField] protected Transform target;

    [Header("Checkpoints")]
    [Tooltip("Waypoints passed through in order, before heading to Target.")]
    [SerializeField] protected List<Transform> checkpoints = new List<Transform>();

    [Header("Movement Tweaks")]
    [SerializeField] protected float totalTravelDuration = 1.2f;
    [SerializeField] protected Vector2 arcControl = new Vector2(0f, 2f);

    // -------------------------------------------------

    protected virtual void Awake()
    {
        if (target == null && !string.IsNullOrEmpty(targetTag))
        {
            GameObject found = GameObject.FindGameObjectWithTag(targetTag);
            if (found != null) target = found.transform;
        }
    }

    /// <summary>Start the flight. Call this from the subclass's own Launch method.</summary>
    protected void BeginFlight() => StartCoroutine(FlightRoutine());

    // -------------------------------------------------

    private IEnumerator FlightRoutine()
    {
        List<Vector3> path = BuildPath();

        int segmentCount = Mathf.Max(1, path.Count - 1);
        float segmentDuration = totalTravelDuration / segmentCount;

        for (int segment = 0; segment < segmentCount; segment++)
        {
            bool isLastSegment = segment == segmentCount - 1;
            bool tracksTarget = isLastSegment && target != null;

            Vector3 start = path[segment];
            Vector3 end = tracksTarget ? target.position : path[segment + 1];
            Vector3 mid = ArcMidpoint(start, end);

            float elapsed = 0f;

            while (elapsed < segmentDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / segmentDuration);

                // Re-aim every frame on the final leg, in case the target moves
                if (tracksTarget)
                {
                    end = target.position;
                    mid = ArcMidpoint(start, end);
                }

                transform.position = QuadraticBezier(start, mid, end, t);
                yield return null;
            }
        }

        OnArrived();
    }

    private List<Vector3> BuildPath()
    {
        List<Vector3> path = new List<Vector3> { transform.position };

        foreach (Transform cp in checkpoints)
        {
            if (cp != null) path.Add(cp.position);
        }

        path.Add(target != null ? target.position : path[path.Count - 1]);
        return path;
    }

    private Vector3 ArcMidpoint(Vector3 start, Vector3 end)
        => Vector3.Lerp(start, end, 0.5f) + (Vector3)arcControl;

    private static Vector3 QuadraticBezier(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float inv = 1f - t;
        return (inv * inv * a) + (2f * inv * t * b) + (t * t * c);
    }

    // -------------------------------------------------

    /// <summary>Called once the flight finishes.</summary>
    protected abstract void OnArrived();
}
