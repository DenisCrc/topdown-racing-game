using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelCatalog
{
    public static readonly string[] Scenes = { "Level1", "Level2", "Level3", "Level4", "Level5" };
    public static readonly string[] Names = { "Classic Circuit", "Coastal Oval", "Canyon Run", "Highland Grand Prix", "Sunset Endurance" };
    public static readonly string[] Descriptions = {
        "A flowing circuit with wide, sweeping bends",
        "A wide, flowing road to find your rhythm",
        "Changing-radius bends & a narrower racing line",
        "A long, wide grand prix loop with fast sweepers",
        "An expansive endurance course with three flowing sectors"
    };
    public static string RecordKey(string track, string car) => "BestLap_" + track + "_" + car + (track == "Level1" ? "_v2" : "");
    public static string BestTimeKey => RecordKey(SceneManager.GetActiveScene().name, CarSelection.Current.id);
    public static void Load(int index)
    {
        if (index >= 0 && index < Scenes.Length) SceneManager.LoadScene(Scenes[index]);
    }
}
