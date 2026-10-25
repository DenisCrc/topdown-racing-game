using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Settings : MonoBehaviour
{
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;
    public Toggle FullscreenToggle;
    public Toggle CameraShakeToggle;
    public Button backButton;
    void Start()
    {
        MasterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MasterVolume", 0.5f));
        MusicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("MusicVolume", 0.5f));
        SFXVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("SFXVolume", 0.5f));
        FullscreenToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("Fullscreen", 1) == 1);
        CameraShakeToggle.SetIsOnWithoutNotify(PlayerPrefs.GetInt("CameraShake", 1) == 1);
        MasterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        MusicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        SFXVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        FullscreenToggle.onValueChanged.AddListener(SetFullscreen);
        CameraShakeToggle.onValueChanged.AddListener(SetCameraShake);
        AudioListener.volume = MasterVolumeSlider.value;
        Menu.Focus(MasterVolumeSlider);
    }
    void SetMasterVolume(float value) { PlayerPrefs.SetFloat("MasterVolume", value); AudioListener.volume = value; }
    void SetMusicVolume(float value) { PlayerPrefs.SetFloat("MusicVolume", value); }
    void SetSfxVolume(float value) { PlayerPrefs.SetFloat("SFXVolume", value); }
    void SetFullscreen(bool value) { PlayerPrefs.SetInt("Fullscreen", value ? 1 : 0); Screen.fullScreen = value; }
    void SetCameraShake(bool value) { PlayerPrefs.SetInt("CameraShake", value ? 1 : 0); }
    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) BackToMenu(); }
    void OnDisable() { PlayerPrefs.Save(); }
    public void BackToMenu() { SceneManager.LoadScene("MainMenu"); }
}
