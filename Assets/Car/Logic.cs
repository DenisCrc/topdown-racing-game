using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
    public float time;
    public Text scoreText;
    public Text highScoreText;
    public GameObject gameOverPanel;
    public bool alive = true;
    public bool isTimerRunning;
    public int checkpointCount;
    private int nextCheckpoint;

    void Start()
    {
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        RefreshBest();
        if (scoreText != null) scoreText.text = "0.00";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) MainMenu();
        if (Input.GetKeyDown(KeyCode.R)) restartGame();
        if (isTimerRunning && alive)
        {
            time += Time.deltaTime;
            if (scoreText != null) scoreText.text = time.ToString("0.00");
        }
    }

    void RefreshBest()
    {
        if (highScoreText != null)
            highScoreText.text = PlayerPrefs.GetFloat(LevelCatalog.BestTimeKey, 0).ToString("0.00");
    }

    public void restartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void MainMenu() { SceneManager.LoadScene("MainMenu"); }
    public void gameOver()
    {
        if (!alive) return;
        alive = false;
        isTimerRunning = false;
        var car = FindAnyObjectByType<CarScript>();
        if (car != null)
        {
            car.alive = false;
            car.myRigidbody.linearVelocity = Vector2.zero;
            car.myRigidbody.angularVelocity = 0;
        }
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
    }

    public void PassCheckpoint(int index)
    {
        if (!alive || checkpointCount < 2 || index != nextCheckpoint) return;
        if (index == 0)
        {
            if (isTimerRunning) SaveLap();
            isTimerRunning = true;
            time = 0;
        }
        nextCheckpoint = (index + 1) % checkpointCount;
    }

    void SaveLap()
    {
        if (time > 0 && time < PlayerPrefs.GetFloat(LevelCatalog.BestTimeKey, float.MaxValue))
        {
            PlayerPrefs.SetFloat(LevelCatalog.BestTimeKey, time);
            PlayerPrefs.Save();
            RefreshBest();
        }
    }

    // Retained for the original scene's serialized finish trigger.
    public void completeLap()
    {
        PassCheckpoint(0);
    }
}
