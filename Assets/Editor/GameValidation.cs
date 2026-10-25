using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class GameValidation
{
    static void Check(bool condition, string message) { if (!condition) throw new Exception(message); }
    static void Initialize(Component component, string method)
    {
        component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic).Invoke(component, null);
    }
    static void CheckScripts()
    {
        foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
            foreach (var transform in root.GetComponentsInChildren<Transform>(true))
                Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(transform.gameObject) == 0, "Missing script: " + transform.name);
    }
    [MenuItem("Tools/Racing/Validate Game")]
    public static void Run()
    {
        bool hadSelection = PlayerPrefs.HasKey(CarSelection.PreferenceKey);
        string savedSelection = PlayerPrefs.GetString(CarSelection.PreferenceKey);
        try
        {
            Check(CarSelection.All.Length == 3, "Expected three new cars");
            Check(CarSelection.All.Select(c => c.id).Distinct().Count() == 3, "Duplicate car IDs");
            PlayerPrefs.SetString(CarSelection.PreferenceKey, "invalid-selection");
            Check(CarSelection.Current.id == "sports", "Invalid selection must fall back to sports car");
            foreach (var definition in CarSelection.All)
            {
                Check(definition.straight && definition.left && definition.right, "Missing sprites: " + definition.id);
                CarSelection.Select(definition);
                foreach (string scene in LevelCatalog.Scenes)
                {
                    string path = "Assets/Scenes/" + scene + ".unity";
                    Check(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == path), "Missing build scene: " + scene);
                    EditorSceneManager.OpenScene(path);
                    CheckScripts();
                    var logic = UnityEngine.Object.FindAnyObjectByType<Logic>();
                    var car = UnityEngine.Object.FindAnyObjectByType<CarScript>();
                    Check(logic && car, "Missing race components: " + scene);
                    Check(logic.scoreText && logic.highScoreText && logic.statusText && logic.gameOverPanel, "Missing HUD");
                    var circuit = UnityEngine.Object.FindAnyObjectByType<CircuitTrack>();
                    if (circuit != null) Initialize(circuit, "Awake");
                    else Initialize(UnityEngine.Object.FindAnyObjectByType<ClassicCircuit>(), "Awake");
                    Initialize(car, "Awake"); Initialize(logic, "Start");
                    Check(car.definition == definition && car.maxSpeed == definition.maxSpeed && car.acceleration == definition.acceleration
                        && car.driftfactor == definition.driftFactor && car.turnfactor == definition.steering, "Incorrect car applied");
                    Check(car.spriteRenderer.sprite == definition.straight && car.spriteRenderer.sortingOrder == 10, "Incorrect sprite");
                    Check(car.GetComponent<BoxCollider2D>().size == definition.colliderSize, "Incorrect hitbox");
                    Physics2D.SyncTransforms();
                    Check(!Physics2D.OverlapBox(car.transform.position, definition.colliderSize * .95f, car.transform.eulerAngles.z,
                        1 << 0, 0, 10), "Car spawn overlaps track boundary: " + scene);
                    foreach (var ps in new[] { car.tireSmokeParticles1, car.tireSmokeParticles2 })
                    {
                        Check(ps.main.simulationSpace == ParticleSystemSimulationSpace.World, "Smoke must stay in world space");
                        Check(ps.main.startSize.constantMax >= .7f && ps.main.startLifetime.constantMax <= 1.1f, "Smoke dimensions");
                        Check(ps.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.isSupported, "Smoke shader unsupported");
                        Check(ps.GetComponent<ParticleSystemRenderer>().sortingOrder == 5, "Smoke sorting");
                    }
                    car.UpdateSmoke(2);
                    Check(car.tireSmokeParticles1.emission.enabled && car.tireSmokeParticles2.emission.enabled, "Drift does not emit smoke");
                    string key = logic.RecordKey;
                    bool hadRecord = PlayerPrefs.HasKey(key); float record = PlayerPrefs.GetFloat(key);
                    string legacyKey = "BestLap_" + scene; float legacy = PlayerPrefs.GetFloat(legacyKey, -1);
                    try
                    {
                        PlayerPrefs.DeleteKey(key);
                        logic.PassCheckpoint(1); Check(!logic.isTimerRunning, "Out-of-order timer start");
                        logic.PassCheckpoint(0); Check(logic.isTimerRunning, "Finish did not start timer");
                        logic.time = 12; logic.PassCheckpoint(0);
                        Check(logic.time == 12 && !PlayerPrefs.HasKey(key), "Shortcut accepted");
                        for (int gate = 1; gate < logic.checkpointCount; gate++) logic.PassCheckpoint(gate);
                        logic.PassCheckpoint(0);
                        Check(PlayerPrefs.GetFloat(key) == 12 && logic.time == 0, "Valid lap not saved");
                        logic.time = 15;
                        for (int gate = 1; gate < logic.checkpointCount; gate++) logic.PassCheckpoint(gate);
                        logic.PassCheckpoint(0); Check(PlayerPrefs.GetFloat(key) == 12, "Slower lap replaced record");
                        Check(PlayerPrefs.GetFloat(legacyKey, -1) == legacy, "Legacy record changed");
                        logic.gameOver();
                        Check(!car.alive && !logic.alive && !logic.isTimerRunning && logic.gameOverPanel.activeSelf, "Crash state");
                        Check(!car.tireSmokeParticles1.emission.enabled && !car.tireSmokeParticles2.emission.enabled, "Smoke continues after crash");
                    }
                    finally
                    {
                        if (hadRecord) PlayerPrefs.SetFloat(key, record); else PlayerPrefs.DeleteKey(key);
                    }
                    Debug.Log("VALIDATED: " + scene + " / " + definition.id);
                }
            }
            EditorSceneManager.OpenScene("Assets/Scenes/Garage.unity"); CheckScripts();
            var garage = UnityEngine.Object.FindAnyObjectByType<Garage>(); Initialize(garage, "Start");
            garage.Next(); garage.Select(); Check(CarSelection.Current == garage.PreviewCar, "Garage selection not saved");
            string chosen = CarSelection.Current.id;
            EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity"); CheckScripts();
            var menu = UnityEngine.Object.FindAnyObjectByType<Menu>(); Initialize(menu, "Start");
            Check(menu.home.activeSelf && !menu.levels.activeSelf, "Initial menu state");
            menu.Play(); Check(!menu.home.activeSelf && menu.levels.activeSelf, "Picker navigation");
            foreach (var card in UnityEngine.Object.FindObjectsByType<LevelCard>())
            { card.Refresh(); Check(card.preview.sprite == CarSelection.Current.straight, "Picker preview differs from selection"); }
            menu.ShowMainMenu(); Check(menu.home.activeSelf && !menu.levels.activeSelf, "Back navigation");
            Check(CarSelection.Current.id == chosen, "Selection lost across scenes");
            EditorSceneManager.OpenScene("Assets/Scenes/Settings.unity"); CheckScripts();
            var settings = UnityEngine.Object.FindAnyObjectByType<Settings>();
            Check(settings.MasterVolumeSlider && settings.MusicVolumeSlider && settings.SFXVolumeSlider && settings.FullscreenToggle && settings.CameraShakeToggle, "Settings references");
            Check(EditorBuildSettings.scenes.Count(s => s.enabled) == 6, "Expected six enabled scenes");
            Debug.Log("GAME VALIDATION PASSED: nine car/track combinations, garage, records and menus");
        }
        finally
        {
            if (hadSelection) PlayerPrefs.SetString(CarSelection.PreferenceKey, savedSelection);
            else PlayerPrefs.DeleteKey(CarSelection.PreferenceKey);
            PlayerPrefs.Save();
        }
    }
    public static void ValidateAndBuild() { Run(); BuildWindows(); }

    [MenuItem("Tools/Racing/Build Windows Development Player")]
    public static void BuildWindows()
    {
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
            locationPathName = "Builds/SundayCircuit.exe", target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.Development
        });
        Check(report.summary.result == BuildResult.Succeeded, "Windows build failed: " + report.summary.result);
        Debug.Log("WINDOWS DEVELOPMENT BUILD PASSED: " + report.summary.totalSize + " bytes");
    }
}
