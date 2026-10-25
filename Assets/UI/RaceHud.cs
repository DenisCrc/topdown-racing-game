using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RaceHud : MonoBehaviour
{
    public Text circuit;
    public Text carName;
    public RacingMinimap minimap;
    public void Restart() { FindAnyObjectByType<Logic>().restartGame(); }
    public void MainMenu() { FindAnyObjectByType<Logic>().MainMenu(); }
    void Start()
    {
        int index = Array.IndexOf(LevelCatalog.Scenes, SceneManager.GetActiveScene().name);
        circuit.text = index >= 0 ? LevelCatalog.Names[index] : "Time trial";
        carName.text = CarSelection.Current.displayName;
        minimap.Bind(FindAnyObjectByType<CircuitTrack>(), FindAnyObjectByType<CarScript>().transform);
    }
}
