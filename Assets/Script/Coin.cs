using System.Collections;
using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Target & Movement Settings")]
    [SerializeField] private string moneyBagTag = "MoneyBag"; // Search tag fallback
    [SerializeField] private Transform target;                // Assign manually in Prefab or let Auto-Find run
    [SerializeField] private float travelDuration = 0.8f;
    [SerializeField] private Vector2 arcControl = new Vector2(0f, 2f); // Controls curve height/angle

    private int value;


    private void Awake()
    {
        // Auto-find MoneyBag target if not manually assigned in inspector
        if (target == null && !string.IsNullOrEmpty(moneyBagTag))
        {
            GameObject bagObj = GameObject.FindGameObjectWithTag(moneyBagTag);
            if (bagObj != null) target = bagObj.transform;
        }
    }

    /// <summary>
    /// Call this from Enemy to launch the coin toward its destination.
    /// </summary>
    public void Launch(int goldAmount, Transform customTarget = null)
    {
        value = goldAmount;

        if (customTarget != null)
            target = customTarget;

        StartCoroutine(FlyToTargetRoutine());
    }

    private IEnumerator FlyToTargetRoutine()
    {
        Vector3 startPos = transform.position;
        float elapsedTime = 0f;

        Vector3 targetPos = target != null ? target.position : startPos;
        Vector3 midPoint = Vector3.Lerp(startPos, targetPos, 0.5f) + (Vector3)arcControl;

        while (elapsedTime < travelDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / travelDuration);

            // Update real-time target position in case the target moves
            if (target != null)
            {
                targetPos = target.position;
                midPoint = Vector3.Lerp(startPos, targetPos, 0.5f) + (Vector3)arcControl;
            }

            // Quadratic Bezier Curve formula
            Vector3 positionOnCurve = Mathf.Pow(1f - t, 2) * startPos +
                                      2f * (1f - t) * t * midPoint +
                                      Mathf.Pow(t, 2) * targetPos;

            transform.position = positionOnCurve;

            yield return null;
        }

        // Award gold on arrival
        GoldManager.Instance?.AddGold(value);

        Destroy(gameObject);
    }
}