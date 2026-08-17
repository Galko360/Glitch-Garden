using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class DamageFlash : MonoBehaviour
{
    [Header("Visual Settings")]
    [SerializeField] private Color flashColor = Color.red;
    [SerializeField] private float flashTime = 0.08f;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField, Range(0f, 0.5f)] private float pitchVariance = 0.1f;
    [SerializeField, Range(0f, 0.5f)] private float volumeVariance = 0.1f;
    [SerializeField, Range(0.01f, 3f)] private float baseAudioVolume = 1f;
    [SerializeField, Range(1f, 5f)] private float volumeBoost = 1.5f; // Allows making it louder easily

    [Header("Audio Toggle")]
    [field: SerializeField] public bool PlayHitSoundEnabled { get; set; } = true;

    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private Coroutine routine;
    private AudioSource audioSource;

    private static int activeCharacterCount = 0;

    private void Awake()
    {
        renderers = GetComponentsInChildren<SpriteRenderer>();

        originalColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            originalColors[i] = renderers[i].color;

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 1f;
    }

    private void OnEnable()
    {
        activeCharacterCount++;
    }

    private void OnDisable()
    {
        activeCharacterCount = Mathf.Max(1, activeCharacterCount - 1);
    }

    public void Flash()
    {
        if (renderers.Length == 0) return;

        // Play sound immediately before the flash runs
        PlayHitSound();

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(FlashRoutine());
    }

    private void PlayHitSound()
    {
        if (!PlayHitSoundEnabled || hitSound == null || audioSource == null) return;

        audioSource.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);

        float crowdMultiplier = 1f / Mathf.Sqrt(Mathf.Max(1, activeCharacterCount));
        float finalVolume = baseAudioVolume * volumeBoost * crowdMultiplier;
        float randomizedVolume = finalVolume + Random.Range(-volumeVariance, volumeVariance);

        audioSource.PlayOneShot(hitSound, Mathf.Clamp01(randomizedVolume));
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