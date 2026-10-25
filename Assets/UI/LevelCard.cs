using UnityEngine;
using UnityEngine.UI;

public class LevelCard : MonoBehaviour
{
    public int index;
    public Text title;
    public Text description;
    public Text record;
    public CircuitMapGraphic preview;
    void OnEnable() { Refresh(); }
    public void Refresh()
    {
        preview.SetLayout(LevelCatalog.Layouts[index]);
        title.text = (index + 1).ToString("00") + "  " + LevelCatalog.Names[index];
        description.text = LevelCatalog.Descriptions[index];
        float best = PlayerPrefs.GetFloat(LevelCatalog.RecordKey(LevelCatalog.Scenes[index], CarSelection.Current.id), 0);
        record.text = CarSelection.Current.displayName + "  /  " + (best > 0 ? "BEST " + best.ToString("0.00") + " s" : "SET YOUR FIRST LAP");
    }
    public void Race() { LevelCatalog.Load(index); }
}
