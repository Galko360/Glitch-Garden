using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Unity-specific implementation of music playback.
/// The rest of the music system depends on IMusicPlayback instead
/// of directly controlling an AudioSource.
/// </summary>
public sealed class UnityMusicPlayback : IMusicPlayback
{
    private readonly AudioSource audioSource;
    private readonly MonoBehaviour coroutineHost;

    private Coroutine completionCoroutine;
    private int playbackVersion;

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;

    public UnityMusicPlayback(AudioSource audioSource, MonoBehaviour coroutineHost)
    {
        this.audioSource = audioSource;
        this.coroutineHost = coroutineHost;
    }

    public void PlayLoop(AudioClip clip)
    {
        Stop();

        if (clip == null)
            return;

        audioSource.loop = true;
        audioSource.clip = clip;
        audioSource.Play();
    }

    public void PlayOneShot(AudioClip clip, Action onFinished)
    {
        Stop();

        if (clip == null)
        {
            onFinished?.Invoke();
            return;
        }

        audioSource.loop = false;
        audioSource.clip = clip;
        audioSource.Play();

        int versionAtStart = playbackVersion;

        completionCoroutine = coroutineHost.StartCoroutine(
            WaitForCompletion(clip, versionAtStart, onFinished)
        );
    }

    public void Stop()
    {
        playbackVersion++;

        if (completionCoroutine != null)
        {
            coroutineHost.StopCoroutine(completionCoroutine);
            completionCoroutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
            audioSource.loop = false;
        }
    }

    private IEnumerator WaitForCompletion(
        AudioClip expectedClip,
        int versionAtStart,
        Action onFinished)
    {
        // Audio playback continues even if Time.timeScale becomes 0.
        yield return new WaitForSecondsRealtime(expectedClip.length);

        if (versionAtStart != playbackVersion)
            yield break;

        if (audioSource == null)
            yield break;

        if (audioSource.clip != expectedClip)
            yield break;

        completionCoroutine = null;
        onFinished?.Invoke();
    }
}