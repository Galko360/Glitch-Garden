using System;
using UnityEngine;

public interface IMusicPlayback
{
    bool IsPlaying { get; }

    void PlayLoop(AudioClip clip);

    void PlayOneShot(AudioClip clip, Action onFinished);

    void Stop();
}