using UnityEngine;
using UnityEngine.SceneManagement;

public static class LevelCatalog
{
    public static readonly string[] Scenes = { "Level1", "Level2", "Level3" };
    public static readonly string[] Names = { "Classic Circuit", "Coastal Oval", "Canyon Run" };
    public static readonly string[] Descriptions = {
        "Original circuit - long straights and sweeping turns",
        "Beginner - wide road and flowing corners",
        "Advanced - narrow road and changing-radius bends"
    };
    public static string BestTimeKey => "BestLap_" + SceneManager.GetActiveScene().name;
    public static void Load(int index)
    {
        if (index >= 0 && index < Scenes.Length)
            SceneManager.LoadScene(Scenes[index]);
    }
}
