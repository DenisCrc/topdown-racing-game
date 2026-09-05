using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    private bool Fullscreen = true;

    [SerializeField] private Slider MasterVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Toggle FullscreenToggle;
    [SerializeField] private Toggle CameraShakeToggle;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Fullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        Screen.fullScreen = Fullscreen;
    }
    void Start()
    {
        MasterVolumeSlider.value = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        MusicVolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        SFXVolumeSlider.value = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        FullscreenToggle.isOn = Fullscreen;
        CameraShakeToggle.isOn = PlayerPrefs.GetInt("CameraShake", 1) == 1;
    }

    // Update is called once per frame
    void Update()
    {
        PlayerPrefs.SetFloat("MasterVolume", MasterVolumeSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", MusicVolumeSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", SFXVolumeSlider.value);
        PlayerPrefs.SetInt("Fullscreen", FullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("CameraShake", CameraShakeToggle.isOn ? 1 : 0);
    }

    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
