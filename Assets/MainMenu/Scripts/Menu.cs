using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    private RectTransform content;
    private Font font;

    void Start()
    {
        AudioListener.volume = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        var canvas = GetComponent<Canvas>();
        font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        foreach (Transform child in canvas.transform)
            child.gameObject.SetActive(false);
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280, 720);
        scaler.matchWidthOrHeight = 0.5f;
        var panel = new GameObject("Menu Content", typeof(RectTransform), typeof(Image));
        content = panel.GetComponent<RectTransform>();
        content.SetParent(canvas.transform, false);
        content.anchorMin = Vector2.zero;
        content.anchorMax = Vector2.one;
        content.offsetMin = content.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.035f, 0.06f, 0.10f);
        ShowMainMenu();
    }

    void Clear()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i).gameObject;
            child.SetActive(false);
            if (Application.isPlaying) Destroy(child);
            else DestroyImmediate(child);
        }
    }

    void Label(string value, float y, int size)
    {
        var go = new GameObject(value, typeof(RectTransform), typeof(Text));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(content, false);
        rect.sizeDelta = new Vector2(1120, 55);
        rect.anchoredPosition = new Vector2(0, y);
        var text = go.GetComponent<Text>();
        text.font = font;
        text.text = value;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        text.raycastTarget = false;
    }

    Button AddButton(string label, float y, UnityEngine.Events.UnityAction action)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        var rect = go.GetComponent<RectTransform>();
        rect.SetParent(content, false);
        rect.sizeDelta = new Vector2(700, 64);
        rect.anchoredPosition = new Vector2(0, y);
        go.GetComponent<Image>().color = new Color(0.10f, 0.28f, 0.38f);
        var button = go.GetComponent<Button>();
        button.onClick.AddListener(action);
        var textGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        var textRect = textGo.GetComponent<RectTransform>();
        textRect.SetParent(rect, false);
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textRect.offsetMax = Vector2.zero;
        var text = textGo.GetComponent<Text>();
        text.font = font;
        text.text = label;
        text.fontSize = 24;
        text.color = Color.white;
        text.alignment = TextAnchor.MiddleCenter;
        text.raycastTarget = false;
        return button;
    }

    void Select(Button button)
    {
        if (Application.isPlaying && EventSystem.current != null)
            EventSystem.current.SetSelectedGameObject(button.gameObject);
    }

    public void ShowMainMenu()
    {
        Clear();
        Label("TOP-DOWN RACING", 230, 46);
        Label("Choose a circuit. Find your fastest lap.", 165, 22);
        var play = AddButton("Play - Select Circuit", 55, Play);
        AddButton("Settings", -35, LoadSettings);
        AddButton("Quit", -125, Quit);
        Label("WASD / Arrow keys to drive | Esc to return to menu", -260, 20);
        Select(play);
    }

    public void Play()
    {
        Clear();
        Label("SELECT CIRCUIT", 260, 40);
        Button first = null;
        for (int i = 0; i < LevelCatalog.Scenes.Length; i++)
        {
            int index = i;
            float best = PlayerPrefs.GetFloat("BestLap_" + LevelCatalog.Scenes[i], 0);
            string record = best > 0 ? "  |  Best: " + best.ToString("0.00") + "s" : "  |  No record";
            var button = AddButton((i + 1) + ". " + LevelCatalog.Names[i] + record,
                160 - i * 140, () => LevelCatalog.Load(index));
            Label(LevelCatalog.Descriptions[i], 108 - i * 140, 19);
            if (first == null) first = button;
        }
        AddButton("Back", -285, ShowMainMenu);
        Select(first);
    }

    public void Quit() { Application.Quit(); }
    public void LoadSettings() { SceneManager.LoadScene("Settings"); }
}
