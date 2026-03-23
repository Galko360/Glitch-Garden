using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Back")]
    [SerializeField] private GameObject pausePanel;

    // PlayerPrefs keys
    private const string MusicKey = "MusicVolume";
    private const string SfxKey   = "SfxVolume";

    // -------------------------------------------------

    private void OnEnable()
    {
        // Load saved values (default 1.0 = full volume)
        float music = PlayerPrefs.GetFloat(MusicKey, 1f);
        float sfx   = PlayerPrefs.GetFloat(SfxKey,   1f);

        if (musicSlider != null) musicSlider.value = music;
        if (sfxSlider   != null) sfxSlider.value   = sfx;

        ApplyMusic(music);
        ApplySfx(sfx);
    }

    // -------------------------------------------------
    // Called by sliders OnValueChanged

    public void OnMusicChanged(float value)
    {
        ApplyMusic(value);
        PlayerPrefs.SetFloat(MusicKey, value);
    }

    public void OnSfxChanged(float value)
    {
        ApplySfx(value);
        PlayerPrefs.SetFloat(SfxKey, value);
    }

    // -------------------------------------------------

    private void ApplyMusic(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("MusicVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    private void ApplySfx(float value)
    {
        if (audioMixer != null)
            audioMixer.SetFloat("SfxVolume", Mathf.Log10(Mathf.Max(value, 0.0001f)) * 20f);
    }

    // -------------------------------------------------

    public void Back()
    {
        gameObject.SetActive(false);
        pausePanel?.SetActive(true);
    }
}
