using UnityEngine;
using UnityEngine.UI;

public class Logic : MonoBehaviour
{
    public int score = 0;
    public Text scoreText;
   
    public void addScore(int amount){
        score += amount;
        scoreText.text = score.ToString();
    }
}
