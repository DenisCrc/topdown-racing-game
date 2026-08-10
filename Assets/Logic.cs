using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Logic : MonoBehaviour
{
    public int score = 0;
    public Text scoreText;
    public GameObject gameOverPanel;
    public bool alive = true;

    public void restartGame(){
        score = 0;
        scoreText.text = score.ToString();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void gameOver(){
        gameOverPanel.SetActive(true);
        alive = false;
    }
    public void MainMenu(){
        SceneManager.LoadScene("MainMenu");
    }
}
