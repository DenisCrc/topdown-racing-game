using System.Linq;
using UnityEngine;

public static class GamePreferences
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Apply()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", .5f);
        if (!System.Environment.GetCommandLineArgs().Any(a => a.StartsWith("-racing-")))
            Screen.fullScreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
    }
}
