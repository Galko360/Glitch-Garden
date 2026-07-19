using System.Collections;
using UnityEngine;

public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashTime = 0.08f;

    SpriteRenderer[] renderers;
    Color[] originalColors;
    Coroutine routine;

    void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

    }

    public void Flash()
    {
        if (renderers.Length == 0) return;

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        foreach (var r in renderers)
            r.color = flashColor;

        yield return new WaitForSeconds(flashTime);

        for (int i = 0; i < renderers.Length; i++)
            renderers[i].color = originalColors[i];
    }
}
