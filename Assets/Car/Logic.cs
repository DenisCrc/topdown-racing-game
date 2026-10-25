using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
    public float time;
    public Text scoreText;
    public Text highScoreText;
    public Text statusText;
    public GameObject gameOverPanel;
    public Button restartButton;
    public bool alive = true;
    public bool isTimerRunning;
    public int checkpointCount;
    int nextCheckpoint;
    CarScript car;
    string recordKey;
    public string RecordKey => recordKey;
    void Start()
    {
        car = FindAnyObjectByType<CarScript>();
        recordKey = LevelCatalog.RecordKey(SceneManager.GetActiveScene().name, car.definition.id);
        gameOverPanel.SetActive(false);
        RefreshBest();
        scoreText.text = "0.00 s";
        statusText.text = "Cross the finish stripe to start";
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) MainMenu();
        if (Input.GetKeyDown(KeyCode.R)) restartGame();
        if (isTimerRunning && alive)
        {
            time += Time.deltaTime;
            scoreText.text = time.ToString("0.00") + " s";
        }
    }
    void RefreshBest()
    {
        float best = PlayerPrefs.GetFloat(recordKey, 0);
        highScoreText.text = best > 0 ? "BEST  " + best.ToString("0.00") + " s" : "BEST  --";
    }
    public void restartGame() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
    public void MainMenu() { SceneManager.LoadScene("MainMenu"); }
    public void gameOver()
    {
        if (!alive) return;
        alive = false;
        isTimerRunning = false;
        car.StopCar();
        gameOverPanel.SetActive(true);
        Menu.Focus(restartButton);
    }
    public void PassCheckpoint(int index)
    {
        if (!alive || checkpointCount < 2 || index != nextCheckpoint) return;
        if (index == 0)
        {
            if (isTimerRunning && time > 0 && time < PlayerPrefs.GetFloat(recordKey, float.MaxValue))
            {
                PlayerPrefs.SetFloat(recordKey, time);
                PlayerPrefs.Save();
                RefreshBest();
            }
            isTimerRunning = true;
            time = 0;
            scoreText.text = "0.00 s";
        }
        nextCheckpoint = (index + 1) % checkpointCount;
        statusText.text = nextCheckpoint == 0 ? "All gates passed - cross the finish!" : "Checkpoint " + nextCheckpoint + " / " + (checkpointCount - 1);
    }
}
