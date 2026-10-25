using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelCatalog
{
    public static readonly string[] Scenes = { "Level1", "Level2", "Level3" };
    public static readonly string[] Names = { "Classic Circuit", "Coastal Oval", "Canyon Run" };
    public static readonly string[] Descriptions = {
        "Long straights & sweeping turns",
        "A wide, flowing road to find your rhythm",
        "Changing-radius bends & a narrower racing line"
    };
    public static string RecordKey(string track, string car) => "BestLap_" + track + "_" + car;
    public static string BestTimeKey => RecordKey(SceneManager.GetActiveScene().name, CarSelection.Current.id);
    public static void Load(int index)
    {
        if (index >= 0 && index < Scenes.Length) SceneManager.LoadScene(Scenes[index]);
    }
}
