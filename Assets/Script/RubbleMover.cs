using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Castle rubble that arcs along its assigned checkpoints to a destination,
/// then fires a callback. Unlike Coin it neither destroys itself nor awards currency.
/// </summary>
public class RubbleMover : CheckpointFlyer
{
    private Action onArrival;

    // -------------------------------------------------

    private void Reset() => targetTag = "TargetDestination";

    /// <summary>
    /// Call when spawning the rubble to start its movement along the assigned checkpoints.
    /// </summary>
    /// <param name="onComplete">Optional callback when movement finishes</param>
    /// <param name="customTarget">Optional override for the final destination</param>
    /// <param name="customCheckpoints">Optional runtime override for checkpoints</param>
    public void Launch(
        Action onComplete = null,
        Transform customTarget = null,
        List<Transform> customCheckpoints = null)
    {
        onArrival = onComplete;

        if (customTarget != null)
            target = customTarget;

        if (customCheckpoints != null && customCheckpoints.Count > 0)
            checkpoints = customCheckpoints;

        BeginFlight();
    }

    protected override void OnArrived() => onArrival?.Invoke();
}
