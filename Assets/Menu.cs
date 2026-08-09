using UnityEngine;

public class Menu : MonoBehaviour
{
    public GameObject MainMenu;
    public GameObject GameAssets;
    public GameObject Canvas;
    public GameObject ObstacolSpwn;
    void Awake()
    {
        GameAssets.SetActive(false);
        Canvas.SetActive(false);
        ObstacolSpwn.SetActive(false);
        MainMenu.SetActive(true);
    }
    public void GameStart()
    {
        GameAssets.SetActive(true);
        Canvas.SetActive(true);
        //ObstacolSpwn.SetActive(true);
        MainMenu.SetActive(false);
    }
    public void Quitgame()
    {
        Application.Quit();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created

}
