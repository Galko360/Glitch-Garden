using System;
using UnityEngine;

[Serializable]
public sealed class MusicTrackLibrary
{
    [Header("Launch")]
    [Tooltip("Played once when the application starts.")]
    [SerializeField] private AudioClip gameLaunched;

    [Header("Main Menu")]
    [Tooltip("Loops while the player is in the main menu.")]
    [SerializeField] private AudioClip mainMenuLoop;

    [Header("Wave Preparation")]
    [Tooltip("Loops during the initial preparation and every break between waves.")]
    [SerializeField] private AudioClip prepForWave;

    [Header("Wave Start")]
    [Tooltip("Played once whenever a new wave begins.")]
    [SerializeField] private AudioClip waveStarted;

    [Header("Active Wave")]
    [Tooltip("Loops during the active wave after the Wave Started track finishes.")]
    [SerializeField] private AudioClip gameLoop;

    [Header("Game Over")]
    [Tooltip("Played once immediately when the base dies.")]
    [SerializeField] private AudioClip gameOver;

    public AudioClip GameLaunched => gameLaunched;
    public AudioClip MainMenuLoop => mainMenuLoop;
    public AudioClip PrepForWave => prepForWave;
    public AudioClip WaveStarted => waveStarted;
    public AudioClip GameLoop => gameLoop;
    public AudioClip GameOver => gameOver;
}