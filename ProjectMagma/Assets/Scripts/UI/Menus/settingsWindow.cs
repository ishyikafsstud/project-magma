using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;


public class settingsWindow : MonoBehaviour
{
    [Header("---- Elements ----")]
    [SerializeField] Slider masterVolumeSlider;
    [SerializeField] Slider sfxVolumeSlider;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider uiVolumeSlider;
    [SerializeField] Slider sensitivitySlider;
    [SerializeField] Toggle tiltToggle;
    [SerializeField] Toggle invertYToggle;

    [Header("---- Other Dependencies ----")]
    [SerializeField] private AudioMixer audioMixer;

    private void Start()
    {
        GeneralSettingsData savedSettings = saveSystem.LoadGeneralSettings();
        ApplyGeneralSettings(savedSettings);
    }

    void ApplyGeneralSettings(GeneralSettingsData settingsData)
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(settingsData.masterVolume);
            sfxVolumeSlider.SetValueWithoutNotify(settingsData.sfxVolume);
            musicVolumeSlider.SetValueWithoutNotify(settingsData.musicVolume);
            uiVolumeSlider.SetValueWithoutNotify(settingsData.uiVolume);
            sensitivitySlider.SetValueWithoutNotify(settingsData.mouseSensitivity);
            tiltToggle.SetIsOnWithoutNotify(settingsData.tiltEnabled);
            invertYToggle.SetIsOnWithoutNotify(settingsData.invertY);
        }
    }

    public void MasterVolumeChanged()
    {
        //Debug.Log("Slider Test: Master Volume Changed.");
        SetAudioMixerVolume("master", masterVolumeSlider.value);
    }

    public void SFXVolumeChanged()
    {
        //Debug.Log("Slider Test: SFX Volume Changed.");
        SetAudioMixerVolume("sfx", sfxVolumeSlider.value);
    }
    public void MusicVolumeChanged()
    {
        //Debug.Log("Slider Test: Music Volume Changed.");
        SetAudioMixerVolume("music", musicVolumeSlider.value);
    }
    public void UIVolumeChanged()
    {
        //Debug.Log("Slider Test: UI Volume Changed.");
        SetAudioMixerVolume("ui", uiVolumeSlider.value);
    }

    private void SetAudioMixerVolume(string paramName, float newValue)
    {
        audioMixer.SetFloat(paramName, Mathf.Log10(newValue) * 20);

        saveSystem.SaveVolume(paramName, newValue);
    }

    public void SensitivityChanged()
    {
        //Debug.Log("Slider Test: Sensitivity Changed.");
        saveSystem.SaveSensitivity((int)sensitivitySlider.value);
    }

    public void TiltCameraChanged()
    {
        //Debug.Log("Toggle Test: Camera Tilt Changed.");
        saveSystem.SaveTilt(tiltToggle.isOn);
    }

    public void InvertYChanged()
    {
        //Debug.Log("Toggle Test: Invert Y Changed.");
        saveSystem.SaveInvertY(invertYToggle.isOn);
    }
}
