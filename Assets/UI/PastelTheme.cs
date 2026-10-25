using UnityEngine;

[CreateAssetMenu(menuName = "Racing/Pastel Theme")]
public class PastelTheme : ScriptableObject
{
    public Color background = new Color32(210, 180, 140, 255);
    public Color panel = new Color32(250, 240, 220, 255);
    public Color ink = new Color32(71, 53, 44, 255);
    public Color accent = new Color32(172, 193, 166, 255);
    public Color coral = new Color32(215, 153, 133, 255);
    public Color blue = new Color32(165, 188, 197, 255);
    public Color road = new Color32(148, 132, 115, 255);
    public Font font;
    public static PastelTheme Current => Resources.Load<PastelTheme>("PastelTheme");
}
