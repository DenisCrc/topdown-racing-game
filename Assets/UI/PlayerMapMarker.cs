using UnityEngine;
using UnityEngine.UI;

public class PlayerMapMarker : MaskableGraphic
{
    public PastelTheme theme;
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();
        var palette = theme != null ? theme : PastelTheme.Current;
        if (palette == null) return;
        Rect r = rectTransform.rect;
        Triangle(vertices, r.center, r.width, r.height, palette.panel);
        Triangle(vertices, r.center, r.width * .55f, r.height * .60f, palette.coral);
    }
    static void Triangle(VertexHelper vertices, Vector2 center, float width, float height, Color tint)
    {
        int start = vertices.currentVertCount;
        vertices.AddVert(center + new Vector2(0, height * .5f), tint, Vector2.zero);
        vertices.AddVert(center + new Vector2(-width * .5f, -height * .5f), tint, Vector2.zero);
        vertices.AddVert(center + new Vector2(width * .5f, -height * .5f), tint, Vector2.zero);
        vertices.AddTriangle(start, start + 1, start + 2);
    }
}
