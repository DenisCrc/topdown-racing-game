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
    static float Cross(Vector2 a, Vector2 b, Vector2 c) => (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
    static bool Intersects(Vector2 a, Vector2 b, Vector2 c, Vector2 d)
    {
        return Cross(a, b, c) * Cross(a, b, d) < 0 && Cross(c, d, a) * Cross(c, d, b) < 0;
    }
    static void CheckRoadGeometry(CircuitTrack track)
    {
        var boundaries = track.GetComponents<EdgeCollider2D>();
        Check(boundaries.Length == 2, "Missing road boundaries");
        for (int edge = 0; edge < 2; edge++)
        {
            Vector2[] points = boundaries[edge].points;
            int count = points.Length - 1;
            for (int i = 0; i < count; i++)
                for (int j = i + 2; j < count; j++)
                {
                    if (i == 0 && j == count - 1) continue;
                    Check(!Intersects(points[i], points[i + 1], points[j], points[j + 1]), "Road boundary folds at layout " + track.layout);
                }
        }
        Vector2[] outer = boundaries[0].points, inner = boundaries[1].points;
        for (int i = 0; i < outer.Length - 1; i++)
            for (int j = 0; j < inner.Length - 1; j++)
                Check(!Intersects(outer[i], outer[i + 1], inner[j], inner[j + 1]), "Road sides overlap at layout " + track.layout);
    }
    static void CheckMapProjection()
    {
        var go = new GameObject("Map Projection Check", typeof(RectTransform));
        var map = go.AddComponent<CircuitMapGraphic>();
        try
        {
            foreach (var size in new[] { new Vector2(260, 134), new Vector2(140, 120), new Vector2(100, 300) })
            {
                map.rectTransform.sizeDelta = size;
                foreach (int layout in LevelCatalog.Layouts)
                {
                    map.SetLayout(layout);
                    Rect rect = map.rectTransform.rect;
                    for (int i = 0; i < 384; i++)
                    {
                        Vector2 p = map.Project(CircuitTrack.Point(layout, i * Mathf.PI * 2 / 384));
                        Check(p.x >= rect.xMin + map.padding - .01f && p.x <= rect.xMax - map.padding + .01f
                            && p.y >= rect.yMin + map.padding - .01f && p.y <= rect.yMax - map.padding + .01f, "Map clips circuit outline");
                    }
                    float xScale = Vector2.Distance(map.Project(Vector2.zero), map.Project(Vector2.right));
                    float yScale = Vector2.Distance(map.Project(Vector2.zero), map.Project(Vector2.up));
                    Check(Mathf.Abs(xScale - yScale) < .001f, "Map distorts circuit aspect ratio");
                    Check(Vector2.Distance(map.FinishPosition, map.Project(CircuitTrack.Point(layout, 0))) < .001f, "Finish marker mismatch");
                    Vector2 clamped = map.ProjectClamped(new Vector2(1000000, -1000000), 7);
                    Check(rect.Contains(clamped) && Mathf.Abs(clamped.x - rect.xMax + 7) < .01f, "Map marker clamp failed");
                }
            }
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
    static void CheckLiveMap(CircuitTrack track, CarScript car, Logic logic)
    {
        var hud = UnityEngine.Object.FindAnyObjectByType<RaceHud>(); Initialize(hud, "Start");
        var minimap = hud.minimap;
        Check(minimap.Player == car.transform && minimap.route.Layout == track.layout, "Minimap bound to wrong car or circuit");
        Check(minimap.GetComponent<RectTransform>().sizeDelta == new Vector2(160, 140), "Minimap panel size changed");
        Check(minimap.GetComponent<RectTransform>().anchoredPosition == new Vector2(-18, 18), "Minimap safe-area inset changed");
        Check(minimap.GetComponentsInChildren<Graphic>().All(g => !g.raycastTarget), "Minimap intercepts UI input");
        Check(minimap.transform.GetSiblingIndex() < logic.gameOverPanel.transform.GetSiblingIndex(), "Map draws above game-over overlay");
        Vector3 savedPosition = car.transform.position;
        Quaternion savedRotation = car.transform.rotation, savedCameraRotation = Camera.main.transform.rotation;
        try
        {
            Vector2 quarter = CircuitTrack.Point(track.layout, Mathf.PI / 2);
            car.transform.position = track.transform.TransformPoint(quarter);
            car.transform.up = track.transform.TransformDirection(Vector3.left);
            minimap.UpdateMarker();
            Check(Vector2.Distance(minimap.marker.anchoredPosition, minimap.route.ProjectClamped(quarter, 7)) < .01f, "Player marker position incorrect");
            Check(Vector3.Dot(minimap.marker.up, Vector3.left) > .99f, "Player marker heading incorrect");
            Vector2 before = minimap.marker.anchoredPosition;
            Quaternion heading = minimap.marker.localRotation;
            Camera.main.transform.rotation = Quaternion.Euler(0, 0, 147);
            minimap.UpdateMarker();
            Check(Vector2.Distance(before, minimap.marker.anchoredPosition) < .01f && Quaternion.Angle(heading, minimap.marker.localRotation) < .01f,
                "Camera rotation affects minimap");
            Check(Quaternion.Angle(minimap.route.transform.localRotation, Quaternion.identity) < .01f, "Circuit map must stay fixed");
        }
        finally
        {
            car.transform.SetPositionAndRotation(savedPosition, savedRotation);
            Camera.main.transform.rotation = savedCameraRotation;
        }
    }
    [MenuItem("Tools/Racing/Validate Game")]
    public static void Run()
    {
        bool hadSelection = PlayerPrefs.HasKey(CarSelection.PreferenceKey);
        string savedSelection = PlayerPrefs.GetString(CarSelection.PreferenceKey);
        try
        {
            CheckMapProjection();
            Check(LevelCatalog.Layouts.Length == LevelCatalog.Scenes.Length, "Circuit metadata mismatch");
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
                    Check(circuit != null, "Every circuit must use the common track renderer");
                    Check(circuit.layout == LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, scene)], "Scene layout differs from catalog");
                    Initialize(circuit, "Awake");
                    CheckRoadGeometry(circuit);
                    Initialize(car, "Awake"); Initialize(logic, "Start");
                    CheckLiveMap(circuit, car, logic);
                    Check(car.definition == definition && car.maxSpeed == definition.maxSpeed && car.acceleration == definition.acceleration
                        && car.driftfactor == definition.driftFactor && car.turnfactor == definition.steering, "Incorrect car applied");
                    Check(car.spriteRenderer.sprite == definition.straight && car.spriteRenderer.sortingOrder == 10, "Incorrect sprite");
                    Check(car.GetComponent<BoxCollider2D>().size == definition.colliderSize, "Incorrect hitbox");
                    Check(logic.scoreText.transform.parent.GetComponent<RectTransform>().sizeDelta.y <= 64, "HUD is too tall");
                    Check(!logic.scoreText.canvas.GetComponentsInChildren<Text>(true).Any(t => t.text.Contains("WASD")), "Race controls should be hidden");
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
            { card.Refresh(); Check(card.preview.Layout == LevelCatalog.Layouts[card.index], "Picker preview differs from circuit"); }
            menu.NextCircuitPage();
            Check(UnityEngine.Object.FindObjectsByType<LevelCard>().Length == 2, "Second page should show both new circuits");
            foreach (var card in UnityEngine.Object.FindObjectsByType<LevelCard>())
                Check(card.preview.Layout == LevelCatalog.Layouts[card.index], "Second page layout mismatch");
            foreach (var definition in CarSelection.All)
            {
                CarSelection.Select(definition);
                foreach (var card in UnityEngine.Object.FindObjectsByType<LevelCard>())
                { card.Refresh(); Check(card.preview.Layout == LevelCatalog.Layouts[card.index], "Car selection changed circuit outline"); }
            }
            PlayerPrefs.SetString(CarSelection.PreferenceKey, chosen);
            menu.PreviousCircuitPage();
            Check(UnityEngine.Object.FindObjectsByType<LevelCard>().Length == 3, "First page should show three circuits");
            menu.ShowMainMenu(); Check(menu.home.activeSelf && !menu.levels.activeSelf, "Back navigation");
            Check(CarSelection.Current.id == chosen, "Selection lost across scenes");
            EditorSceneManager.OpenScene("Assets/Scenes/Settings.unity"); CheckScripts();
            var settings = UnityEngine.Object.FindAnyObjectByType<Settings>();
            Check(settings.MasterVolumeSlider && settings.MusicVolumeSlider && settings.SFXVolumeSlider && settings.FullscreenToggle && settings.CameraShakeToggle, "Settings references");
            Check(EditorBuildSettings.scenes.Count(s => s.enabled) == 8, "Expected eight enabled scenes");
            Debug.Log("GAME VALIDATION PASSED: 15 car/track combinations, minimap projections, circuit previews, garage, records and menus");
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
