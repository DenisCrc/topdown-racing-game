using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Opt-in integration checks in development players. Never runs in normal games.
public class RacingPlayerValidation : MonoBehaviour
{
#if DEVELOPMENT_BUILD || UNITY_EDITOR
    readonly Dictionary<string, float?> savedRecords = new Dictionary<string, float?>();
    string selected;
    bool hadSelection;
    string output;
    bool failed;
    bool hadMasterVolume;
    float masterVolume;
    [Serializable] class SelectionBackup { public bool existed; public string value; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StartIfRequested()
    {
        var args = Environment.GetCommandLineArgs();
        if (!args.Contains("-racing-validation") && !args.Contains("-racing-persist-write") && !args.Contains("-racing-persist-read")) return;
        var go = new GameObject("Development Player Validation");
        DontDestroyOnLoad(go);
        go.AddComponent<RacingPlayerValidation>().Begin(args);
    }
    void Begin(string[] args)
    {
        Application.runInBackground = true;
        int index = Array.IndexOf(args, "-racing-output");
        output = index >= 0 && index + 1 < args.Length ? args[index + 1] : Path.Combine(Application.dataPath, "..", "Validation");
        Directory.CreateDirectory(output);
        string backupPath = Path.Combine(output, "selection-backup.json");
        if (args.Contains("-racing-persist-write"))
        {
            File.WriteAllText(backupPath, JsonUtility.ToJson(new SelectionBackup {
                existed = PlayerPrefs.HasKey(CarSelection.PreferenceKey), value = PlayerPrefs.GetString(CarSelection.PreferenceKey) }));
            CarSelection.Select(CarSelection.All.First(c => c.id == "rally"));
            Debug.Log("PERSISTENCE WRITE PASSED"); Application.Quit(0); return;
        }
        if (args.Contains("-racing-persist-read"))
        {
            bool correct = CarSelection.Current.id == "rally";
            var backup = JsonUtility.FromJson<SelectionBackup>(File.ReadAllText(backupPath));
            if (backup.existed) PlayerPrefs.SetString(CarSelection.PreferenceKey, backup.value); else PlayerPrefs.DeleteKey(CarSelection.PreferenceKey);
            PlayerPrefs.Save();
            Debug.Log(correct ? "PERSISTENCE RELAUNCH PASSED" : "PERSISTENCE RELAUNCH FAILED");
            Application.Quit(correct ? 0 : 1); return;
        }
        hadMasterVolume = PlayerPrefs.HasKey("MasterVolume");
        masterVolume = PlayerPrefs.GetFloat("MasterVolume", .5f);
        hadSelection = PlayerPrefs.HasKey(CarSelection.PreferenceKey);
        selected = PlayerPrefs.GetString(CarSelection.PreferenceKey);
        Application.logMessageReceived += CaptureError;
        StartCoroutine(Guarded());
    }
    void CaptureError(string text, string stack, LogType type)
    {
        if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert) failed = true;
    }
    static void Check(bool value, string reason) { if (!value) throw new Exception(reason); }
    void Backup(string key)
    {
        if (!savedRecords.ContainsKey(key)) savedRecords[key] = PlayerPrefs.HasKey(key) ? PlayerPrefs.GetFloat(key) : (float?)null;
    }
    IEnumerator Guarded()
    {
        var run = Run();
        while (true)
        {
            bool more; object next;
            try { more = run.MoveNext(); next = more ? run.Current : null; }
            catch (Exception exception) { Debug.LogException(exception); Finish(); yield break; }
            if (!more) break;
            yield return next;
        }
        Finish();
    }
    void Finish()
    {
        foreach (var pair in savedRecords)
            if (pair.Value.HasValue) PlayerPrefs.SetFloat(pair.Key, pair.Value.Value); else PlayerPrefs.DeleteKey(pair.Key);
        if (hadSelection) PlayerPrefs.SetString(CarSelection.PreferenceKey, selected); else PlayerPrefs.DeleteKey(CarSelection.PreferenceKey);
        if (hadMasterVolume) PlayerPrefs.SetFloat("MasterVolume", masterVolume); else PlayerPrefs.DeleteKey("MasterVolume");
        PlayerPrefs.Save();
        Application.logMessageReceived -= CaptureError;
        string result = failed ? "PLAYER VALIDATION FAILED" : "PLAYER VALIDATION PASSED";
        File.WriteAllText(Path.Combine(output, "result.txt"), result);
        Debug.Log(result);
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(failed ? 1 : 0);
#endif
    }
    static Button NamedButton(string name) => FindObjectsByType<Button>().Single(b => b.name == name);
    static void Submit(Button button)
    {
        EventSystem.current.SetSelectedGameObject(button.gameObject);
        ExecuteEvents.Execute(button.gameObject, new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
    }
    static void Click(Button button)
    {
        Canvas.ForceUpdateCanvases();
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, button.transform.position);
        var data = new PointerEventData(EventSystem.current) { position = position, button = PointerEventData.InputButton.Left };
        var hits = new List<RaycastResult>(); EventSystem.current.RaycastAll(data, hits);
        Check(hits.Any(h => h.gameObject == button.gameObject || h.gameObject.transform.IsChildOf(button.transform)), "Button not reachable by mouse: " + button.name);
        ExecuteEvents.Execute(button.gameObject, data, ExecuteEvents.pointerClickHandler);
    }
    void Screenshot(string name) { ScreenCapture.CaptureScreenshot(Path.Combine(output, name + ".png")); }
    IEnumerator Run()
    {
        yield return new WaitForSeconds(3);
        // Force a swap-chain resize when the process starts hidden, even if it
        // previously saved 1280x720 as its window size.
        Screen.SetResolution(800, 600, FullScreenMode.Windowed);
        yield return new WaitForSeconds(.5f);
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        yield return new WaitForSeconds(.5f);
        for (int resolution = 0; resolution < 2; resolution++)
        {
            string suffix = resolution == 0 ? "1280" : "1920";
            Screen.SetResolution(resolution == 0 ? 1280 : 1920, resolution == 0 ? 720 : 1080, FullScreenMode.Windowed);
            SceneManager.LoadScene("MainMenu"); yield return new WaitForSeconds(.35f);
            Screenshot("menu-" + suffix); yield return new WaitForSeconds(.15f);
            Click(NamedButton("Choose a circuit")); yield return new WaitForSeconds(.1f);
            Check(FindAnyObjectByType<Menu>().levels.activeSelf, "Circuit picker did not open");
            Screenshot("circuits-" + suffix); yield return new WaitForSeconds(.15f);
            Click(NamedButton("Next >")); yield return new WaitForSeconds(.1f);
            Check(FindObjectsByType<LevelCard>().Length == 2, "New circuits are missing from page two");
            Screenshot("circuits-page-two-" + suffix); yield return new WaitForSeconds(.15f);
            Submit(NamedButton("Back")); yield return null;
            Click(NamedButton("Garage")); yield return new WaitForSeconds(.25f);
            var garage = FindAnyObjectByType<Garage>();
            Check(garage != null, "Garage did not load");
            for (int i = 0; i < 3; i++)
            {
                Click(NamedButton("Next >")); yield return null;
                Submit(garage.selectButton); yield return null;
                Check(CarSelection.Current == garage.PreviewCar && garage.preview.sprite == garage.PreviewCar.straight, "Garage selection failed");
                Screenshot("garage-" + garage.PreviewCar.id + "-" + suffix); yield return new WaitForSeconds(.15f);
            }
            Click(NamedButton("Back")); yield return new WaitForSeconds(.25f);
            Click(NamedButton("Settings")); yield return new WaitForSeconds(.25f);
            var settings = FindAnyObjectByType<Settings>();
            float volume = settings.MasterVolumeSlider.value;
            settings.MasterVolumeSlider.value = .3f;
            Check(Mathf.Approximately(AudioListener.volume, .3f), "Master volume is not applied");
            settings.MasterVolumeSlider.value = volume;
            var next = settings.MasterVolumeSlider.FindSelectableOnDown();
            Check(next != null, "Settings keyboard navigation missing");
            Screenshot("settings-" + suffix); yield return new WaitForSeconds(.15f);
            Submit(settings.backButton); yield return new WaitForSeconds(.25f);
        }
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        foreach (var model in CarSelection.All)
        {
            CarSelection.Select(model);
            foreach (string level in LevelCatalog.Scenes)
            {
                string key = LevelCatalog.RecordKey(level, model.id); Backup(key); PlayerPrefs.DeleteKey(key);
                SceneManager.LoadScene(level); yield return new WaitForSeconds(.25f);
                var car = FindAnyObjectByType<CarScript>();
                var logic = FindAnyObjectByType<Logic>();
                var minimap = FindAnyObjectByType<RacingMinimap>();
                Check(minimap != null && minimap.Player == car.transform, "Minimap did not bind to current car");
                Check(minimap.route.Layout == LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, level)], "Minimap shows wrong layout");
                minimap.UpdateMarker();
                Vector2 expectedPosition = minimap.route.ProjectClamped(FindAnyObjectByType<CircuitTrack>().transform.InverseTransformPoint(car.transform.position), 7);
                Check(Vector2.Distance(minimap.marker.anchoredPosition, expectedPosition) < .01f, "Minimap marker projects incorrectly");
                Check(car.definition == model && car.alive && logic.alive, "Wrong car or unsafe spawn: " + level + "/" + model.id);
                Check(car.tireSmokeParticles1.particleCount == 0, "Smoke not clear on spawn");
                Check(car.maxSpeed == model.maxSpeed && car.acceleration == model.acceleration, "Handling differs from definition");
                car.enabled = false;
                car.myRigidbody.linearDamping = 0;
                car.UpdateSmoke(3);
                car.myRigidbody.linearVelocity = (Vector2)car.transform.up * 3 + (Vector2)car.transform.right * 0.8f;
                yield return new WaitForSeconds(.45f);
                car.myRigidbody.linearVelocity = Vector2.zero;
                Check(car.tireSmokeParticles1.particleCount > 0 && car.tireSmokeParticles2.particleCount > 0, "Smoke not visible");
                Check(car.tireSmokeParticles1.GetComponent<ParticleSystemRenderer>().sharedMaterial.shader.isSupported, "Smoke material unsupported");
                Screenshot("smoke-" + level + "-" + model.id); yield return new WaitForSeconds(.15f);
                if (model.id == "sports")
                {
                    Screen.SetResolution(1920, 1080, FullScreenMode.Windowed);
                    yield return new WaitForSeconds(.4f);
                    Screenshot("minimap-1080-" + level); yield return new WaitForSeconds(.15f);
                    Screen.SetResolution(1280, 720, FullScreenMode.Windowed); yield return new WaitForSeconds(.25f);
                }
                var gates = FindObjectsByType<RaceCheckpoint>().OrderBy(c => c.index).ToArray();
                var finish = gates.FirstOrDefault(c => c.index == 0);
                Vector2 finishPosition = finish != null ? (Vector2)finish.transform.position : (Vector2)FindAnyObjectByType<Trigger>().GetComponent<BoxCollider2D>().bounds.center;
                Vector2 finishDirection = finish != null ? (Vector2)finish.transform.up : Vector2.right;
                // Physical trigger crossings exercise direction checks and checkpoint ordering.
                for (int gateIndex = 0; gateIndex <= logic.checkpointCount; gateIndex++)
                {
                    int number = gateIndex % logic.checkpointCount;
                    Vector2 position = finishPosition, direction = finishDirection;
                    if (number != 0)
                    {
                        var gate = gates.Single(c => c.index == number);
                        position = gate.transform.position; direction = gate.transform.up;
                    }
                    car.myRigidbody.position = position - direction * 2;
                    car.myRigidbody.rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90;
                    car.myRigidbody.linearVelocity = direction * 5;
                    Physics2D.SyncTransforms();
                    yield return new WaitForSeconds(.65f);
                    car.myRigidbody.linearVelocity = Vector2.zero;
                    minimap.UpdateMarker();
                    Vector2 actualMapPosition = minimap.route.ProjectClamped(FindAnyObjectByType<CircuitTrack>().transform.InverseTransformPoint(car.transform.position), 7);
                    Check(Vector2.Distance(minimap.marker.anchoredPosition, actualMapPosition) < .01f, "Map marker stopped following car");
                    Check(logic.alive && logic.isTimerRunning, "Gate crossing failed: " + level + " / step " + gateIndex + " gate " + number + " alive=" + logic.alive + " running=" + logic.isTimerRunning + " position=" + car.myRigidbody.position);
                }
                Check(PlayerPrefs.GetFloat(key, 0) > 0, "Physical lap not recorded: " + level);
                float lap = PlayerPrefs.GetFloat(key);
                logic.PassCheckpoint(0);
                Check(PlayerPrefs.GetFloat(key) == lap, "Repeated finish altered record");
                // Strike the real boundary instead of invoking the game-over method.
                var edge = FindObjectsByType<EdgeCollider2D>().First(e => !e.isTrigger);
                Vector2 a = edge.transform.TransformPoint(edge.points[0]);
                Vector2 b = edge.transform.TransformPoint(edge.points[1]);
                Vector2 midpoint = (a + b) * .5f;
                Vector2 normal = new Vector2(-(b - a).y, (b - a).x).normalized;
                car.myRigidbody.position = midpoint + normal * 1.5f;
                car.myRigidbody.linearVelocity = -normal * 5;
                Physics2D.SyncTransforms(); yield return new WaitForSeconds(.5f);
                Check(!logic.alive && !car.alive && logic.gameOverPanel.activeSelf, "Real collision did not end run");
                Check(!car.tireSmokeParticles1.emission.enabled, "Smoke continues after crash");
                Screenshot("crash-" + level + "-" + model.id); yield return new WaitForSeconds(.15f);
                Submit(logic.restartButton); yield return new WaitForSeconds(.25f);
                Check(FindAnyObjectByType<CarScript>().definition == model && FindAnyObjectByType<Logic>().alive, "Restart lost model or run state");
                Check(FindAnyObjectByType<RacingMinimap>().Player == FindAnyObjectByType<CarScript>().transform, "Restart retained stale minimap target");
                Check(FindAnyObjectByType<CarScript>().tireSmokeParticles1.particleCount == 0, "Restart retained particles");
                FindAnyObjectByType<Logic>().MainMenu(); yield return new WaitForSeconds(.25f);
                FindAnyObjectByType<Menu>().Play();
                FindAnyObjectByType<Menu>().ShowCircuitPageFor(Array.IndexOf(LevelCatalog.Scenes, level)); yield return null;
                var card = FindObjectsByType<LevelCard>().Single(c => LevelCatalog.Scenes[c.index] == level);
                Check(card.preview.Layout == LevelCatalog.Layouts[Array.IndexOf(LevelCatalog.Scenes, level)], "Menu preview differs from raced layout");
                Check(card.record.text.Contains(lap.ToString("0.00")), "Record not shown in menu");
                Debug.Log("PLAYER VALIDATED: " + level + " / " + model.id);
            }
        }
    }
#endif
}
