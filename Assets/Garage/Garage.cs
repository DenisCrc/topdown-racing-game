using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Garage : MonoBehaviour
{
    public Image preview;
    public Text carName;
    public Text description;
    public Text stats;
    public Text counter;
    public Text selection;
    public Button selectButton;
    public Button previousButton;
    int index;
    public CarDefinition PreviewCar => CarSelection.All[index];
    void Start()
    {
        index = System.Array.IndexOf(CarSelection.All, CarSelection.Current);
        Refresh();
        Menu.Focus(previousButton);
    }
    void Update() { if (Input.GetKeyDown(KeyCode.Escape)) Back(); }
    public void Previous() { index = (index + CarSelection.All.Length - 1) % CarSelection.All.Length; Refresh(); }
    public void Next() { index = (index + 1) % CarSelection.All.Length; Refresh(); }
    public void Select() { CarSelection.Select(PreviewCar); Refresh(); }
    public void Refresh()
    {
        var car = PreviewCar;
        preview.sprite = car.straight;
        carName.text = car.displayName;
        description.text = car.description;
        counter.text = (index + 1).ToString("00") + " / " + CarSelection.All.Length.ToString("00");
        stats.text = "TOP SPEED     " + car.maxSpeed.ToString("0") + "\nACCELERATION  " + car.acceleration.ToString("0")
            + "\nSTEERING      " + car.steering.ToString("0.0") + "\nGRIP          " + ((1 - car.driftFactor) * 100).ToString("0");
        selection.text = car == CarSelection.Current ? "SELECTED FOR EVERY CIRCUIT" : "SELECT TO TAKE THIS CAR RACING";
        selectButton.GetComponentInChildren<Text>().text = car == CarSelection.Current ? "Selected" : "Select car";
    }
    public void Back() { SceneManager.LoadScene("MainMenu"); }
}
