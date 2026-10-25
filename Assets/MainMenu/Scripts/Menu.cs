using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public GameObject home;
    public GameObject levels;
    public Button homeFirst;
    public Button levelsFirst;
    public Text selectedCar;
    void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        ShowMainMenu();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && levels.activeSelf) ShowMainMenu();
    }
    public void ShowMainMenu()
    {
        levels.SetActive(false);
        home.SetActive(true);
        selectedCar.text = "YOUR CAR  /  " + CarSelection.Current.displayName;
        Focus(homeFirst);
    }
    public void Play()
    {
        home.SetActive(false);
        levels.SetActive(true);
        Focus(levelsFirst);
    }
    public static void Focus(Selectable target)
    {
        if (Application.isPlaying && EventSystem.current != null && target != null)
            EventSystem.current.SetSelectedGameObject(target.gameObject);
    }
    public void LoadGarage() { SceneManager.LoadScene("Garage"); }
    public void LoadSettings() { SceneManager.LoadScene("Settings"); }
    public void Quit() { Application.Quit(); }
}
