using UnityEngine;
using UnityEngine.UI;

// Both picker previews and the live map sample the same circuit centre line.
[RequireComponent(typeof(CanvasRenderer))]
public class CircuitMapGraphic : MaskableGraphic
{
    [SerializeField] int layout;
    public PastelTheme theme;
    public float padding = 12;
    public float lineWidth = 6;
    const int Samples = 384;
    Vector2[] points;
    Vector2 sourceCenter;
    float scale;
    Rect cachedRect;
    int cachedLayout = -1;
    float cachedPadding = -1;
    public int Layout => layout;
    public Vector2 FinishPosition => Project(CircuitTrack.Point(layout, 0));

    public void SetLayout(int value)
    {
        if (layout == value) return;
        layout = value;
        SetVerticesDirty();
    }
    void EnsureProjection()
    {
        Rect rect = rectTransform.rect;
        if (points != null && cachedLayout == layout && cachedRect == rect && cachedPadding == padding) return;
        cachedLayout = layout; cachedRect = rect; cachedPadding = padding;
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue), max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 0; i < Samples; i++)
        {
            Vector2 p = CircuitTrack.Point(layout, i * Mathf.PI * 2 / Samples);
            min = Vector2.Min(min, p); max = Vector2.Max(max, p);
        }
        sourceCenter = (min + max) * .5f;
        Vector2 extent = max - min;
        scale = Mathf.Max(0, Mathf.Min((rect.width - padding * 2) / Mathf.Max(extent.x, .001f),
            (rect.height - padding * 2) / Mathf.Max(extent.y, .001f)));
        if (points == null) points = new Vector2[Samples];
        for (int i = 0; i < Samples; i++)
            points[i] = rect.center + (CircuitTrack.Point(layout, i * Mathf.PI * 2 / Samples) - sourceCenter) * scale;
    }
    public Vector2 Project(Vector2 position)
    {
        EnsureProjection();
        return cachedRect.center + (position - sourceCenter) * scale;
    }
    public Vector2 ProjectClamped(Vector2 position, float margin)
    {
        Vector2 p = Project(position);
        p.x = Mathf.Clamp(p.x, cachedRect.xMin + margin, cachedRect.xMax - margin);
        p.y = Mathf.Clamp(p.y, cachedRect.yMin + margin, cachedRect.yMax - margin);
        return p;
    }
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear(); EnsureProjection();
        var palette = theme != null ? theme : PastelTheme.Current;
        if (palette == null || scale <= 0) return;
        Ribbon(vertices, lineWidth + 2, palette.ink);
        Ribbon(vertices, lineWidth, palette.road);
        Vector2 tangent = (points[1] - points[Samples - 1]).normalized;
        Vector2 across = new Vector2(-tangent.y, tangent.x) * (lineWidth * .5f + 2);
        Segment(vertices, points[0] - across, points[0] + across, 4, palette.ink);
        Segment(vertices, points[0] - across, points[0] + across, 2, palette.panel);
    }
    void Ribbon(VertexHelper vertices, float width, Color tint)
    {
        int start = vertices.currentVertCount;
        for (int i = 0; i < Samples; i++)
        {
            Vector2 tangent = (points[(i + 1) % Samples] - points[(i + Samples - 1) % Samples]).normalized;
            Vector2 normal = new Vector2(-tangent.y, tangent.x) * width * .5f;
            vertices.AddVert(points[i] - normal, tint, Vector2.zero);
            vertices.AddVert(points[i] + normal, tint, Vector2.zero);
        }
        for (int i = 0; i < Samples; i++)
        {
            int a = start + i * 2, b = start + (i + 1) % Samples * 2;
            vertices.AddTriangle(a, b, a + 1); vertices.AddTriangle(a + 1, b, b + 1);
        }
    }
    static void Segment(VertexHelper vertices, Vector2 a, Vector2 b, float width, Color tint)
    {
        Vector2 direction = (b - a).normalized;
        Vector2 normal = new Vector2(-direction.y, direction.x) * width * .5f;
        int start = vertices.currentVertCount;
        vertices.AddVert(a - normal, tint, Vector2.zero); vertices.AddVert(a + normal, tint, Vector2.zero);
        vertices.AddVert(b + normal, tint, Vector2.zero); vertices.AddVert(b - normal, tint, Vector2.zero);
        vertices.AddTriangle(start, start + 1, start + 2); vertices.AddTriangle(start, start + 2, start + 3);
    }
}
