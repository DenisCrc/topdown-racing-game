using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class GameValidation
{
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
    }

    static void Initialize(Component component, string method)
    {
        component.GetType().GetMethod(method, BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(component, null);
    }

    [MenuItem("Tools/Racing/Validate Game")]
    public static void Run()
    {
        foreach (string scene in LevelCatalog.Scenes)
        {
            string path = "Assets/Scenes/" + scene + ".unity";
            Check(EditorBuildSettings.scenes.Any(s => s.enabled && s.path == path), "Missing build scene: " + scene);
            EditorSceneManager.OpenScene(path);
            var logic = UnityEngine.Object.FindAnyObjectByType<Logic>();
            var car = UnityEngine.Object.FindAnyObjectByType<CarScript>();
            Check(logic != null && car != null, "Missing race components: " + scene);
            Check(logic.scoreText && logic.highScoreText && logic.gameOverPanel, "Missing HUD: " + scene);
            Initialize(car, "Awake");
            var circuit = UnityEngine.Object.FindAnyObjectByType<CircuitTrack>();
            if (circuit != null)
            {
                Initialize(circuit, "Awake");
                Check(circuit.GetComponents<EdgeCollider2D>().Length == 2, "Road boundaries missing");
                var mesh = circuit.GetComponent<MeshFilter>().sharedMesh;
                Check(mesh != null && mesh.vertexCount == 768, "Road mesh missing");
                Check(!circuit.GetComponents<EdgeCollider2D>().Any(c => c.OverlapPoint(car.transform.position)), "Car starts on a barrier");
            }
            else
            {
                Initialize(UnityEngine.Object.FindAnyObjectByType<ClassicCircuit>(), "Awake");
                Check(GameObject.Find("Level1").GetComponent<MeshFilter>().sharedMesh != null, "Classic road mesh missing");
            }
            Initialize(logic, "Start");
            Check(!logic.gameOverPanel.activeSelf, "Game over panel visible at start");
            string key = LevelCatalog.BestTimeKey;
            bool hadRecord = PlayerPrefs.HasKey(key);
            float savedRecord = PlayerPrefs.GetFloat(key);
            try
            {
                PlayerPrefs.DeleteKey(key);
                logic.PassCheckpoint(1);
                Check(!logic.isTimerRunning, "Out-of-order checkpoint started timer");
                logic.PassCheckpoint(0);
                Check(logic.isTimerRunning, "Finish did not start timer");
                logic.time = 12;
                logic.PassCheckpoint(0);
                Check(logic.time == 12 && !PlayerPrefs.HasKey(key), "Finish shortcut accepted");
                for (int i = 1; i < logic.checkpointCount; i++) logic.PassCheckpoint(i);
                logic.PassCheckpoint(0);
                Check(PlayerPrefs.GetFloat(key) == 12 && logic.time == 0, "Valid lap not recorded");
                logic.time = 15;
                for (int i = 1; i < logic.checkpointCount; i++) logic.PassCheckpoint(i);
                logic.PassCheckpoint(0);
                Check(PlayerPrefs.GetFloat(key) == 12, "Slower lap replaced record");
                logic.gameOver();
                Check(!logic.alive && !car.alive && !logic.isTimerRunning && logic.gameOverPanel.activeSelf, "Crash state incorrect");
            }
            finally
            {
                if (hadRecord) PlayerPrefs.SetFloat(key, savedRecord);
                else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
            }
            Debug.Log("VALIDATED: " + scene);
        }
        EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");
        var menu = UnityEngine.Object.FindAnyObjectByType<Menu>();
        Initialize(menu, "Start");
        Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b => b.gameObject.activeInHierarchy) == 3, "Main menu actions missing");
        menu.Play();
        Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b => b.gameObject.activeInHierarchy) == 4, "Level picker actions missing");
        menu.ShowMainMenu();
        Check(UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Count(b => b.gameObject.activeInHierarchy) == 3, "Back navigation failed");
        Debug.Log("VALIDATED: menu navigation; GAME VALIDATION PASSED");
    }
}
