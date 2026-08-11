using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Logic : MonoBehaviour
{
    public float time = 0f;
    public Text scoreText;
    public Text highScoreText;
    public GameObject gameOverPanel;
    public bool alive = true;
    
    public bool isTimerRunning = false; 

    void Awake()
    {
        if (PlayerPrefs.HasKey("HighScore"))
        {
            highScoreText.text = PlayerPrefs.GetFloat("HighScore").ToString("0.00");
        }
        else
        {
            highScoreText.text = "0.00";
        }
    }
    
    void Update()
    {
        if (isTimerRunning && alive)
        {
            time += Time.deltaTime;
            scoreText.text = time.ToString("0.00");
        }
    }

    public void restartGame()
    {
        time = 0f;
        scoreText.text = time.ToString("0.00");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver()
    {
        gameOverPanel.SetActive(true);
        alive = false;
        isTimerRunning = false; 
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void completeLap()
    {
        
        if(!isTimerRunning)
        {
            isTimerRunning = true;
        }
        else if(time > 3f)
        {
            if (time < PlayerPrefs.GetFloat("HighScore", float.MaxValue))
            {
                PlayerPrefs.SetFloat("HighScore", time);
                highScoreText.text = time.ToString("0.00");
            }
            time = 0f;
        }
    }
}