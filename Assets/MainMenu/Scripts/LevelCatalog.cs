using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelCatalog
{
    public static readonly string[] Scenes = { "Level1", "Level2", "Level3", "Level4", "Level5" };
    public static readonly int[] Layouts = { 2, 0, 1, 3, 4 };
    public static readonly string[] Names = { "Classic Circuit", "Coastal Grand Prix", "Canyon Run", "Highland Grand Prix", "Sunset Endurance" };
    public static readonly string[] Descriptions = {
        "Long straights, a fast sweep and heavy-braking chicanes",
        "Fast sweepers, flowing S bends and a technical final sector",
        "A compact street-style lap with tight traction corners",
        "A fast outfield leading into a demanding Grand Prix infield",
        "A long Grand Prix loop with heavy braking and linked corners"
    };
    public static string RecordKey(string track, string car) => "BestLap_" + track + "_" + car + "_v3";
    public static string BestTimeKey => RecordKey(SceneManager.GetActiveScene().name, CarSelection.Current.id);
    public static void Load(int index)
    {
        if (index >= 0 && index < Scenes.Length) SceneManager.LoadScene(Scenes[index]);
    }
}
