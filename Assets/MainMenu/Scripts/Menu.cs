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
    public GameObject[] circuitPages;
    public Button[] pageFirst;
    public Text pageLabel;
    int page;
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
        ShowCircuitPageFor(0);
    }
    public void ShowCircuitPageFor(int circuitIndex) { SetPage(circuitIndex / 3); }
    public void NextCircuitPage() { SetPage((page + 1) % circuitPages.Length); }
    public void PreviousCircuitPage() { SetPage((page + circuitPages.Length - 1) % circuitPages.Length); }
    void SetPage(int value)
    {
        page = Mathf.Clamp(value, 0, circuitPages.Length - 1);
        for (int i = 0; i < circuitPages.Length; i++) circuitPages[i].SetActive(i == page);
        pageLabel.text = (page + 1) + " / " + circuitPages.Length;
        Focus(pageFirst[page]);
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
